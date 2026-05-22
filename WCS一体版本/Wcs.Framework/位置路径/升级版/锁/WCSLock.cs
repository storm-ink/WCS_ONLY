using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Lock
{
    /// <summary>
    /// WCS锁
    /// </summary>
    public class WCSLock
    {
        public virtual int LockId { get; set; }
        public virtual ActionInfo ActionInfo { get; set; }
        public virtual WCSLockInfo LockInfo { get; set; }

        public virtual string LockDescription { get; set; }

        public WCSLock() { }
    }
}
