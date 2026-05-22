using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public class HoldSignalInfo : Comparable<HoldSignalInfo>, IComparer<HoldSignalInfo>
    {
        public HoldSignalInfo()
        {

        }
        public virtual Int32 AssignmentID { get; set; }
        public virtual Int32 TU_ID { get; set; }
        public virtual Int32 TU_Type { get; set; }
        public virtual Int32 IO_Data { get; set; }

        public virtual int Compare(HoldSignalInfo x, HoldSignalInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.AssignmentID != y.AssignmentID
             || x.IO_Data != y.IO_Data
             || x.TU_Type != y.TU_Type
             || x.IO_Data != y.IO_Data
                )
            {
                return 1;
            }

            return 0;
        }
    }
}
