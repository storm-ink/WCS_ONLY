using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    public class DeviceErrorEventArgs:EventArgs
    {
        /// <summary>
        /// 错误编码
        /// </summary>
        public String ErrorCode { get; private set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public String ErrorDescription { get; private set; }
        /// <summary>
        /// 指示此事件是否已被处理
        /// </summary>
        public Boolean Handled { get; set; }
        public DeviceErrorEventArgs(String code, String description)
        {
            this.ErrorCode = code;
            this.ErrorDescription = description;
            this.Handled = false;
        }
    }
}
