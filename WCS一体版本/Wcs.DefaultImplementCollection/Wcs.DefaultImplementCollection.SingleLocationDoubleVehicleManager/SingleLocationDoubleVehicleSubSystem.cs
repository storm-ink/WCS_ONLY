using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 单货位一轨双车调度系统
    /// </summary>
    [System.ComponentModel.Description("单货位一轨双车调度系统")]
    public class SingleLocationDoubleVehicleSubSystem : TaskableDevice, IEditableTaskOwner
    {
        public List<RailGuidedVehicleDevice> RailGuidedVehicles { get; set; }
        public List<string> RailGuidedVehicleDeviceNames { get; set; }
        public SingleLocationDoubleVehicleSubSystem(String name, Int32 no, List<String> railGuidedVehicleDeviceNames)
            : base(name, no, 500, 500, 500, true)
        {
            this.RailGuidedVehicles = new List<RailGuidedVehicleDevice>();
            this.RailGuidedVehicleDeviceNames = railGuidedVehicleDeviceNames;
            Config = new SingleLocationDoubleVehicleSubSystemConfig(this.Name);
        }

        /// <summary>
        /// 穿梭车调度系统设置
        /// </summary>
        public SingleLocationDoubleVehicleSubSystemConfig Config { get; set; }
        public override IDeviceUserInterface CreateUserInterface()
        {
            return new SingleLocationDoubleVehicleSubSystemDeviceUserInterface();
        }
        public override bool Connect()
        {
            InitContainerDevice();
            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
                scheduler.Restore();
            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
                scheduler2.Restore();
            //((SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler)this.EquipmentActionScheduler).Restore();
            return true;
        }

        static object objlock = new object();
        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                lock (objlock)
                {
                    List<Int32> equipmentTasks = new List<int>();
                    foreach (var item in RailGuidedVehicles)
                    {
                        if (item.LastStatus == null)
                            continue;

                        Int32 equipmentTaskId = 0;
                        if (int.TryParse(item.LastStatus.TaskId.Substring(2, 6), out equipmentTaskId))
                            equipmentTasks.Add(equipmentTaskId);
                    }

                    return equipmentTasks.ToArray();
                }
            }
        }

        private void InitContainerDevice()
        {
            if (this.RailGuidedVehicles == null || this.RailGuidedVehicles.Count() == 0)
            {
                foreach (var item in RailGuidedVehicleDeviceNames)
                {
                    var crane = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(item);
                    if (!RailGuidedVehicles.Any(x => x.Name == item))
                        this.RailGuidedVehicles.Add(crane);
                }
            }
        }

        /// <summary>
        /// 获取空闲设备
        /// </summary>
        /// <returns></returns>
        public RailGuidedVehicleDevice[] GetFreeRailGuidedVehicles()
        {
            if (RailGuidedVehicles.Count() == 0)
                InitContainerDevice();

            List<RailGuidedVehicleDevice> _freeCranes = new List<RailGuidedVehicleDevice>();
            foreach (var item in this.RailGuidedVehicles)
            {
                //通过穿梭车当前状态判断设备是否空闲
                if (!item.IsIdle.Result)
                    continue;

                //通过任务记录判断设备是否空闲
                Boolean bindingTask = false;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    bindingTask = unitOfWork.session.Query<TaskActionBindingDevice>().Any(x => x.DeviceName == item.Name);
                    unitOfWork.Commit();
                }
                if (bindingTask)
                    continue;

                _freeCranes.Add(item);
            }

            return _freeCranes.ToArray();
        }

        public virtual void AcceptStation(params SingleLocationDoubleVehicleSubSystemLocation[] stations)
        {
            if (stations == null)
            {
                throw new ArgumentNullException("stations");
            }

            if (stations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            foreach (var loc in stations)
            {
                if (this._locations.Any(x => x.ToConvertibleCode() == loc.ToConvertibleCode()))
                {
                    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", loc, this));
                }
            }

            _locations.AddRange(stations);
        }

        public override void Disconnect()
        {
        }

        public override bool IsConnected
        {
            get { return true; }
        }

        public override IsIdleResult IsIdle
        {
            get
            {
                if (!this.Locker.IsEmpty)
                    return new IsIdleResult(false, "锁不为空");
                else
                    return new IsIdleResult(true, "");
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<String> result = new List<string>();
                if (this.Locker != null && !this.Locker.IsEmpty)
                    result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));
                if (RailGuidedVehicleDeviceNames == null || RailGuidedVehicleDeviceNames.Count() == 0)
                    result.Add($"{this.Name} 未配置可用穿梭车");
                else
                {
                    foreach (var item in RailGuidedVehicleDeviceNames)
                    {
                        if (this.GetVehicleState(item) != RailGuidVehicleSetStatus.Working)
                            result.Add($"{item} 处于 {this.GetVehicleState(item).GetDescription()} 状态");
                    }
                }
                if (this.Config.SafeDistance <= 0)
                    result.Add($"未设置安全距离，当前安全距离设置为 {this.Config.SafeDistance} mm");

                return result.ToArray();
            }
        }

        public void Cancel(EquipmentAction task, out bool cancelled)
        {
            //throw new NotImplementedException();
            cancelled = false;
        }

        public override void CancelTask(EquipmentAction action)
        {
            //throw new NotImplementedException();
        }

        public void Complete(EquipmentAction task, out bool cancelled)
        {
            //throw new NotImplementedException();
            cancelled = false;
        }

        public override TState Read<TState>()
        {
            throw new NotImplementedException();
        }

        public void Resume(EquipmentAction task, out bool cancelled)
        {
            //throw new NotImplementedException();
            cancelled = false;
        }

        public override void SendTask(EquipmentAction action, params string[] args)
        {
            SendTaskPre(action);

            var vehicle = args[0];
            //加入到调度系统中
            StateManagerRestoreInfo restoreInfo;
            if (action.Movement.Task.CurrentLocation.UserCode == vehicle)
                BindingTaskAndVehicle(action.Id, "移动到终点", vehicle, out restoreInfo);
            else
                BindingTaskAndVehicle(action.Id, "移动到起点", vehicle, out restoreInfo);

            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
                scheduler.RestoreInfos.Add(restoreInfo);
            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
                scheduler2.RestoreInfos.Add(restoreInfo);
        }

        public override void SendTaskPre(EquipmentAction action)
        {
            foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.EquipmentActionSendPreHandlersElement.BaseEquipmentActionSendPreHandlers)
            {
                handler.Hand(action);
            }
        }

        public void Suspend(EquipmentAction task, out bool cancelled)
        {
            //throw new NotImplementedException();
            cancelled = false;
        }

        public override void Write<TCommand>(TCommand data, Func<TaskableDevice, TCommand, bool> isSuccess)
        {
            throw new NotImplementedException();
        }

        protected override void FireTaskCompletedEvent(TaskCompletedEventArgs args)
        {
            base.FireTaskCompletedEvent(args);
        }

        #region 绑定任务操作
        /// <summary>
        /// 绑定记录
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="currentState"></param>
        /// <param name="vehicle"></param>
        /// <param name="restoreInfo"></param>
        public void BindingTaskAndVehicle(int actionId, string currentState, string vehicle, out StateManagerRestoreInfo restoreInfo)
        {
            restoreInfo = new StateManagerRestoreInfo(actionId, currentState, this.Name, vehicle);
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    unitOfWork.session.Save(restoreInfo);
                    unitOfWork.session.Flush();
                    unitOfWork.Commit();
                }

                ts.Complete();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="currentState"></param>
        /// <param name="vehicle"></param>
        /// <param name="restoreInfo"></param>
        public void DeleteBindingTaskAndVehicle(int actionId)
        {
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var binding = unitOfWork.session.Query<StateManagerRestoreInfo>().FirstOrDefault(x => x.EquipmentActionId == actionId);
                    unitOfWork.session.Delete(binding);
                    unitOfWork.session.Flush();
                    unitOfWork.Commit();
                }

                ts.Complete();
            }
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="vehicle"></param>
        /// <param name="restoreInfo"></param>
        public void UpdateTaskAndVehicleInfo(string currentState, string vehicle, int actionId)
        {
            StateManagerRestoreInfo restoreInfo = null;
            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
                restoreInfo = scheduler.RestoreInfos.First(x => x.EquipmentActionId == actionId);
            if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
                restoreInfo = scheduler2.RestoreInfos.First(x => x.EquipmentActionId == actionId);
            if (restoreInfo == null)
                return;

            if (restoreInfo.CurrentStateName != currentState || restoreInfo.ActuatingDeviceName != vehicle)
            {
                restoreInfo.CurrentStateName = currentState;
                restoreInfo.ActuatingDeviceName = vehicle;
                restoreInfo.LastUpdateAt = DateTime.Now;
            }

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    restoreInfo = unitOfWork.session.Query<StateManagerRestoreInfo>().FirstOrDefault(x => x.EquipmentActionId == actionId);
                    if (restoreInfo.CurrentStateName != currentState || restoreInfo.ActuatingDeviceName != vehicle)
                    {
                        restoreInfo.CurrentStateName = currentState;
                        restoreInfo.ActuatingDeviceName = vehicle;
                        restoreInfo.LastUpdateAt = DateTime.Now;
                        unitOfWork.session.Update(restoreInfo);
                        unitOfWork.session.Flush();
                    }
                    unitOfWork.Commit();
                }

                ts.Complete();
            }
        }

        /// <summary>
        /// 重置绑定任务
        /// </summary>
        /// <param name="deviceName"></param>
        internal void ResetBindingTask(string deviceName)
        {
            try
            {
                var vehicleState = this.GetVehicleState(deviceName);
                if (vehicleState != RailGuidVehicleSetStatus.Stoping && vehicleState == RailGuidVehicleSetStatus.Repairing)
                    throw new Exception($"{deviceName} 处于 {vehicleState} 状态，无法重置已分配任务");

                StateManagerRestoreInfo restoreInfo = null;
                EquipmentAction action = null;
                if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
                {
                    restoreInfo = scheduler.RestoreInfos.FirstOrDefault(x => x.OwnDeviceName == deviceName);
                    action = scheduler.Actions.FirstOrDefault(x => x.Id == restoreInfo.EquipmentActionId);
                }
                if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
                {
                    restoreInfo = scheduler2.RestoreInfos.FirstOrDefault(x => x.OwnDeviceName == deviceName);
                    action = scheduler2.Actions.FirstOrDefault(x => x.Id == restoreInfo.EquipmentActionId);
                }
                if (restoreInfo == null)
                    throw new Exception($"{deviceName} 未查询到已绑定的任务，无法重置已分配任务");
                if (action == null)
                    throw new Exception($"{deviceName} 未查询 Id 为 {restoreInfo.EquipmentActionId} 的物理动作，无法重置已分配任务");
                var vehicleAction = (SingleLocationDoubleVehicleSubSystemTransferAction)action;
                var taskId = action.Movement.Task.Id;
                Task task;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    task = unitOfWork.session.Get<Task>(taskId);
                    unitOfWork.Commit();
                }
                if (task == null)
                    throw new Exception($"{deviceName} 未查询 Id 为 {restoreInfo.EquipmentActionId} 的物理动作对应的主任务，无法重置已分配任务");
                if (vehicleAction.StartLocation.DeviceCode != task.CurrentLocation.DeviceCode)
                    throw new Exception($"{deviceName} 所绑定任务的当前位置({task.CurrentLocation.DeviceCode})不是物理动作起点位置({vehicleAction.StartLocation.DeviceCode})，无法重置已分配任务，可以选择人工将托盘送到指定位置后强制完成该物理动作对应的逻辑动作并继续执行，也可以选择人工将托盘排出系统后取消该条主任务");

                this.DeleteBindingTaskAndVehicle(vehicleAction.Id);
                if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler0)
                    scheduler0.RestoreInfos.Remove(restoreInfo);
                if (this.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler02)
                    scheduler02.RestoreInfos.Remove(restoreInfo);

                _logger.Warn1($"已手动移除车辆{deviceName}和任务{vehicleAction}的绑定", this);
                using (NHUnitOfWork unitOfWork1 = new NHUnitOfWork())
                {
                    var _action = unitOfWork1.session.Get<SingleLocationDoubleVehicleSubSystemTransferAction>(action.Id);
                    _action.Status = EquipmentActionStatus.New;
                    unitOfWork1.session.SaveOrUpdate(_action);
                    unitOfWork1.Commit();
                }
                _logger.Warn1($"手动移除车辆{deviceName}和任务{vehicleAction}的绑定时，已重置任务{vehicleAction}的状态为{EquipmentActionStatus.New}", this);
                Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.Framework.Events.EquipmentActionStatusChangedEvent>(new Framework.Events.EquipmentActionStatusChangedEvent(task.Id, vehicleAction.Movement.Id, vehicleAction.Id, EquipmentActionStatus.New));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
