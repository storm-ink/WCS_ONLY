using System;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public abstract class WalkCommand : RailGuidedVehicleTelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 初始货 AddWalkCommand 类型
        /// </summary>
        /// <param name="taskId">设备任务号</param>
        /// <param name="containerCode">托盘号</param>
        /// <param name="endStation">结束站点</param>
        public WalkCommand(String taskId, UInt32 containerCode, UInt16 endStation)
        {
            if (taskId.Length != 8)
            {
                throw new ArgumentOutOfRangeException("taskId", string.Format("任务号“{0}“长度不等于 8", taskId));
            }

            this.TaskId = taskId;
            this.ContainerCode = containerCode;
            this.EndStation = endStation;
        }

        /// <summary>
        /// 托盘号（4位）
        /// </summary>
        public UInt32 ContainerCode { get; set; }

        /// <summary>
        /// 结束站
        /// </summary>
        public UInt16 EndStation { get; set; }

        public override int Length
        {
            get { return 25; }
        }

        /// <summary>
        /// 任务号（8位）
        /// </summary>
        public String TaskId { get; set; }

        /// <summary>
        /// 任务模式
        /// </summary>
        public abstract RailGuidedVehicleTaskMode TaskMode { get; }

        public override string TypeFlag
        {
            get { return "HS"; }
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
                EndStation,
                (int)ChainAction.None,
                (int)ChainAction.None,
                (int)TaskMode,
                Suffix);
        }

        public bool SendSuccess(Wcs.Framework.TaskableDevice taskableDevice, Wcs.Framework.DeviceCommand command)
        {
            RailGuidedVehicleDevice railGuidedVehicle = (RailGuidedVehicleDevice)taskableDevice;
            WalkCommand cmd = (WalkCommand)command;

            return railGuidedVehicle.LastStatus != null 
                && railGuidedVehicle.LastStatus.TaskId == this.TaskId;
        }
    }
}