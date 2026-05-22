using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 无货行走
    /// </summary>
    public sealed class NormalWalkCommand : WalkCommand
    {
        public NormalWalkCommand(String taskId, UInt32 containerCode, UInt16 endStation)
            : base(taskId, containerCode, endStation)
        {
        }
        public override RailGuidedVehicleTaskMode TaskMode
        {
            get { return RailGuidedVehicleTaskMode.Walk; }
        }
    }
}
