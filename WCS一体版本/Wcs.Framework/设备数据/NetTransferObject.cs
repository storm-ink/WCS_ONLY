using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 网络传输对象.所有用于网络传输的数据都可以继承此类
    /// </summary>
    [Serializable()]
    [DataContract]
    public abstract class NetTransferObject : Comparable<NetTransferObject>
    {
        /// <summary>
        /// 索引器.
        /// 加索引器的原因是因为在对网络数据进行解码时，大量的反射操作会花费更多的时间
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns></returns>
        public abstract object this[string name] { set; }

        /// <summary>
        /// 返回表示当前 <see cref="T:System.Object" /> 的 <see cref="T:System.String" />。.<br />
        /// 形式如下：<br />
        ///     propertyName1/Description/DisplayName:propertyValue.ToString()<br />
        ///     propertyName2/Description/DisplayName:propertyValue.ToString()<br />
        ///     propertyName3/Description/DisplayName:propertyValue.ToString()<br />
        ///     .....<br />
        ///     propertyNameN/Description/DisplayName:propertyValue.ToString()<br />
        /// 即:<br />
        ///     默认为属性名称：属性值<br />
        ///     但是，当属性包含 <see cref="T:System.ComponentModel.DescriptionAttribute"/> 时，将显示 <see cref="T:System.ComponentModel.DescriptionAttribute.Description"/> 属性值<br />
        ///     或者，当属性包含 <see cref="T:System.ComponentModel.DisplayNameAttribute"/> 时，将显示 <see cref="T:System.ComponentModel.DisplayNameAttribute.DisplayName"/> 属性值<br />
        /// </summary>
        /// <returns>
        /// <see cref="T:System.String" />，表示当前的 <see cref="T:System.Object" />。.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            int maxNameLength = propertyInfos.Max(x => x.GetDisplayName().GetASCIILength());
            foreach (var property in propertyInfos)
            {
                //不处理索引器
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                sb.AppendFormat("{0}: {1}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '), property.GetValue(this,null));
            }

            return sb.ToString();
        }
    }
}