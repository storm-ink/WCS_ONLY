using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 带货行走
    /// </summary>
    public sealed class WalkWithGoodsCommand:WalkCommand
    {
        public WalkWithGoodsCommand(String taskId, UInt32 containerCode, UInt16 endStation):base(taskId,containerCode,endStation)
        {
        }

        public override RailGuidedVehicleTaskMode TaskMode
        {
            get { return RailGuidedVehicleTaskMode.WalkWithGoods; }
        }
    }
}
