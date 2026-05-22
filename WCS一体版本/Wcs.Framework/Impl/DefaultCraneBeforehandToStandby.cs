using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的入库时堆垛机提前到取货点待命的附加任务<br />
    /// 在堆垛机空闲时（无出库任务可执行、无其它动作），此时有要到该堆垛机所在巷道的入库任务（还未到达巷道入库口），堆垛机将下发一个运行到推测取货点的位置提前待命。
    /// </summary>
    public class DefaultCraneBeforehandToStandby: IEquipmentActionSequenceAdditionalTask
    {
        static Random random = new Random();
        /*public void Execute(EquipmentActionSequence sequence, NHUnitOfWork unitOfWork)
        {
            if (!sequence.DeviceInfo.GetDevice().IsIdle)
            {
                return;
            }

            if (sequence.SendingActions.Length > 0 || sequence.ExecutingActions.Length > 0)
            {
                return;
            }

            var endLocation = sequence
                .WaitingActions
                .Where(x => x.Movement.Task.Direction == TaskDirection.In && (x is EquipmentActions.CraneAutomaticTransferAction))
                .Select(x => (x as EquipmentActions.CraneAutomaticTransferAction).GetLoadLocation())
                .FirstOrDefault();

            if (endLocation == null)
            {
                bool mustToClearUnitOfWork;
                if (unitOfWork == null)
                {
                    unitOfWork = new NHUnitOfWork();
                    mustToClearUnitOfWork = true;
                }
                else
                {
                    mustToClearUnitOfWork = false;
                }

                //预测入库任务的巷道取货口
                var task = Repository.Query<Task>(unitOfWork,
                    x => x.Status != TaskStatus.Completed
                        && x.Status != TaskStatus.Cancelled
                        && x.Status != TaskStatus.Suspend
                        && x.Status != TaskStatus.Error
                        && (x.EndLocation.DeviceName == sequence.DeviceInfo.DeviceName && x.EndLocation.DeviceType == sequence.DeviceInfo.DeviceType)
                        && !x.Movements
                            .Any(movement => movement.Status != LogicMovementStatus.New
                                 && movement.DeviceInfo.DeviceName == sequence.DeviceInfo.DeviceName && movement.DeviceInfo.DeviceType == sequence.DeviceInfo.DeviceType)
                        )
                        .FirstOrDefault();
                if (task != null)
                {
                    var routes = Task.FindNextPath(task.StartLocation.GetLocation(), task.EndLocation.GetLocation(), task.CurrentLocation.GetLocation(),null, task.BizType == TaskBizType.Counting ? DeviceRouteType.Counting : DeviceRouteType.Normal);
                    if (routes != null && routes.Count > 0)
                    {
                        var route = routes.First().Key.Routes.SingleOrDefault(x => x.EndLocation.Equals(task.EndLocation.GetLocation()));
                        if (route.StartLocation is RackLocation)
                        {
                            endLocation = (RackLocation)route.StartLocation;
                        }
                        else
                        {
                            endLocation = (RackLocation)route.StartLocation.SameAs.Single(x => x.Device == sequence.DeviceInfo.GetDevice());
                        }
                    }
                }
                //////////////////////
                if (mustToClearUnitOfWork)
                {
                    unitOfWork.Dispose();
                }
            }

            Crane crane=sequence.DeviceInfo.GetDevice() as Crane;
            if (endLocation != null 
                && crane.StatusInfo != null
                && (crane.StatusInfo.CurrentPosition.ColumnUserCode != endLocation.Column.ToString("000")
                || (crane.StatusInfo.CurrentPosition.LevelUserCode != endLocation.Level.ToString("000")
                    && crane.StatusInfo.CurrentPosition.LevelUserCode != "???"
                    )
                )
                )
            {
                EquipmentActions.CraneMoveToAction moveTo = new EquipmentActions.CraneMoveToAction(sequence.DeviceInfo.GetDevice(), random.Next(10000000, 99999999), 0, endLocation);
                sequence.Logger.Info(string.Format("{0} 开始向 {1} 发送 {2}...",this,sequence,moveTo),this,moveTo);

                sequence.DeviceInfo.GetDevice().SendTask(moveTo);
            }
        }*/

        public void Execute(EquipmentActionSequence sequence, NHUnitOfWork unitOfWork)
        {
            if (!sequence.DeviceInfo.GetDevice().IsIdle)
            {
                return;
            }

            if (sequence.SendingActions.Length > 0 || sequence.ExecutingActions.Length > 0 || sequence.CurrentEquipmentAction!=null)
            {
                return;
            }

            var endLocation = sequence
                .WaitingActions
                .Where(x => x.Movement.Task.Direction == TaskDirection.In && (x is EquipmentActions.CraneAutomaticTransferAction))
                .Select(x => (x as EquipmentActions.CraneAutomaticTransferAction).GetLoadLocation())
                .FirstOrDefault();

            if (endLocation == null)
            {

                var tasks = Cfg.Configuration.Devices
                    .Select(x => x.ActionSequence)
                    .SelectMany(x => x.Actions.Concat(x.SendingActions))
                    .GroupBy(x => x.Movement.Task)
                    .Select(x => x.Key);

                //预测入库任务的巷道取货口
                var task = tasks.Where(x => x.Status != TaskStatus.Completed
                        && x.Status != TaskStatus.Cancelled
                        && x.Status != TaskStatus.Suspend
                        && x.Status != TaskStatus.Error
                        && (x.EndLocation.DeviceName == sequence.DeviceInfo.DeviceName && x.EndLocation.DeviceType == sequence.DeviceInfo.DeviceType)
                        && !x.Movements
                            .Any(movement => movement.Status != LogicMovementStatus.New
                                 && movement.DeviceInfo.DeviceName == sequence.DeviceInfo.DeviceName && movement.DeviceInfo.DeviceType == sequence.DeviceInfo.DeviceType)
                        )
                        .FirstOrDefault();

                if (task != null)
                {
                    var routes = Task.FindNextPath(task.StartLocation.GetLocation(), task.EndLocation.GetLocation(), task.CurrentLocation.GetLocation(), null, task.BizType == TaskBizType.Counting ? DeviceRouteType.Counting : DeviceRouteType.Normal);
                    if (routes != null && routes.Count > 0)
                    {
                        var route = routes.First().Key.Routes.SingleOrDefault(x => x.EndLocation.Equals(task.EndLocation.GetLocation()));
                        if (route.StartLocation is RackLocation)
                        {
                            endLocation = (RackLocation)route.StartLocation;
                        }
                        else
                        {
                            endLocation = (RackLocation)route.StartLocation.SameAs.Single(x => x.Device == sequence.DeviceInfo.GetDevice());
                        }
                    }
                }
            }

            Crane crane = sequence.DeviceInfo.GetDevice() as Crane;
            if (endLocation != null
                && crane.StatusInfo != null
                && (crane.StatusInfo.CurrentPosition.ColumnUserCode != endLocation.Column.ToString("000")
                || (crane.StatusInfo.CurrentPosition.LevelUserCode != endLocation.Level.ToString("000")
                    && crane.StatusInfo.CurrentPosition.LevelUserCode != "???"
                    )
                )
                )
            {
                EquipmentActions.CraneMoveToAction moveTo = new EquipmentActions.CraneMoveToAction(sequence.DeviceInfo.GetDevice(), random.Next(10000000, 99999999), 0, endLocation);
                sequence.Logger.Info(string.Format("{0} 开始向 {1} 发送 {2}...", this, sequence, moveTo), this, moveTo);

                sequence.DeviceInfo.GetDevice().SendTask(moveTo);
            }
        }
    }
}
