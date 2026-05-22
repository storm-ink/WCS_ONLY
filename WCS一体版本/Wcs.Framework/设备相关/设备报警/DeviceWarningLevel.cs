using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{

    public enum DeviceWarningLevel
    {
        /// <summary>
        /// 警告
        /// 在不需要人工介入或较短的介入时间可以解决的问题，可快速重新运行的报警
        /// </summary>
        Warning,
        /// <summary>
        /// 故障
        /// 在需要人工介入并且需要较长的时间，会影响到正常工作的报警
        /// </summary>
        Fault,
    }

}
