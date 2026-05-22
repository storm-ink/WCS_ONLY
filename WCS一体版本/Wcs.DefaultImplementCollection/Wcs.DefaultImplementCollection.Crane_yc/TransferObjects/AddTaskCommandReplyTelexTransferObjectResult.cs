using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public enum AddTaskCommandReplyTelexTransferObjectResult
    {
        接收正确=1,
        接收不正确=2,
        设备忙=3,
        起点位置不正确=4,
        终点位置不正确=5
    }
}
