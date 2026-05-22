using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个申请删除占位的命令
    /// </summary>
    public sealed class ApplyDeleteHoldSignalCommand : HoldSignalCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        public ApplyDeleteHoldSignalCommand()
            : base()
        {

        }
        public ApplyDeleteHoldSignalCommand(UInt16 posNo, UInt16 io_data, UInt16 index, UInt16 dataId)
            : base(posNo, io_data, index,dataId)
        {

        }
        public override HoldSignalNetTransferObjectHandShake HandShake
        {
            get { return HoldSignalNetTransferObjectHandShake.ApplyToDelete; }
        }
        public override string ToString()
        {
            return String.Format("申请删除占位命令,货位{1}握手{2}", this.GetType(), this.PosNo, HandShake.GetDescription());
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            ApplyDeleteHoldSignalCommand _cmd = (ApplyDeleteHoldSignalCommand)command;

            return conveyorDevice.OccupiedSignals != null &&
                conveyorDevice.OccupiedSignals.Length > 0 && 
                conveyorDevice.OccupiedSignals[_cmd.DB1023_Index - 1].HandShake != HoldSignalNetTransferObjectHandShake.New;
        }
    }
}
