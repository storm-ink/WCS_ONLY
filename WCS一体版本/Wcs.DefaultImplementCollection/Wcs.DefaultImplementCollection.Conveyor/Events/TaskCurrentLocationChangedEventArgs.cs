using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class TaskCurrentLocationChangedEventArgs : HandleableEventArgs
    {
        public LocationInfoBlock LocationInfoBlock { get;private set;}
        public ConveyorLocation CurrentLocation{get;private set;}
        public TaskCurrentLocationChangedEventArgs(LocationInfoBlock locationInfoBlock, ConveyorLocation currentLocation)
        {
            this.LocationInfoBlock = locationInfoBlock;
            this.CurrentLocation=currentLocation;
        }
    }
}
