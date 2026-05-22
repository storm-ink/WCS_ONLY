using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 取货命令
    /// </summary>
    public class LoadCommand : AddTaskCommand
    {
        public LoadCommand(String taskId, RackLocation pickingLocation, string barcode, bool needCheck)
            : base(taskId, AddTaskCommandType.半自动取货, pickingLocation, pickingLocation,barcode, needCheck)
        {
            var currentLocation = ((CraneDevice)pickingLocation.Device).GetCurrentLocation();
            if (currentLocation == null)
            {
                throw new InvalidOperationException(string.Format("{0}无法获取当前位置，无法生成取货命令", pickingLocation.Device));
            }

            if (pickingLocation.Column != currentLocation.Column || pickingLocation.Level != currentLocation.Level)
            {
                throw new InvalidOperationException(string.Format("{0}不在取货位置，无法生成取货命令", pickingLocation.Device));
            }

            if (!pickingLocation.Enabled)
            {
                throw new InvalidOperationException(string.Format("{0}已被禁用，无法生成取货命令", pickingLocation));
            }

            if (!pickingLocation.IsForkActionAllowed(ForkAction.Pickup))
            {
                throw new InvalidOperationException(string.Format("{0}不允许取货动作，无法生成取货命令", pickingLocation));
            }
        }
    }
}
