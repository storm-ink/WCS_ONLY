using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 堆垛机货叉深度（必须先1、2，后3、4的顺序取）物理动作过滤器
    /// </summary>
    public class CraneForkDeepEquipmentActionSchedulerFilter : EquipmentActionSchedulerFilter
    {
        class _actionCache
        {
            public CraneAutomaticTransferWithStepByStepAction Action { get; set; }
            public RackLocation LoadLocation { get; set; }
            public RackLocation UnloadLocation { get; set; }
        }

        static class _actionCacheRecord
        {
            public static DateTime CreateAt { get; set; }
            public static int Count { get; set; }
            public static Boolean 已过期
            {
                get
                {
                    if (CreateAt == null || Cache == null)
                    {
                        return true;
                    }

                    return DateTime.Now.Subtract(CreateAt).TotalMilliseconds > 1000;
                }
            }

            public static List<_actionCache> Cache { get; set; }
        }
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(equipmentActionScheduler.Device is CraneDevice))
            {
                return new ActionSchedulerFilterResult(false, "非堆垛机任务");
            }

            if (!(action is CraneAutomaticTransferWithStepByStepAction))
            {
                return new ActionSchedulerFilterResult(false, "非堆垛机任务");
            }

            var craneAutomaticTransferAction = (CraneAutomaticTransferWithStepByStepAction)action;
            var crane = (CraneDevice)equipmentActionScheduler.Device;

            List<_actionCache> allActions;

            if (_actionCacheRecord.已过期 || _actionCacheRecord.Count != equipmentActionScheduler.Actions.Length)
            {
                allActions = equipmentActionScheduler.Actions
                .Where(x => x.Status != EquipmentActionStatus.Completed && x.Status != EquipmentActionStatus.Cancelled)
                .Where(x => x is CraneAutomaticTransferWithStepByStepAction)
                .Cast<CraneAutomaticTransferWithStepByStepAction>()
                .Select(x => new _actionCache
                {
                    Action = x,
                    LoadLocation = IsRackLocation(x.LoadLocation) ? (RackLocation)LocationConverter.ToLocation(x.LoadLocation) : null,
                    UnloadLocation = IsRackLocation(x.UnloadLocation) ? (RackLocation)LocationConverter.ToLocation(x.UnloadLocation) : null,
                })
                .ToList();

                _actionCacheRecord.Cache = allActions;
                _actionCacheRecord.Count = equipmentActionScheduler.Actions.Length;
                _actionCacheRecord.CreateAt = DateTime.Now;
            }
            else
            {
                allActions = _actionCacheRecord.Cache;
            }
            if (IsRackLocation(craneAutomaticTransferAction.LoadLocation))
            {
                var imcompleteLoadLocations = allActions.Where(x => x.LoadLocation != null);

                var loadLocation = (RackLocation)LocationConverter.ToLocation(craneAutomaticTransferAction.LoadLocation);
                
                ///左二和右二任务必须延迟一段时间等待程序判断后再执行
                double Left2vsRight2DelayTime;
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey("CraneForkDeepEquipmentActionRunningDelayTime"))
                    Left2vsRight2DelayTime = Convert.ToInt32(Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["CraneForkDeepEquipmentActionRunningDelayTime"]);
                else
                    Left2vsRight2DelayTime = 3000;
	                
                if (loadLocation.ForkDirection == ForkDirection.Left2)
                {
                    var span = DateTime.Now.Subtract(craneAutomaticTransferAction.CreatedAt).TotalMilliseconds;
                    if (Left2vsRight2DelayTime > span)
                        return new ActionSchedulerFilterResult(true, string.Format("任务将在创建后 {0} 执行", Left2vsRight2DelayTime));

                    var _imcompleteLoadLocation = imcompleteLoadLocations
                        .FirstOrDefault(x =>
                        x.LoadLocation.UserColumn == loadLocation.UserColumn
                        && x.LoadLocation.UserLevel == loadLocation.UserLevel
                        && x.LoadLocation.ForkDirection == ForkDirection.Left);

                    if (_imcompleteLoadLocation != null)
                    {
                        return new ActionSchedulerFilterResult(true, string.Format("在取{0}上的货物之前必须先将{1}上的货物取出。", loadLocation, _imcompleteLoadLocation.Action));
                    }

                    ///2017年10月19日威宁现场发现WMS在同时下发近端和远端任务时（即WCS收到的任务创建时间一样），在equipmentActionScheduler的Actions层面判断可能会出现漏判断的情况
                    ///因此增加主任务Task层面的判断，判断方法如下：
                    ///1.先获取需要判断位置的对应位置
                    /// 取货位置是二伸位的获取对应的1伸位 放货位置是1伸位的获取对应位置为2伸位的位置
                    ///2.再从任务层面判断是否存在任务起点或者终点为 对应位置 的任务未执行，判断条件是 是否存在已完成的包含对应位置的动作
                    var _relativeLocations = crane.Locations.Select(x=>(RackLocation)x).Where(x => x.UserColumn == loadLocation.UserColumn && x.UserLevel == loadLocation.UserLevel && x.ForkDirection == ForkDirection.Left);
                    //针对于威宁现场一台堆垛机存在多个巷道的情况做如下过滤，取用户编码前两位差值为1的位置
                    RackLocation _relativeLocation = null;
                    foreach (var item in _relativeLocations)
                    {
                        var _deviation = Math.Abs(Convert.ToInt32(item.UserLine) - Convert.ToInt32(loadLocation.UserLine));
                        if (_deviation == 1)
                        {
                            _relativeLocation = item;
                            break;
                        }
                    }
                    if (_relativeLocation != null)
                    {
                        Task[] _tasks;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _tasks = unitOfWork.session.Query<Task>().Where(x => x.StartLocation.UserCode == _relativeLocation.UserCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed).ToArray();
                            unitOfWork.Commit();
                        }
                        foreach (var item in _tasks)
                        {
                            if (!item.Movements.Any(x => x.StartLocation.UserCode == _relativeLocation.UserCode && x.Status == LogicMovementStatus.Cancelled || x.Status == LogicMovementStatus.Completed))
                                return new ActionSchedulerFilterResult(true, string.Format("在取{0}上的货物之前必须先将{1}上的货物取出。", loadLocation, _relativeLocation));
                        }
                    }
                }

                if (loadLocation.ForkDirection == ForkDirection.Right2)
                {
                    var span = DateTime.Now.Subtract(craneAutomaticTransferAction.CreatedAt).TotalMilliseconds;
                    if (Left2vsRight2DelayTime > span)
                        return new ActionSchedulerFilterResult(true, string.Format("任务将在创建后 {0} 执行", Left2vsRight2DelayTime));

                    var _imcompleteLoadLocation = imcompleteLoadLocations
                        .FirstOrDefault(x =>
                        x.LoadLocation.UserColumn == loadLocation.UserColumn
                        && x.LoadLocation.UserLevel == loadLocation.UserLevel
                        && x.LoadLocation.ForkDirection == ForkDirection.Right);

                    if (_imcompleteLoadLocation != null)
                    {
                        return new ActionSchedulerFilterResult(true, string.Format("在取{0}上的货物之前必须先将{1}上的货物取出。", loadLocation, _imcompleteLoadLocation.Action));
                    }

                    ///2017年10月19日威宁现场发现WMS在同时下发近端和远端任务时（即WCS收到的任务创建时间一样），在equipmentActionScheduler的Actions层面判断可能会出现漏判断的情况
                    ///因此增加主任务Task层面的判断，判断方法如下：
                    ///1.先获取需要判断位置的对应位置
                    /// 取货位置是二伸位的获取对应的1伸位 放货位置是1伸位的获取对应位置为2伸位的位置
                    ///2.再从任务层面判断是否存在任务起点或者终点为 对应位置 的任务未执行，判断条件是 是否存在已完成的包含对应位置的动作
                    var _relativeLocations = crane.Locations.Select(x => (RackLocation)x).Where(x => x.UserColumn == loadLocation.UserColumn && x.UserLevel == loadLocation.UserLevel && x.ForkDirection == ForkDirection.Right);
                    //针对于威宁现场一台堆垛机存在多个巷道的情况做如下过滤，取用户编码前两位差值为1的位置
                    RackLocation _relativeLocation = null;
                    foreach (var item in _relativeLocations)
                    {
                        var _deviation = Math.Abs(Convert.ToInt32(item.UserLine) - Convert.ToInt32(loadLocation.UserLine));
                        if (_deviation == 1)
                        {
                            _relativeLocation = item;
                            break;
                        }
                    }
                    if (_relativeLocation != null)
                    {
                        Task[] _tasks;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _tasks = unitOfWork.session.Query<Task>().Where(x => x.StartLocation.UserCode == _relativeLocation.UserCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed).ToArray();
                            unitOfWork.Commit();
                        }
                        foreach (var item in _tasks)
                        {
                            if (!item.Movements.Any(x => x.StartLocation.UserCode == _relativeLocation.UserCode && x.Status == LogicMovementStatus.Cancelled || x.Status == LogicMovementStatus.Completed))
                                return new ActionSchedulerFilterResult(true, string.Format("在取{0}上的货物之前必须先将{1}上的货物取出。", loadLocation, _relativeLocation));
                        }
                    }
                }
            }

            if (IsRackLocation(craneAutomaticTransferAction.UnloadLocation))
            {
                var imcompleteUnLoadLocations = allActions
                .Where(x => x.UnloadLocation != null);

                var unloadLocation = (RackLocation)LocationConverter.ToLocation(craneAutomaticTransferAction.UnloadLocation);
                if (unloadLocation.ForkDirection == ForkDirection.Left)
                {
                    var _imcompleteUnLoadLocation = imcompleteUnLoadLocations
                        .FirstOrDefault(x =>
                        x.UnloadLocation.UserColumn == unloadLocation.UserColumn
                        && x.UnloadLocation.UserLevel == unloadLocation.UserLevel
                        && x.UnloadLocation.ForkDirection == ForkDirection.Left2);

                    if (_imcompleteUnLoadLocation != null)
                    {
                        return new ActionSchedulerFilterResult(true, string.Format("在向{0}上的存放货物之前必须先将{1}上的货物放好。", unloadLocation, _imcompleteUnLoadLocation.Action));
                    }

                    ///2017年10月19日威宁现场发现WMS在同时下发近端和远端任务时（即WCS收到的任务创建时间一样），在equipmentActionScheduler的Actions层面判断可能会出现漏判断的情况
                    ///因此增加主任务Task层面的判断，判断方法如下：
                    ///1.先获取需要判断位置的对应位置
                    /// 取货位置是二伸位的获取对应的1伸位 放货位置是1伸位的获取对应位置为2伸位的位置
                    ///2.再从任务层面判断是否存在任务起点或者终点为 对应位置 的任务未执行，判断条件是 是否存在已完成的包含对应位置的动作
                    var _relativeLocations = crane.Locations.Select(x => (RackLocation)x).Where(x => x.UserColumn == unloadLocation.UserColumn && x.UserLevel == unloadLocation.UserLevel && x.ForkDirection == ForkDirection.Left2);
                    //针对于威宁现场一台堆垛机存在多个巷道的情况做如下过滤，取用户编码前两位差值为1的位置
                    RackLocation _relativeLocation = null;
                    foreach (var item in _relativeLocations)
                    {
                        var _deviation = Math.Abs(Convert.ToInt32(item.UserLine) - Convert.ToInt32(unloadLocation.UserLine));
                        if (_deviation == 1)
                        {
                            _relativeLocation = item;
                            break;
                        }
                    }
                    if (_relativeLocation != null)
                    {
                        Task[] _tasks;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _tasks = unitOfWork.session.Query<Task>().Where(x => x.EndLocation.UserCode == _relativeLocation.UserCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed).ToArray();
                            unitOfWork.Commit();
                        }
                        foreach (var item in _tasks)
                        {
                            if (!item.Movements.Any(x => x.EndLocation.UserCode == _relativeLocation.UserCode && x.Status == LogicMovementStatus.Cancelled || x.Status == LogicMovementStatus.Completed))
                                return new ActionSchedulerFilterResult(true, string.Format("在向{0}上的存放货物之前必须先将{1}上的货物放好。", unloadLocation, _relativeLocation));
                        }
                    }
                }

                if (unloadLocation.ForkDirection == ForkDirection.Right)
                {
                    var _imcompleteUnLoadLocation = imcompleteUnLoadLocations
                        .FirstOrDefault(x =>
                        x.UnloadLocation.UserColumn == unloadLocation.UserColumn
                        && x.UnloadLocation.UserLevel == unloadLocation.UserLevel
                        && x.UnloadLocation.ForkDirection == ForkDirection.Right2);

                    if (_imcompleteUnLoadLocation != null)
                    {
                        return new ActionSchedulerFilterResult(true, string.Format("在向{0}上的存放货物之前必须先将{1}上的货物放好。", unloadLocation, _imcompleteUnLoadLocation.Action));
                    }

                    ///2017年10月19日威宁现场发现WMS在同时下发近端和远端任务时（即WCS收到的任务创建时间一样），在equipmentActionScheduler的Actions层面判断可能会出现漏判断的情况
                    ///因此增加主任务Task层面的判断，判断方法如下：
                    ///1.先获取需要判断位置的对应位置
                    /// 取货位置是二伸位的获取对应的1伸位 放货位置是1伸位的获取对应位置为2伸位的位置
                    ///2.再从任务层面判断是否存在任务起点或者终点为 对应位置 的任务未执行，判断条件是 是否存在已完成的包含对应位置的动作
                    var _relativeLocations = crane.Locations.Select(x => (RackLocation)x).Where(x => x.UserColumn == unloadLocation.UserColumn && x.UserLevel == unloadLocation.UserLevel && x.ForkDirection == ForkDirection.Right2);
                    //针对于威宁现场一台堆垛机存在多个巷道的情况做如下过滤，取用户编码前两位差值为1的位置
                    RackLocation _relativeLocation = null;
                    foreach (var item in _relativeLocations)
                    {
                        var _deviation = Math.Abs(Convert.ToInt32(item.UserLine) - Convert.ToInt32(unloadLocation.UserLine));
                        if (_deviation == 1)
                        {
                            _relativeLocation = item;
                            break;
                        }
                    }
                    if (_relativeLocation != null)
                    {
                        Task[] _tasks;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _tasks = unitOfWork.session.Query<Task>().Where(x => x.EndLocation.UserCode == _relativeLocation.UserCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed).ToArray();
                            unitOfWork.Commit();
                        }
                        foreach (var item in _tasks)
                        {
                            if (!item.Movements.Any(x => x.EndLocation.UserCode == _relativeLocation.UserCode && x.Status == LogicMovementStatus.Cancelled || x.Status == LogicMovementStatus.Completed))
                                return new ActionSchedulerFilterResult(true, string.Format("在向{0}上的存放货物之前必须先将{1}上的货物放好。", unloadLocation, _relativeLocation));
                        }
                    }
                }
            }

            return new ActionSchedulerFilterResult(false, "");
        }


        Boolean IsRackLocation(LocationInfo li)
        {
            var loc = LocationConverter.ToLocation(li);
            if (loc is RackLocation)
            {
                return true;
            }

            return false;
        }
    }
}