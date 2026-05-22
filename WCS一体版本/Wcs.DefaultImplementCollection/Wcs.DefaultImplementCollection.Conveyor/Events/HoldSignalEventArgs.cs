using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{    

    public class RequestEventArgs : HandleableEventArgs
    {
        public ConveyorLocation Location { get; private set; }
        public RequestBlock RequestBlock { get; private set; }
        public RequestEventArgs(ConveyorLocation location, RequestBlock requestBlock)
        {
            this.Location = location;
            this.RequestBlock = requestBlock;
        }
    }
}
