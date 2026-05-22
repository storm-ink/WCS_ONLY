using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    class HJHandShakeRecordes
    {
        public virtual Int32 Id { get; set; }
        public virtual string Location { get; set; }
        public virtual Int16 IOData { get; set; }
        public virtual UInt16 RequestID { get; set; }
        public virtual DateTime CreateTime { get; set; }

    }
}
