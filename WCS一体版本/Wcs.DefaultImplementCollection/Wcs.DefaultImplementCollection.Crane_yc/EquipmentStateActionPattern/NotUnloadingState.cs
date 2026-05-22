using System;


using Wcs.Framework;
using System.Linq;
using System.Collections.Generic;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 未放货，等待放货
    /// </summary>
    public class NotUnloadingState : AbstractState
    {
        public NotUnloadingState(AbstractStateManager context)
            : base(context) { }

        public override string Name
        {
            get { return "等待放货"; }
        }

        /// <summary>
        /// 指示当前是否可以发送放货动指令
        /// </summary>
        /// <remarks>
        /// <para>发送条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、当前状态未完成</para>
        /// <para>3、设备的当前位置（层、列）和逻辑动作的终点位置（层、列）一致</para>
        /// <para>4、上下文中的物理动作状态为 Executing</para>
        /// </remarks>
        /// <returns></returns>
        public override CanPerformResult CanPerform()
        {
            if (IsCompleted().Result)
                return new CanPerformResult(false, string.Format("当前状态 {0} 已完成, 不允许发送命令", this.Name));

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new CanPerformResult(false, string.Format("当前任务处于 {0} 状态(允许发送放货命令的状态应为 新任务/执行中), 不允许发送任务", this.Name));

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new CanPerformResult(false, $"当前设备不空闲(tips:{isidle.Information}), 不允许发送命令");

            RackLocation currentLocation = craneDevice.GetCurrentLocation();
            if (currentLocation == null)
                return new CanPerformResult(false, string.Format("设备 {0} 当前位置获取为空, 不允许发送命令", craneDevice.Name));

            if (craneDevice.LastStatus.State != CraneStatus.有货待命)
                return new CanPerformResult(false, string.Format("设备 {0} 当前状态不处于 有货待命, 不允许发送命令", craneDevice.Name));

            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);

            RackLocation unloadingLocation = (RackLocation)endLocation;

            if (currentLocation.Column != unloadingLocation.Column || currentLocation.Level != unloadingLocation.Level)
            {
                //this.StepSerialNo = "Fz" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
                this.StepSerialNo = "43" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
                //if (craneDevice.LastStatus.TaskId == this.StepSerialNo)
                //    this.StepSerialNo = "433" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");

                var barcode = this.Context.EquipmentAction.Movement.Task.ContainerCodes == null || this.Context.EquipmentAction.Movement.Task.ContainerCodes.Count() == 0 ? "" : this.Context.EquipmentAction.Movement.Task.ContainerCodes.FirstOrDefault();
                AddTaskCommand cmd = new AddTaskCommand(
                    this.StepSerialNo.ToString(),
                    AddTaskCommandType.半自动行走,
                    currentLocation,
                    unloadingLocation,
                    barcode,
                    false);

                craneDevice.Write(cmd, cmd.SendSuccess);

                return new CanPerformResult(false, string.Format("设备 {0} 当前位置不是物理动作结束位置,已尝试发送移动命令", craneDevice.Name));
            }

            #region 发送前检查是否可以安全放货
            return SafeWorkCheck(craneDevice, unloadingLocation);
            #endregion

            //return new CanPerformResult(true, "可以发送命令");
        }

        private CanPerformResult SafeWorkCheck(CraneDevice craneDevice, RackLocation unloadingLocation)
        {
            //ConveyorLocation conveyorLocation = (ConveyorLocation)unloadingLocation.Synonymous.FirstOrDefault(x => x is ConveyorLocation);
            //if (conveyorLocation == null)
            //    return new CanPerformResult(true, "结束点非输送线位置可以直接放货");
            //List<ConveyorLocation> _list = new List<ConveyorLocation>();
            //_list.Add(conveyorLocation);
            //if (conveyorLocation.IsFictitiousLocation || conveyorLocation.HasFictitousLocation)
            //    _list.Add((ConveyorLocation)LocationConverter.ConvertibleCodeToLcation(conveyorLocation.FictitiousConvertibleCode));
            //foreach (var loc in _list)
            //{
            //    ConveyorDevice _conveyor = (ConveyorDevice)loc.Device;
            //    if (!_conveyor.IsConnected)
            //        return new CanPerformResult(false, String.Format("放货点是输送线位置{0}，但是位置{1}所在设备{2}未连接，当前不能发送放货命令", loc.DeviceCode, loc.DeviceCode, _conveyor.Name));
            //    ///判断占位
            //    HoldSignalNetTransferObject holdSingle = _conveyor.OccupiedSignals.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //    SimpleHoldSignalNetTransferObject simpleHoldSingle = _conveyor.ReadStatus<SimpleHoldSignalNetTransferObject>().FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //    if (holdSingle == null && simpleHoldSingle == null)
            //        return new CanPerformResult(false, String.Format("在设备{0}的占位信息和简单占位都没有查询到位置{1}的信息，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    if (holdSingle != null && holdSingle.HandShake != HoldSignalNetTransferObjectHandShake.Empty)
            //        return new CanPerformResult(false, String.Format("在设备{0}的占位信息中包含位置{1}的占位信号为不空，当前不能发送命令", _conveyor.Name, loc.DeviceCode));
            //    if (simpleHoldSingle != null && simpleHoldSingle.HandShake != HoldSignalNetTransferObjectHandShake.Empty)
            //        return new CanPerformResult(false, String.Format("在设备{0}的简单占位中包含位置{1}的占位信号为不空，当前不能发送命令", _conveyor.Name, loc.DeviceCode));
            //    ///判断光电
            //    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "") != "V2")
            //    {
            //        OccupyNetTransferObject occupy = _conveyor.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //        if (occupy == null)
            //            return new CanPerformResult(false, String.Format("在设备{0}的光电信息中没有查询到位置{1}的信息，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //        if (occupy.AftPosPotocell || occupy.AftProPotocell || occupy.AftSloPotocell || occupy.FroPosPotocell || occupy.FroProPotocell || occupy.FroSloPotocell)
            //            return new CanPerformResult(false, String.Format("在设备{0}的位置{1}光电有信号，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    }
            //    ///判断任务
            //    LocationTaskNetTransferObject _locationTask = _conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //    if (_locationTask == null)
            //        return new CanPerformResult(false, String.Format("在设备{0}的货位任务中没有查询到位置{1}的任务信息，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    if (_locationTask.TaskNo != 0)
            //        return new CanPerformResult(false, String.Format("在设备{0}的货位任务中位置{1}的任务号不为0，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    ///判断输送线状态
            //    if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "") != "V2")
            //    {
            //        LocationNetTransferObject locationState = _conveyor.ConveyorLocationStates.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //        if (locationState == null)
            //            return new CanPerformResult(false, String.Format("在设备{0}的货位状态中没有查询到位置{1}的状态信息，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //        if (locationState.Status == LocationNetTransferObjectStatus.Manual || locationState.Status == LocationNetTransferObjectStatus.Offline || locationState.Status == LocationNetTransferObjectStatus.Running)
            //            return new CanPerformResult(false, String.Format("在设备{0}的位置{1}的状态为手动、离线或者运行状态，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    }
            //    else
            //    {
            //        LocationNetTransferObject2 locationState = _conveyor.ReadStatus<LocationNetTransferObject2>().FirstOrDefault(x => x.PosNo == Convert.ToUInt16(loc.DeviceCode));
            //        if (locationState == null)
            //            return new CanPerformResult(false, String.Format("在设备{0}的货位状态中没有查询到位置{1}的状态信息，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //        if (locationState.Status == LocationNetTransferObjectStatus.Manual || locationState.Status == LocationNetTransferObjectStatus.Offline || locationState.Status == LocationNetTransferObjectStatus.Running)
            //            return new CanPerformResult(false, String.Format("在设备{0}的位置{1}的状态为手动、离线或者运行状态，当前不能发送放货命令", _conveyor.Name, loc.DeviceCode));
            //    }

            //    ///查询数据库
            //    Boolean _task;
            //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //    {
            //        _task = unitOfWork.session.Query<Task>().Any(x => x.CurrentLocation.UserCode == unloadingLocation.UserCode || x.CurrentLocation.UserCode == loc.UserCode);
            //        unitOfWork.Commit();
            //    }
            //    if (_task)
            //        return new CanPerformResult(false, String.Format("至少存在一条任务当前位置在{0}或者{1}位置，当前不能发送放货命令", unloadingLocation.UserCode, loc.UserCode));
            //}

            return new CanPerformResult(true, "可以发送放货命令");
        }

        /// <summary>
        /// 指示堆垛机是否已放货
        /// </summary>
        /// <remarks>
        /// <para>完成条件：</para>
        /// <para>1、设备处于空闲状态</para>
        /// <para>2、设备已连接并且状态数据已同步/para>
        /// <para>3、设备的处于无货待命状态</para>
        /// </remarks>
        /// <returns>完成返回 true；未完成返回假 false</returns>
        public override IsCompeltedResult IsCompleted()
        {
            string msg;
            if (this.Context._equipmentAction == null)
            {
                msg = String.Format("堆垛机单步任务完成0：由于上下文中任务信息为空 {0} 完成", this.Name);
                _logger.Info(msg);
                return new IsCompeltedResult(true, msg);
            }

            if (this.Context.EquipmentAction == null)
            {
                msg = String.Format("堆垛机单步任务完成1：由于未查询到对应的任务 {0} 完成", this.Name);
                _logger.Info(msg);
                return new IsCompeltedResult(true, msg);
            }

            if (this.Context.EquipmentAction.Status == EquipmentActionStatus.Cancelled || this.Context.EquipmentAction.Status == EquipmentActionStatus.Completed)
            {
                msg = String.Format("堆垛机单步任务完成2：由于任务已处于完成或者取消状态 {0} 放货完成", this.Context.EquipmentAction.EquipmentTaskId);
                _logger.Info(msg);
                return new IsCompeltedResult(true, msg);
            }

            if (this.Context.EquipmentAction.Status != EquipmentActionStatus.New && this.Context.EquipmentAction.Status != EquipmentActionStatus.Executing)
                return new IsCompeltedResult(false, "当前物理动作不处于 新任务/执行中 状态");

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;
            var isidle = craneDevice.IsIdle;
            if (!isidle.Result)
                return new IsCompeltedResult(false, $"设备不空闲(tips:{isidle.Information})");

            if (craneDevice.LastStatus == null)
                return new IsCompeltedResult(false, "当前设备未获取状态");

            if (craneDevice.LastStatus.State == CraneStatus.无货待命)
            {
                msg = String.Format("堆垛机单步任务完成3：{0} 放货完成，堆垛机状态：{1}", this.Context.EquipmentAction.EquipmentTaskId, craneDevice.LastStatus.GetInfo());
                _logger.Info(msg);
                return new IsCompeltedResult(true, msg);
            }
            else
                return new IsCompeltedResult(false, "当前设备不处于 无货待命 状态");
        }

        /// <summary>
        /// 发送放货指令
        /// </summary>
        public override void Perform()
        {
            if (!CanPerform().Result)
            {
                throw new InvalidOperationException("当前状态不可发送放货指令");
            }

            CraneDevice craneDevice = (CraneDevice)this.Context.Device;

            Location endLocation = LocationConverter.ToLocation(this.Context.EquipmentAction.Movement.EndLocation);

            RackLocation unloadingLocation = (RackLocation)endLocation;

            //this.StepSerialNo = "FH" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");
            this.StepSerialNo = "4" + this.Context.EquipmentAction.EquipmentTaskId.ToString("0000000");
            //if (craneDevice.LastStatus.TaskId == this.StepSerialNo)
            //    this.StepSerialNo = "44" + this.Context.EquipmentAction.EquipmentTaskId.ToString("000000");

            var barcode = this.Context.EquipmentAction.Movement.Task.ContainerCodes == null || this.Context.EquipmentAction.Movement.Task.ContainerCodes.Count() == 0 ? "" : this.Context.EquipmentAction.Movement.Task.ContainerCodes.FirstOrDefault();
            AddTaskCommand cmd = new AddTaskCommand(
                this.StepSerialNo,
                AddTaskCommandType.半自动放货,
                unloadingLocation,
                unloadingLocation,
                barcode,
                false);

            craneDevice.Write(cmd, cmd.SendSuccess);
        }
    }
}