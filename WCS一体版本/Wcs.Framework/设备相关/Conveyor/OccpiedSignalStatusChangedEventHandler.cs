using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices.Conveyor
{    
    public class OccpiedSignalStatusChangedEventArgs : HandleableEventArgs
    {
        public ConveyorLocation Location { get; private set; }
        public OccupiedSignal OccupiedSignal { get; private set; }
        public OccpiedSignalStatusChangedEventArgs(ConveyorLocation location, OccupiedSignal occupiedSignal)
        {
            this.Location = location;
            this.OccupiedSignal = occupiedSignal;
        }
    }
}
