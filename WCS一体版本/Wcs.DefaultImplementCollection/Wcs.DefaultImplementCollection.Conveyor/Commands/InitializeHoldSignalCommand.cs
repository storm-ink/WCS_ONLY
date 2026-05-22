using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个初始化占位的命令
    /// </summary>
    public abstract class InitializeHoldSignalCommand : HoldSignalCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        public InitializeHoldSignalCommand()
            : base()
        {

        }
        public InitializeHoldSignalCommand(UInt16 posNo, UInt16 io_data, UInt16 index, UInt16 dataId)
            : base(posNo, io_data, index,dataId)
        {

        }
        public override HoldSignalNetTransferObjectHandShake HandShake
        {
            get { return HoldSignalNetTransferObjectHandShake.Empty; }
        }

        public override string ToString()
        {
            return String.Format("初始化占位命令,货位{1}握手{2}", this.GetType(), this.PosNo, HandShake.GetDescription());
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            InitializeHoldSignalCommand _cmd = (InitializeHoldSignalCommand)command;

            return conveyorDevice.OccupiedSignals != null &&
                conveyorDevice.OccupiedSignals.Length > 0 && 
                conveyorDevice.OccupiedSignals[_cmd.DB1023_Index - 1].HandShake == HoldSignalNetTransferObjectHandShake.Empty;

        }
    }
}
