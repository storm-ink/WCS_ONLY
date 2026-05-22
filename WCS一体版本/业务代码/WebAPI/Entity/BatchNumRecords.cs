using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    public class BatchNumRecords
    {
        public virtual string Location { get; set; }
        public virtual string BatchNo { get; set; }
        public virtual Int32 Id { get; set; }   
        public virtual DateTime CreateTime { get; set; }
    }
}
