using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class TaskConfirmEventArgs : HandleableEventArgs
    {
        public TaskBlock TaskBlock { get; private set; }

        public TaskConfirmEventArgs(TaskBlock taskBlock)
        {
            this.TaskBlock = taskBlock;
        }
    }
}
