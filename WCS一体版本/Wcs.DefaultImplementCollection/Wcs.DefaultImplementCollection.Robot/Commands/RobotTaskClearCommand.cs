using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手删除任务命令
    /// </summary>
    public class RobotTaskClearCommand : RobotTaskCommand
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotTaskClearCommand()
            :base()
        {
            HandShake = HandShake.Delete;
            Raw = "00000000000000000000";
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            var robot = (RobotDevice)taskableDevice;
            if (robot.LastState == null)
                return false;
            return robot.LastState.RobotTask.TaskId == 0;              
        }
    }
}