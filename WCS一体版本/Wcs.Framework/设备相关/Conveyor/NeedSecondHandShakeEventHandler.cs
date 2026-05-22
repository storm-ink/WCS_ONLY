using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices.Conveyor
{
    public class NeedSecondHandShakeEventArgs : HandleableEventArgs
    {
        public TaskBlock TaskBlock { get; private set; }

        public NeedSecondHandShakeEventArgs(TaskBlock taskBlock)
        {
            this.TaskBlock = taskBlock;
        }
    }
}
