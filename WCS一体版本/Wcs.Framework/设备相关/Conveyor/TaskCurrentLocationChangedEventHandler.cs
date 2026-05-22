using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices.Conveyor
{
    public class TaskCurrentLocationChangedEventArgs : HandleableEventArgs
    {
        public LocationCurrentTask LocationCurrentTask{get;private set;}
        public ConveyorLocation CurrentLocation{get;private set;}
        public TaskCurrentLocationChangedEventArgs(LocationCurrentTask locationCurrentTask, ConveyorLocation currentLocation)
        {
            this.LocationCurrentTask=locationCurrentTask;
            this.CurrentLocation=currentLocation;
        }
    }
}
