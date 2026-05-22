using System;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 手动清空命令
    /// </summary>
    public sealed class ClearClearCacheCommand : RailGuidedVehicleTelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 初始化 ClearTaskCommand 类型
        /// </summary>
        public ClearClearCacheCommand()
        {
        }

        public override int Length
        {
            get { return 25; }
        }


        public override string TypeFlag
        {
            get { return "00"; }
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
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                Suffix);
        }

        public bool SendSuccess(Wcs.Framework.TaskableDevice taskableDevice, Wcs.Framework.DeviceCommand command)
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)taskableDevice;

            return railGuidedVehicleDevice.LastStatus != null
                && railGuidedVehicleDevice.LastStatus.TaskId == "00000000"
                && railGuidedVehicleDevice.LastStatus.Event != RailGuidedVehicleEvent.AutomaticTaskCompletion
                && railGuidedVehicleDevice.LastStatus.Event != RailGuidedVehicleEvent.TaskCompletionByManual
                && railGuidedVehicleDevice.LastStatus.State != RailGuidedVehicleStatus.B;
        }
    }
}