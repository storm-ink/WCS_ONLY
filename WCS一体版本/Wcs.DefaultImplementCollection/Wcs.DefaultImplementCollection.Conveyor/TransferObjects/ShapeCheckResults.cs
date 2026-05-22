using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    [Description("检测结果")]
    public enum ShapeCheckResults
    {
        [Description("未知")]
        UnKnow = 0,
        [Description("成功")]
        OK = 1,
        [Description("失败")]
        Error=2,
        [Description("检测中")]
        Checking = 3
    }
}
