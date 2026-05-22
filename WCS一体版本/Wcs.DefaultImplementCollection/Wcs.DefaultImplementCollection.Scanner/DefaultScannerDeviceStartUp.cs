using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Scanner
{
    /// <summary>
    /// 湖北玉立 固定式扫码启动程序
    /// <startup type="湖北玉立.固定式扫码启动程序, 湖北玉立"/>
    /// </summary>
    public class DefaultScannerDeviceStartUp : IApplicationStartup
    {
        Random source = new Random();
        Logger _logger;
        //ConveyorDevice conveyorDevice;
        Wcs.Framework.Cfg.StartupElement _element;
        List<String> _addContainerCodeToTask;
        //List<String> _requestToWMS;                      

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            this._logger = LogManager.GetCurrentClassLogger();
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _addContainerCodeToTask = _element.GetAttributeOrDefault<String>("addContainerCodeToTask", "").Split(',').ToList();
            // _requestToWMS = _element.GetAttributeOrDefault<String>("requestToWMS", "").Split(',').ToList();

            var scanerDevices = Wcs.Framework.Cfg.WcsConfiguration.Instance
                  .DeviceCollection.ParticularDeviceCollection
                  .SelectMany(x => x.DeviceElements).Where(x => x.Device is ScanerDevice)
                  .Select(x => x.Device as ScanerDevice);

            foreach (var item in scanerDevices)
            {
                _logger.Trace(String.Format("订阅 {0} 的条码事件", item), this);
                item.BarcodeReceived += device_BarcodeReceived;
            }
        }

        private void device_BarcodeReceived(object sender, BarcodeReceivedArgs e)
        {
            ushort result = 1;
            ScanerDevice scanerDevice = sender as ScanerDevice;
            if (scanerDevice == null)
            {
                _logger.Warn1(String.Format("{0} 不是条码扫描设备。", sender), this);
                return;
            }

            if (e.Barcode.Trim() == "" || e.Barcode.Contains("?") || e.Barcode.ToUpper() == "NOREAD")
            {
                result = 2;
            }

            _logger.Info(String.Format("收到了 {0} 条码扫描事件 {1} 的事件", sender, e.Barcode), this);
            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
                    String.Format("条码扫描 {0}", scanerDevice.Name),
                    String.Format("收到了 {0} 条码扫描事件 {1} 的事件", sender, e.Barcode),
                    null));

            try
            {
                ConveyorLocation location = (ConveyorLocation)LocationConverter.ConvertibleCodeToLcation(scanerDevice.BindingLocation);
                ConveyorDevice conveyor = (ConveyorDevice)location.Device;
                if (_addContainerCodeToTask.Contains(location.DeviceCode))
                    固定式条码扫描(conveyor, location, e);
            }
            catch (Exception ex)
            {
                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                        new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                        String.Format("条码扫描 {0}", scanerDevice.Name),
                        String.Format("收到了 {0} 条码扫描事件 {1} 的事件,但是在处理过程中发生异常，异常信息：{2}", sender, e.Barcode, ex.Message),
                        null));
                _logger.Error1(ex, this);
            }
        }

        private Boolean 固定式条码扫描(ConveyorDevice conveyor, ConveyorLocation location, BarcodeReceivedArgs e)
        {
            if (conveyor.TaskBlocks == null || conveyor.TaskBlocks.Count() == 0 || conveyor.LocationInfoBlocks == null || conveyor.LocationInfoBlocks.Count() == 0)
            {
                _logger.Warn1(String.Format("位置 {0} 接收到条码值 {1}，但其所在输送线 任务列表/货位当前任务 获取异常，处理失败", location.DeviceCode, e.Barcode), this);

                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Warning,
                        String.Format("扫码({0})", location.DeviceCode),
                        String.Format("位置 {0} 接收到条码值 {1}，但当前所在输送线 任务列表/货位当前任务 获取异常，处理失败", location.DeviceCode, e.Barcode),
                        null));

                _logger.Error(String.Format("位置 {0} 接收到条码值 {1}，但当前所在输送线 任务列表/货位当前任务 获取异常，处理失败", location.DeviceCode, e.Barcode));
                return false;
            }

            var equipmentTaskId = conveyor.LocationInfoBlocks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(location.DeviceCode)).TaskNo;
            TaskBlock taskNetTransferObject;
            if (equipmentTaskId == 0)
                taskNetTransferObject = conveyor.TaskBlocks.FirstOrDefault(x => x.From == Convert.ToUInt16(location.DeviceCode) && x.TaskState == TaskBlockTaskStatus.Finished);
            else
                taskNetTransferObject = conveyor.TaskBlocks.FirstOrDefault(x => x.TaskNo == equipmentTaskId);

            if (taskNetTransferObject == null)
            {
                string message = String.Format("位置 {0} 收到条码 {1},但是未在PLC中找到对应的任务，系统判断为测试扫码，忽略。", location.DeviceCode, e.Barcode);
                _logger.Warn(message, this);
                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                    new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Warning,
                        String.Format("扫码({0})", location.DeviceCode),
                        message,
                        null));
                return false;
            }

            Boolean _handResult = false;
            Task _oldTask = null;
            Task _newTask = null;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var action = unitOfWork.session.Query<ConveyorTransferAction>().FirstOrDefault(x =>
                                x.EquipmentTaskId == taskNetTransferObject.TaskNo
                                && x.Status != EquipmentActionStatus.Cancelled
                                && x.Status != EquipmentActionStatus.Error
                                );
                if (action == null)
                {
                    ///这里可能需要增加处理
                    unitOfWork.Commit();
                    _handResult = false;
                }
                else
                {
                    _newTask = action.Movement.Task;
                    //_oldTask = (Task)_newTask.Clone();

                    _newTask.ContainerCodes.Add(e.Barcode.Trim());

                    unitOfWork.session.SaveOrUpdate(_newTask);
                    unitOfWork.Commit();
                    _handResult = true;
                }

            }
            //if (_handResult)
            //    Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskUpdateEvent(_oldTask, _newTask));

            return _handResult;

            #region 根据任务绑定
            //using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            //{
            //    try
            //    {
            //        var tasks = unitOfWork.session.Query<Task>().Where(x =>
            //                x.EndLocation.DeviceCode == location.DeviceCode
            //                && x.CurrentLocation.DeviceCode == location.DeviceCode
            //                ).ToList();


            //        if (tasks == null)
            //        {
            //            msg = String.Format("未找任务结束点是位置 {0}并且任务当前位置是 {1}的任务，系统判断为测试扫码，", location.DeviceCode, location.DeviceCode);

            //            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
            //                new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Warning,
            //                String.Format("扫码({0})", location.DeviceCode),
            //                    msg,
            //                    null));
            //            unitOfWork.Commit();
            //            throw new Exception(msg);
            //        }

            //        if (tasks.Count() > 1)
            //        {
            //            msg = String.Format("找 {0} 条任务结束点是位置 {1} 并且任务当前位置是 {2} 的任务，系统判断为测试扫码，", tasks.Count(), location.DeviceCode, location.DeviceCode);

            //            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
            //                new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Warning,
            //                String.Format("扫码({0})", location.DeviceCode),
            //                    msg,
            //                    null));
            //            unitOfWork.Commit();
            //            throw new Exception(msg);
            //        }

            //        var task = tasks.First();
            //        task.AdditionalInfo.Add("验证扫码", e.Barcode.Trim());

            //        unitOfWork.session.Update(task);
            //        unitOfWork.Commit();
            //        Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskStatusChangedEvent(task.Id, task.TaskCode, task.Status, task.BizType, task.Source, task.TaskType));

            //        msg = String.Format("成功为任务 {0} 添加附加属性 验证扫码={1}", task.TaskCode, e.Barcode);
            //        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
            //            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
            //                String.Format("扫码({0})", location.DeviceCode),
            //                msg,
            //                null));
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        unitOfWork.Commit();
            //        throw new Exception(ex.Message);
            //    }
            //}
            #endregion
        }

        void 写扫码结果(ConveyorDevice conveyorDevice, String deviceCode, String barcode, UInt16 result)
        {
            Random source = new Random();
            Boolean isAlReadyOutPut = false;
            int i = 0;
            while (i < 5)
            {
                try
                {
                    var data_id = Convert.ToUInt32(source.Next(1, 2147483647));
                    var scanerTransferObject = conveyorDevice.ReadStatus<ScanerTransferObject>().Where(x => x.ScanerNo.ToString() == deviceCode).FirstOrDefault();
                    if (scanerTransferObject == null)
                    {
                        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                                String.Format("条码扫描{0}", deviceCode),
                                String.Format("获取位置 {0} 的扫码结果为 NULL，系统默认成功发送结果 {1} 写入成功，不再发送扫码结果", deviceCode, result),
                                null));
                        return;
                    }
                    ScanerResultCommand cmd = new ScanerResultCommand(1, 1, result, Convert.ToUInt16(scanerTransferObject.AtPacketIndex), data_id);

                    conveyorDevice.Write(cmd, (device, _cmd) =>
                    {
                        if (conveyorDevice.IsConnected)
                        {
                            return conveyorDevice.ReadStatus<ScanerTransferObject>().Where(x =>
                                x.ScanerNo.ToString() == deviceCode
                                && x.ScanResult == result)
                                .FirstOrDefault() != null;
                        }
                        else
                        {
                            _logger.Warn1(String.Format("写{0}({1})扫码结果 {2} 由于 {3} 连接异常，默认成功。", deviceCode, barcode, result, conveyorDevice.Name), this);
                            return true;
                        }
                    });

                    return;
                }
                catch (Exception ex)
                {
                    i++;

                    _logger.Error1(new Exception(String.Format("写{0}({1})扫码结果 {2} 失败", deviceCode, barcode, result), ex), this);

                    if (!isAlReadyOutPut)
                    {
                        Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                                String.Format("条码扫描{0}", deviceCode),
                                "写扫码结果成功信号失败" + ex.Message,
                                null));
                        isAlReadyOutPut = true;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public override string ToString()
        {
            return "固定式扫码启动程序";
        }
    }
}