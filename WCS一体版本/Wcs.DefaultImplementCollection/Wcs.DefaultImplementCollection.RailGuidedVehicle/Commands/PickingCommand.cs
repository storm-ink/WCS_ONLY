using System;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 取货命令
    /// </summary>
    public sealed class PickingCommand : RailGuidedVehicleTelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 初始货 PickingCommand 类型
        /// </summary>
        /// <param name="taskId">设备任务号</param>
        /// <param name="containerCode">托盘号</param>
        /// <param name="endStation">取货站点</param>
        /// <param name="chainAction">取货站点链条动作</param>
        public PickingCommand(String taskId, UInt32 containerCode, UInt16 pickingStation, ChainAction chainAction)
        {
            if (taskId.Length != 8)
            {
                throw new ArgumentOutOfRangeException("taskId", string.Format("任务号“{0}“长度不等于 8", taskId));
            }
            this.TaskId = taskId;
            this.ContainerCode = containerCode;
            this.PickingStation = pickingStation;
            this.ChainAction = chainAction;
        }

        /// <summary>
        /// 取货站链条动作
        /// </summary>
        public ChainAction ChainAction { get; set; }

        /// <summary>
        /// 托盘号（4位）
        /// </summary>
        public UInt32 ContainerCode { get; set; }

        public override int Length
        {
            get { return 25; }
        }

        /// <summary>
        /// 取货站
        /// </summary>
        public UInt16 PickingStation { get; set; }

        /// <summary>
        /// 任务号（8位）
        /// </summary>
        public String TaskId { get; set; }

        public override string TypeFlag
        {
            get { return "HB"; }
        }

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string ToTelex()
        {
            return String.Format("{0}{1}{2:00000000}{3:0000}{4:000}{5:000}{6}{7}{8}{9}0",
                Prefix,
                TypeFlag,
                TaskId,
                ContainerCode,
                PickingStation,
                0,
                (int)ChainAction,
                (int)ChainAction.None,
                (int)RailGuidedVehicleTaskMode.Picking,
                Suffix);
        }

        public bool SendSuccess(Wcs.Framework.TaskableDevice taskableDevice, Wcs.Framework.DeviceCommand command)
        {
            RailGuidedVehicleDevice railGuidedVehicle = (RailGuidedVehicleDevice)taskableDevice;
            PickingCommand cmd = (PickingCommand)command;

            return railGuidedVehicle.LastStatus != null
                && railGuidedVehicle.LastStatus.TaskId == this.TaskId;
        }
    }
}