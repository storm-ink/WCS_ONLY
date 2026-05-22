using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个申请清空任务的命令<br />
    /// 被清空的任务必须是已完成状态
    /// </summary>
    public class ApplyClearTaskCommand : TaskCommand, Wcs.Framework.IDeviceCommandAdjudicator
    {
        public ApplyClearTaskCommand():base()
        {

        }
        public ApplyClearTaskCommand(UInt32 assignmentID, UInt16 rotingNo, UInt16 startMotorNo, UInt16 destinationNo, UInt16 index, UInt16 dataId)
            : base(assignmentID, rotingNo, startMotorNo, destinationNo, index, dataId)
        {
        }

        public override TaskHandShakes HandShake
        {
            get { return TaskHandShakes.ApplyForClear; }
        }

        public override string ToString()
        {
            return String.Format("申请清空任务命令#{1},从{2}到{3}路径为{4}分配的索引号为{5}命令", this.GetType(), this.AssignmentID, this.StartMotorNo, this.DestinationNo, this.RotingNo, this.DB1000_Index);
        }

        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            ApplyClearTaskCommand _cmd = (ApplyClearTaskCommand)command;

            return conveyorDevice.Tasks != null &&
                conveyorDevice.Tasks[_cmd.DB1000_Index - 1].AssignmentID != _cmd.AssignmentID;
        }
    }
}
