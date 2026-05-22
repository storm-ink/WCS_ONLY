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
        Location[] _sameAs;

        #region Properities
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
        /// 和此对象表示的位置一样，但表述不一样的对象<br />
        /// 如00-101-001（输送线 1 号货位） 和 01-000-001（堆垛机 1 排 0 列 1层）<br />
        /// 该属性永远不会返回NULL
        /// </summary>
        public Location[] SameAs
        {
            get
            {
                if (_sameAs == null)
                {
                    return new Location[0];
                }
                return _sameAs;
            }
            set
            {
                if (value != null)
                {
                    if (value.Any(x => x.SameAs.Length > 0))
                    {
                        throw new InvalidOperationException("SameAs 属性不支持嵌套的 SameAs 属性。也就是说，你不能将设置了 SameAs 属性值的 Location 对象添加到另一个对象的 SameAs 属性值中。");
                    }
                }

                _sameAs = value;
            }
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var loc = obj as Location;
            if (loc == null)
            {
                return false;
            }

            if (loc is ILocationWildcard)
            {
                return this.Device.Equals(loc.Device) || this.SameAs.Any(x => x.Device.Equals(loc.Device)) || loc.SameAs.Any(x => x.Device.Equals(this.Device));
            }

            return base.Equals(obj) || this.SameAs.Any(x => ReferenceEquals(loc, x)) || loc.SameAs.Any(x => ReferenceEquals(this, x));
        }

        public override string ToString()
        {
            return this.UserCode;
        }
        #endregion

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
