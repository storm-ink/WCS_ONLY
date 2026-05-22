using System;

namespace Wcs.Framework
{
    /// <summary>
    /// 指定设备命令的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class DeviceCommandAttribute : Attribute
    {
        public DeviceCommandAttribute()
        {
            this.IsEquipmentCommand = true;
        }

        /// <summary>
        /// <para>指示包含止特性的类型是否是一个设备命令</para>
        /// <para>注：设备命令才需要在 sendtask 方法中通过与设备的通讯接口转换成流发送给设备，否则不允许发送给物理设备。</para>
        /// </summary>
        public Boolean IsEquipmentCommand { get; set; }
    }
}