using System;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 放货命令
    /// </summary>
    public sealed class PuttingCommand : RailGuidedVehicleTelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 初始化 PutingCommand 类型
        /// </summary>
        /// <param name="String">设备任务号</param>
        /// <param name="containerCode">托盘号</param>
        /// <param name="endStation">放货站点</param>
        /// <param name="puttingStation">放货站点链条动作</param>
        public PuttingCommand(String taskId, UInt32 containerCode, UInt16 puttingStation, ChainAction chainAction)
        {
            if (taskId.Length != 8)
            {
                throw new ArgumentOutOfRangeException("taskId",string.Format("任务号“{0}“长度不等于 8", taskId));
            }

            this.TaskId = taskId;
            this.ContainerCode = containerCode;
            this.PuttingStation = puttingStation;
            this.ChainAction = chainAction;
        }

        /// <summary>
        /// 放货站链条动作
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
        /// 放货站
        /// </summary>
        public UInt16 PuttingStation { get; set; }

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
                0,
                PuttingStation,
                (int)ChainAction.None,
                (int)ChainAction,
                (int)RailGuidedVehicleTaskMode.Putting,
                Suffix);
        }

        public bool SendSuccess(Wcs.Framework.TaskableDevice taskableDevice, Wcs.Framework.DeviceCommand command)
        {
            RailGuidedVehicleDevice railGuidedVehicle = (RailGuidedVehicleDevice)taskableDevice;
            PuttingCommand cmd = (PuttingCommand)command;

            return railGuidedVehicle.LastStatus != null
                && railGuidedVehicle.LastStatus.TaskId == this.TaskId;
        }
    }
}