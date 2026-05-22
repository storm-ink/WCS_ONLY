using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class DefaultCraneEquipmentActionScheduler : EquipmentActionScheduler
    {
        public DefaultCraneEquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
            : base(device, actionFilters)
        {

        }

        protected override IOrderedEnumerable<EquipmentAction> GetAvailableActions()
        {
            IOrderedEnumerable<EquipmentAction> result = base.GetAvailableActions();

            ///调整优先级
            UpdatePriority(result);

            if (result == null)
            {
                return result;
            }

            //String _堆垛机特殊出库策略_当前堆垛机 = "堆垛机特殊出库策略_急速出库_" + Device.Name;
            //var _启用急速出库 = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>(_堆垛机特殊出库策略_当前堆垛机, false);
            //if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>(_堆垛机特殊出库策略_当前堆垛机, false))
            //{
            //    result = result
            //        .OrderByDescending(x => checkIsOutWork(x))
            //      .ThenByDescending(x => x.Movement.Task.Priority)
            //      .ThenBy(x => x.Movement.Task.Direction == TaskDirection.Out ? 0 : 1)
            //      .ThenBy(x => getDistance(x.DeviceName, x.Movement.StartLocation, x.Movement.EndLocation))
            //      .ThenBy(x => x.SequenceOrdering)
            //      .ThenBy(x => x.Id);
            //}
            //重置排序
            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("出库优先"))
            {
                //result = result
                //  .OrderByDescending(x => x.Movement.Task.Priority)
                //  .ThenBy(x => x.Movement.Task.Direction == TaskDirection.Out ? 0 : 1)
                //  .ThenBy(x => getDistance(x.DeviceName, x.Movement.StartLocation, x.Movement.EndLocation))
                //  .ThenBy(x => x.SequenceOrdering)
                //  .ThenBy(x => x.Id);

                result = result
                  .OrderBy(x => x.Movement.Task.Direction == TaskDirection.Out ? 0 : 1)
                  .ThenByDescending(x => x.Movement.Task.Priority)
                  .ThenBy(x => getDistance(x.DeviceName, x.Movement.StartLocation, x.Movement.EndLocation))
                  .ThenBy(x => x.SequenceOrdering)
                  .ThenBy(x => x.Id);
            }
            else
            {
                result = result
                  .OrderByDescending(x => x.Movement.Task.Priority)
                  .ThenBy(x => getDistance(x.DeviceName, x.Movement.StartLocation, x.Movement.EndLocation))
                  .ThenBy(x => x.SequenceOrdering)
                  .ThenBy(x => x.Id);
            }

            //if (this.Device.Name == "C008")
            //    Console.WriteLine("C008任务排序结果：" + String.Join(",", result.Select(x => x.Movement.Task.TaskCode + "-Priority" + x.Movement.Task.Priority).ToArray()));

            return result;
        }

        const String _堆垛机任务等待超时提升优先级 = "堆垛机任务等待超时提升优先级";
        /// <summary>
        /// 超时任务提升优先级
        /// </summary>
        /// <param name="result"></param>
        private IOrderedEnumerable<EquipmentAction> UpdatePriority(IOrderedEnumerable<EquipmentAction> result)
        {
            String _堆垛机任务等待超时提升优先级_当前堆垛机 = _堆垛机任务等待超时提升优先级 + "_" + this.Device.Name;
            String UpDatePriorityAt = "WCS增加优先级";
            UInt32 overTime;
            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey(_堆垛机任务等待超时提升优先级_当前堆垛机))
                overTime = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt32>(_堆垛机任务等待超时提升优先级_当前堆垛机, 0);
            else
                overTime = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt32>(_堆垛机任务等待超时提升优先级, 0);
            if (overTime != 0)
            {
                foreach (var item in result)
                {
                    Task _task;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        _task = unitOfWork.session.Get<Task>(item.Movement.Task.Id);
                        unitOfWork.Commit();
                    }
                    if (_task == null)
                        continue;

                    Int32 autoAddPriority = 0;
                    if (_task.AdditionalInfo.ContainsKey(UpDatePriorityAt))
                        autoAddPriority = Convert.ToInt32(_task.AdditionalInfo[UpDatePriorityAt]);
                    Int32 timeSpan = (Int32)Math.Floor(DateTime.Now.Subtract(item.CreatedAt).TotalMinutes);
                    if (timeSpan >= overTime)
                    {
                        var _priority = timeSpan / overTime;
                        if (_priority != autoAddPriority)
                        {
                            var _newPriority = item.Movement.Task.Priority + (int)_priority - autoAddPriority;
                            TaskHelper.ChangePriority(_newPriority, new int[] { item.Movement.Task.Id });
                            item.Movement.Task.Priority = _newPriority;

                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                            {
                                _task = unitOfWork.session.Get<Task>(item.Movement.Task.Id);
                                if (_task != null)
                                {
                                    if (_task.AdditionalInfo.ContainsKey(UpDatePriorityAt))
                                        _task.AdditionalInfo[UpDatePriorityAt] = _priority.ToString();
                                    else
                                        _task.AdditionalInfo.Add(UpDatePriorityAt, _priority.ToString());
                                }
                                unitOfWork.Commit();
                            }
                            _logger.Info(String.Format("WCS系统自动提高任务 {0} 优先级至 {1}", item.Movement.Task, _newPriority));
                        }
                    }
                }
            }

            return result;
        }

        int getDistance(String deviceName, LocationInfo startLocationInfo, LocationInfo endLocationInfo)
        {
            try
            {
                RackLocation startLoc = (RackLocation)LocationConverter.ToLocation(startLocationInfo);
                RackLocation endLoc = (RackLocation)LocationConverter.ToLocation(endLocationInfo);
                CraneDevice craneDevice = (CraneDevice)this.Device;
                if (craneDevice.LastStatus == null)
                {
                    return -1;
                }

                int curColumn = craneDevice.LastStatus.Column;
                int curLevel = craneDevice.LastStatus.Level;

                //设备利用率最大化应该只计算载货时间比（空跑时间），任务速度应该计算任务切换时间比(任务的完成速度)

                var distance = Math.Abs(startLoc.Column - curColumn) + Math.Abs(startLoc.Level - curLevel);

                return distance;
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }

            return -2;
        }
    }
}
