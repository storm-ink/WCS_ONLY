using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 新增任务命令的类型
    /// </summary>
    public enum AddTaskCommandType
    {
        半自动行走=0,
        半自动取货 =1,
        半自动放货=2,

        手动左伸叉=3,
        手动右伸叉=4,
        手动前进=5,
        手动后退=6,
        手动上=7,
        手动下=8,
        手动取消任务=9,
        手动回叉中位=10,
        全自动=11,
        全自动取消=12,

        手操盒左伸叉=13,
        手操盒右伸叉=14,
        手操盒前进=15,
        手操盒后退=16,
        手操盒上=17,
        手操盒下=18,
    }
}
