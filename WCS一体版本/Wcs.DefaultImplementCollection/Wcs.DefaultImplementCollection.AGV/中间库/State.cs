using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.AGV
{
   public enum State
    {
       设备占用=1,
       AGV申请进入=2,
       设备释放占用=3,
       AGV占用=4,
       AGV释放=5
    }
}
