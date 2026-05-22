using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public class DeviceWarningEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 错误
        /// </summary>
        public DeviceWarning Warning { get; protected set; }
        public DeviceWarningEventArgs(DeviceWarning warning)
        {
            this.Warning = warning;
        }
    }
}
