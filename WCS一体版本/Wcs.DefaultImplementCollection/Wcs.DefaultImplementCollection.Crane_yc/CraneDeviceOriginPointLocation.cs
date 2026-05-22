using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 堆垛机原点位置
    /// </summary>
    public class CraneDeviceOriginPointLocation : RackLocation
    {
        public CraneDeviceOriginPointLocation(CraneDevice device, String userCode, int column, int level, int userColumn, int userLevel)
            : base(device, userCode, null, ForkAction.None, column, level, -1, userColumn, userLevel, true, 0)
        {

        }
        public override ForkDirection? ForkDirection
        {
            get
            {
                return null;
            }
            protected set
            {

            }
        }
    }
}
