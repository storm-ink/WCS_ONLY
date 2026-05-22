using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个确认任务的命令（二次握手）
    /// </summary>
    public class ConfirmTaskCommand : TaskCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        public ConfirmTaskCommand()
            : base()
        {
        }
        public ConfirmTaskCommand(UInt32 assignmentID, UInt16 rotingNo, UInt16 startMotorNo, UInt16 destinationNo, UInt16 index, UInt16 dataId)
            : base(assignmentID, rotingNo, startMotorNo, destinationNo, index, dataId)
        {
        }

        public override TaskHandShakes HandShake
        {
            get { return TaskHandShakes.SecondConfirm; }
        }

        public override string ToString()
        {
            return String.Format("二次握手任务命令#{0},从{1}到{2}路径为{3}分配的索引号为{4}命令", this.AssignmentID, this.StartMotorNo, this.DestinationNo, this.RotingNo, this.DB1000_Index);
        }

        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            ConfirmTaskCommand _cmd = (ConfirmTaskCommand)command;

            return conveyorDevice.Tasks != null &&
                conveyorDevice.Tasks.Length > 0 && 
                conveyorDevice.Tasks[_cmd.DB1000_Index - 1].HandShake == TaskHandShakes.SecondConfirm;
        }
    }
}
