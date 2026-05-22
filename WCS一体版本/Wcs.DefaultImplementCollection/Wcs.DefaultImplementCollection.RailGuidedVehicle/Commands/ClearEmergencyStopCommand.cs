using System;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 手动清空命令
    /// </summary>
    public sealed class ClearEmergencyStopCommand : RailGuidedVehicleTelexTransferObject,Wcs.Framework.IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 初始化 ClearTaskCommand 类型
        /// </summary>
        public ClearEmergencyStopCommand()
        {
        }

        public override int Length
        {
            get { return 25; }
        }


        public override string TypeFlag
        {
            get { return "HC"; }
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
                (int)ChainAction.None,
                (int)ChainAction.None,
                (int)RailGuidedVehicleTaskMode.None,
                Suffix);
        }

        public bool SendSuccess(Wcs.Framework.TaskableDevice taskableDevice, Wcs.Framework.DeviceCommand command)
        {
            RailGuidedVehicleDevice railGuidedVehicleDevice = (RailGuidedVehicleDevice)taskableDevice;

            return railGuidedVehicleDevice.LastStatus != null
                && railGuidedVehicleDevice.LastStatus.Event != RailGuidedVehicleEvent.远程急停;
        }
    }
}