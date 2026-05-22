using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.FrameworkExtend
{
    public static class PreTaskSchedulerFilterHelper
    {
        public static Dictionary<string, ActionSchedulerFilterResult> lastPreTaskSchedulerFilterResult = new Dictionary<string, ActionSchedulerFilterResult>();
    }
}
