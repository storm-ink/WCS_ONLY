using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 设备位置<br />
    /// 系统内所有路径都是通过位置来进行连通
    /// </summary>
    public abstract class Location
    {
        /// <summary>
        /// 位置所属的设备
        /// </summary>
        public Device Device { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public abstract String UserCode { get; }
        /// <summary>
        /// 该位置在设备中的编码形式
        /// </summary>
        public abstract String DeviceCode { get;}

        /// <summary>
        /// 类型
        /// </summary>
        public abstract LocationType Type { get; }

        /// <summary>
        /// 和此对象表示的位置一样，但表述不一样的对象
        /// 如00-101-001（输送线 1 号货位） 和 01-000-001（堆垛机 1 排 0 列 1层）
        /// </summary>
        public Location[] SameAs { get; set; }

        public override int GetHashCode()
        {
            return string.Concat("{0}_{1}_{2}", Device.DeviceType, Device.Name, this.DeviceCode).GetHashCode();
        }

        public override string ToString()
        {
            return this.UserCode;
        }
        /// <summary>
        /// 转换为系统可识别（可二次转换的）编码值
        /// </summary>
        /// <returns>字符串，格式为：位置在设备中的编码形式@设备名称，如：01001001@c001</returns>
        public String GetConvertibleCode()
        {
            return String.Format("{0}@{1}", this.DeviceCode, this.Device.Name);
        }
    }
}
