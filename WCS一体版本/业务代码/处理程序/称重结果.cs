using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using Wcs.DefaultImpls.Conveyor;
using NHibernate.Linq;

namespace BOE
{
    /// <summary>
    /// 用于输送线任务报完成前读取称重结果
    /// </summary>
    public class 称重结果 : ITaskEventHandler<TaskCompletedEventArgs>
    {
        String[] _endLocations = new String[2] {  "1001", "1004" };
        //String[] _startLocations = new String[2] { "3025", "3020" };

        Dictionary<String, String> _locations = new Dictionary<string, string>() { { "1001", "1001" }, { "1004", "1004" } };

        Logger _logger;

        public 称重结果()
        {
            _logger = LogManager.CreateNullLogger();
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is ConveyorDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(ConveyorDevice), this);
                    _logger.Debug(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }

                _logger.Trace(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);

                var action = session.Query<ConveyorTransferAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(ConveyorDevice));
                    args.Handled = true;
                    _logger.Warn(msg, this, args, null, args.EquipmentTaskId);
                    return;
                }


                if (!_endLocations.Contains(action.EndLocation.DeviceCode))
                {
                    String msg = String.Format("收到了 {0} 发送来的 {1} 任务完成信号，但是其起点位置 {2} 不是入货口位置，忽略本信号", device, args.EquipmentTaskId, action.StartLocation.DeviceCode);
                    args.Handled = true;
                    _logger.Trace(msg, this);
                    return;
                }

                if (action.Movement.Task.AdditionalInfo.ContainsKey("重量检测"))
                {
                    String msg = String.Format("收到了 {0} 发送来的 {1} 任务完成信号，已经添加外形检测结果，忽略本信号", device, args.EquipmentTaskId);
                    args.Handled = true;
                    _logger.Trace(msg, this);
                    return;
                }


                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>(action.DeviceName);
                //if (!conveyor.IsConnected || conveyor.ReadStatus<ShapeCheckTransferObject>() == null || conveyor.ReadStatus<ShapeCheckTransferObject>().Count() == 0)
                //{
                //    args.Handled = false;
                //    return;
                //}
                var weightDevice = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection
                                                              .ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                                                              .Where(x => x.Device is WeightDevice)
                                                              .Select(x => x.Device as WeightDevice)
                                                              .SingleOrDefault(x => x.BindingLocation == LocationConverter.ToLocation(action.Movement.Task.EndLocation).ToConvertibleCode());


                // var shapeCheck = conveyor.ReadStatus<ShapeCheckTransferObject>().FirstOrDefault(x => x.ShapeCheckNO == Convert.ToUInt16(_locations[action.EndLocation.DeviceCode]) && x.ShapeStatus != 0);
                if (weightDevice == null)
                {
                    args.Handled = false;
                    _logger.Warn("获取称重信息失败,等待下一次获取", this);
                    return;
                }

                decimal weight = 0;
                if (weightDevice.IsConnected)
                    weight = weightDevice.CurrentWeight;
                else
                {
                    //if (action.Movement.Task.AdditionalInfo.Keys.Contains("重量检测"))
                    weight = Convert.ToDecimal(action.Movement.Task.AdditionalInfo["重量检测"]);
                }
                action.Movement.Task.AdditionalInfo.Add("重量检测", weight.ToString());
                session.Update(action);

                //if (shapeCheck.Warnings.Count() != 0 && action.StartLocation.DeviceCode == "1021")
                // {
                //     action.Movement.Task.Description = String.Join(",", shapeCheck.Warnings);
                //     session.Update(action);
                // }
                // else
                // {
                //foreach (var item in shapeCheck)
                //{

                //}

                //var weight = NXNGHelper.GetWeight(LocationConverter.ToLocation(action.EndLocation));
                //action.Movement.Task.AdditionalInfo.Add("重量", weight);

                //session.Update(action);
                // }

                args.Handled = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, this);
                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                        new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                        "ConveyorTaskCompletedHandler_HNXX",
                        ex.Message,
                        null));
                args.Handled = false;
                return;
            }
        }

        private Dictionary<String, String> GetShapeCheckResult(ConveyorDevice conveyor, ConveyorTransferAction action)
        {
            var shapeCheck = conveyor.ReadStatus<ShapeCheckTransferObject>().FirstOrDefault(x => x.ShapeCheckNO == Convert.ToUInt16(action.StartLocation.DeviceCode));
            var result = shapeCheck.GetResult;
            Console.WriteLine(result);

            return result;
        }

    }
}
