using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个申请删除任务的命令<br />
    /// 被删除的任务可以是任意状态
    /// </summary>
    public class ApplyDeleteTaskCommand:TaskCommand,Wcs.Framework.IDeviceCommandAdjudicator
    {
        public ApplyDeleteTaskCommand()
            : base()
        {

        }
        public ApplyDeleteTaskCommand(UInt32 assignmentID, UInt16 rotingNo, UInt16 startMotorNo, UInt16 destinationNo, UInt16 index, UInt16 dataId)
            :base(assignmentID,rotingNo,startMotorNo,destinationNo,index,dataId)
        {
        }

        public override TaskHandShakes HandShake
        {
            get { return TaskHandShakes.ApplyForDelete; }
        }

        public override string ToString()
        {
            return String.Format("申请删除任务命令#{1},从{2}到{3}路径为{4}分配的索引号为{5}命令", this.GetType(), this.AssignmentID, this.StartMotorNo, this.DestinationNo, this.RotingNo, this.DB1000_Index);
        }

        public bool SendSuccess(Framework.TaskableDevice taskableDevice, Framework.DeviceCommand command)
        {
            ConveyorDevice conveyorDevice = (ConveyorDevice)taskableDevice;
            ApplyDeleteTaskCommand _cmd = (ApplyDeleteTaskCommand)command;

            return conveyorDevice.Tasks != null &&
                conveyorDevice.Tasks.Length>0 && 
                conveyorDevice.Tasks[_cmd.DB1000_Index - 1].AssignmentID != _cmd.AssignmentID;
        }
    }
}
