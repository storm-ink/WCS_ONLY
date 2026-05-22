using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;
using NHibernate.Linq;

namespace BOE
{
    public class WMSTaskCompeletedHandler_出库 : WMSTaskCompeletedExternalHandler
    {
        const String _key = "realLocation";
        List<String> _ends = new List<string>() { "00-001-2018", "00-001-1011" };
        public override bool Allowed(Wcs.Framework.Task task)
        {
            if (!_ends.Contains(task.EndLocation.UserCode))
                return false;
            if (task.AdditionalInfo.ContainsKey(_key))
                return false;
            return base.Allowed(task);
        }

        public override void Hand(ref Wcs.Framework.Task task)
        {
            if (!_ends.Contains(task.EndLocation.UserCode))
                return;
            if (task.AdditionalInfo.ContainsKey(_key))
                return;

            List<String> canSelected = null;
            if (task.EndLocation.UserCode == "00-001-1011")
                canSelected = new List<string>() { "00-001-1015", "00-001-1019" };
            else if (task.EndLocation.UserCode == "00-001-2018")
                canSelected = new List<string>() { "00-001-2022", "00-001-2020" };

            if (canSelected == null)
                throw new Exception("获取可选终点失败");

            Location end;
            Dictionary<string, ActionSchedulerFilterResult> _assignResult = new Dictionary<string, ActionSchedulerFilterResult>();
            foreach (var item in canSelected)
            {
                var _result = IsCanAssign(item, out end);
                _assignResult.Add(item, _result);
                Console.WriteLine($"位置{task.EndLocation.UserCode}尝试分配到位置{item}:{_result.Defeated},{_result.Reason}");
                if (_result.Defeated && end != null)
                {
                    var _end = LocationConverter.ToLocationInfo(end);
                    var _taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                    Task _task = new Task(_taskCode, task.EndLocation, _end);
                    _task.Source = TaskSource.Wcs;
                    _task.TaskType = task.TaskType + "_WCS";
                    _task.Description = task.TaskCode;
                    _task.ContainerCodes.AddAll(task.ContainerCodes);
                    foreach (var _addtionalinfo in task.AdditionalInfo)
                    {
                        _task.AdditionalInfo.Add(_addtionalinfo);
                    }
                    task.AdditionalInfo.Add(_key, _end.UserCode);

                    using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope())
                    {
                        try
                        {
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                            {
                                unitOfWork.session.SaveOrUpdate(task);
                                unitOfWork.session.Save(_task);
                                unitOfWork.Commit();
                            }

                            tsc.Complete();
                        }
                        catch (Exception ex)
                        {
                            tsc.Dispose();
                            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                                new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Error,
                                    "出库口随机分配",
                                    $"结束点是{task.EndLocation.DeviceCode}的任务{task},在选择出库口时发生异常，异常消息{ex}",
                                    null));
                            throw ex;
                        }
                    }

                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                        new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Info,
                            "出库口随机分配",
                            $"结束点是{task.EndLocation.DeviceCode}的任务{task},成功分配出库口{item}",
                            null));

                    Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.Framework.Events.TaskAddedEvent>(new Wcs.Framework.Events.TaskAddedEvent(_task));
                    return;
                }
            }

            string _str = (_assignResult == null || _assignResult.Count() == 0) ? "未获取分配结果" : String.Join("/", _assignResult.Select(x => x.Key + "_" + x.Value.Defeated + "_" + x.Value.Reason));
            Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Warning,
                    "出库口随机分配",
                    $"结束点是{task.EndLocation.DeviceCode}的任务{task}，分配出口失败，等待下次分配，本次分配结果：{_str}",
                    null));

            throw new Exception(String.Format("分配终点 {0} 失败", String.Join("/", canSelected)));
        }

        /// <summary>
        /// 判断位置是否可以分配
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        private ActionSchedulerFilterResult IsCanAssign(String usercode, out Location end)
        {
            end = null;
            var _end = (ConveyorLocation)LocationConverter.UserCodeToLcation(usercode);
            var _device = (ConveyorDevice)_end.Device;

            if (usercode == "00-001-2020")
            {
                var _2020Workmodle = Helper.Get2020WorkModel(_end);
                if (_2020Workmodle != _2020WorkModel._出库模式)
                    return new ActionSchedulerFilterResult(false, "2020非出库模式");
            }

            if (!_device.IsConnected)
                return new ActionSchedulerFilterResult(false, $"设备{_device.Name}未连接");

            var _locationState = _device.ConveyorLocationStates.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(_end.DeviceCode) && x.Status != LocationNetTransferObjectStatus.Offline && x.Status != LocationNetTransferObjectStatus.Manual);
            if (_locationState == null)
                return new ActionSchedulerFilterResult(false, $"位置 {_end.DeviceCode} 在离线或者手动状态");

            var _occupyState = _device.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(_end.DeviceCode) && !x.AftPosPotocell && !x.AftProPotocell && !x.AftSloPotocell && !x.FroPosPotocell && !x.FroProPotocell && !x.FroSloPotocell);
            if (_occupyState == null)
                return new ActionSchedulerFilterResult(false, $"位置 {_end.DeviceCode} 至少有一个光电有信号");

            var _locationTask = _device.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(_end.DeviceCode) && x.TaskNo == 0);
            if (_locationTask == null)
                return new ActionSchedulerFilterResult(false, $"位置 {_end.DeviceCode} 货位任务号不为0");

            if (usercode == "00-001-1015")
            {
                Location _loc = LocationConverter.UserCodeToLcation("00-001-1014");
                ConveyorDevice dev = (ConveyorDevice)_loc.Device;
                if(dev.OccupyStatus.Any(x => x.PosNo == 1014 && (x.AftPosPotocell || x.AftProPotocell || x.FroPosPotocell || x.AftSloPotocell)))
                {
                   return new ActionSchedulerFilterResult(false, "位置 1014 有一个光电有信号");
                }

                if( dev.LocationCurrentTasks.Any(x => x.PosNo == 1014 && x.TaskNo > 0))
                {
                   return new ActionSchedulerFilterResult(false, "位置 1014v货位任务号不为0");
                }
            }

            
            if (usercode == "00-001-1019")
            {
                Location _loc = LocationConverter.UserCodeToLcation("00-001-1018");
                ConveyorDevice dev = (ConveyorDevice)_loc.Device;
                if(dev.OccupyStatus.Any(x => x.PosNo == 1018 && (x.AftPosPotocell || x.AftProPotocell || x.FroPosPotocell || x.AftSloPotocell)))
                {
                   return new ActionSchedulerFilterResult(false, "位置 1018 有一个光电有信号");
                }

                if(dev.LocationCurrentTasks.Any(x => x.PosNo == 1018 && x.TaskNo > 0))
                {
                   return new ActionSchedulerFilterResult(false, "位置 1018货位任务号不为0");
                }
            }

            String __end = string.Empty;
            if (usercode == "00-001-1015")
                __end = "00-001-1014";
            if (usercode == "00-001-1019")
                __end = "00-001-1018";
            if (usercode == "00-001-2020")
                __end = "00-001-2016";
            if (usercode == "00-001-2022")
                __end = "00-001-2019";

            Boolean _tsk, _otherTask = false;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                _tsk = unitOfWork.session.Query<Task>().Any(x => 
                        x.EndLocation.UserCode == usercode 
                        && x.Status != TaskStatus.Cancelled 
                        && x.Status != TaskStatus.Completed);
                if (!string.IsNullOrEmpty(__end))
                    _otherTask = unitOfWork.session.Query<Task>().Any(x =>
                            x.StartLocation.UserCode == usercode
                            && x.EndLocation.UserCode == __end);
                unitOfWork.Commit();
            }
            if (_tsk)
                return new ActionSchedulerFilterResult(false, $"至少存在一条结束位置是 {_end.DeviceCode} 并且状态不为取消或者完成状态");
            if (!String.IsNullOrEmpty(__end) && _otherTask)
                return new ActionSchedulerFilterResult(false, $"至少存在一个起点是{usercode}终点是{__end}的任务");

            end = _end;
            return new ActionSchedulerFilterResult(true, "");
        }
    }
}
