using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework
{
    /// <summary>
    /// 物理动作警告信息
    /// </summary>
    public class EquipmentActionWarning
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 警告码（错误码）
        /// </summary>
        public virtual String Code { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 发生时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 错误引发者
        /// </summary>
        public virtual EquipmentAction Action { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        protected EquipmentActionWarning() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="device">设备</param>
        /// <param name="code">警告码</param>
        /// <param name="description">警告描述</param>
        public EquipmentActionWarning(Device device, string code, string description)
        {
            this.DeviceName = device.Name;
            this.Code = code;
            this.Description = description;
            this.CreatedAt = DateTime.Now;
        }
    }
}
