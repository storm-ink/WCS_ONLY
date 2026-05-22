using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 放货命令
    /// </summary>
    public class UnloadCommand : AddTaskCommand
    {
        public UnloadCommand(String taskId, RackLocation puttingLocation, string barcode,bool needCheck)
            : base(taskId, AddTaskCommandType.半自动放货, puttingLocation, puttingLocation, barcode, needCheck)
        {
            var currentLocation = ((CraneDevice)puttingLocation.Device).GetCurrentLocation();
            if (currentLocation == null)
            {
                throw new InvalidOperationException(string.Format("{0}无法获取当前位置，无法生成放货命令", puttingLocation.Device));
            }

            if (puttingLocation.Column != currentLocation.Column || puttingLocation.Level != currentLocation.Level)
            {
                throw new InvalidOperationException(string.Format("{0}不在放货位置，无法生成放货命令", puttingLocation.Device));
            }

            if (!puttingLocation.Enabled)
            {
                throw new InvalidOperationException(string.Format("{0}已被禁用，无法生成放货命令", puttingLocation));
            }

            if (!puttingLocation.IsForkActionAllowed(ForkAction.Putdown))
            {
                throw new InvalidOperationException(string.Format("{0}不允许放货动作，无法生成放货命令", puttingLocation));
            }
        }
    }
}
