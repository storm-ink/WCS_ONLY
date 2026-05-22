using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 该对象表示设备的具体报警信息
    /// </summary>
    public class DeviceWarning
    {
        /// <summary>
        /// 设备
        /// </summary>
        public Device Deivce { get; set; }
        /// <summary>
        /// 报警源，通常报警指的是设备报警。但在某些情况下可能报警来自于设备中的某些位置或其它对象
        /// </summary>
        public Object Source { get; set; }
        /// <summary>
        /// 错误等级
        /// </summary>
        public DeviceWarningLevel Level { get; set; }
        /// <summary>
        /// 报警码
        /// </summary>
        public String Code { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public String Description { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public String Category { get; set; }
        /// <summary>
        /// 是否属于故障
        /// </summary>
        public Boolean IsFault { get; private set; }

        public DeviceWarning(Device device, Object source, DeviceWarningLevel level, String code, String description, Boolean isFault)
        {
            this.Deivce = device;
            this.Source = source;
            this.Level = level;
            this.Code = code;
            this.Description = description;
            this.IsFault = isFault;
        }
    }
}
