using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class Keeping
    {
        public virtual String UserCode {get;set;}
        public virtual Boolean IsEmpty { get; set; }
        public virtual Iesi.Collections.Generic.ISet<String> ContainerCodes { get; protected set; }
        public virtual Int32 TaskId { get; set; }
    }
}
