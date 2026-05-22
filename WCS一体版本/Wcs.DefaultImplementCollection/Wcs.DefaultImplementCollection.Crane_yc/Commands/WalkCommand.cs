using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 行走命令
    /// </summary>
    public class WalkCommand : AddTaskCommand
    {
        public WalkCommand(String taskId, RackLocation startLocation, RackLocation endLocation, string barcode, bool needCheck)
            :base(taskId,AddTaskCommandType.半自动行走,startLocation,endLocation,barcode, needCheck)
        {
            if (!endLocation.Enabled)
            {
                throw new InvalidOperationException(string.Format("{0}已被禁用，无法生成行走命令", endLocation));
            }
        }
    }
}
