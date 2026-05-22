using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Wcs.Framework
{
    /// <summary>
    /// 网络传输对象.所有用于网络传输的数据都可以继承此类
    /// </summary>
    [Serializable()]
    [DataContract]
    [JsonObject]
    public abstract class NetTransferObject : Comparable<NetTransferObject>
    {
        /// <summary>
        /// 索引器.
        /// 加索引器的原因是因为在对网络数据进行解码时，大量的反射操作会花费更多的时间
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns></returns>
        public abstract object this[string name] { get; set; }
        /// <summary>
        /// 本数据在数据包中的位置（数据包该数据区中的位置）
        /// </summary>
        [System.ComponentModel.DisplayName("存放位置")]
        public Int32 AtPacketIndex { get; set; }

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
            foreach (var property in propertyInfos)
            {
                //不处理索引器
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    sb.AppendFormat(",{0}={1}", property.GetDisplayName(), property.GetValue(this, null));
                }
                else
                {
                    sb.AppendFormat("{0}={1}", property.GetDisplayName(), property.GetValue(this, null));
                }
            }

            return String.Format(@"{0}#{{{1}}}",this.GetType().Name,sb.ToString());
        }

        public Dictionary<String, String> ToPropertyValues()
        {
            Dictionary<String, String> result = new Dictionary<String, String>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            int maxNameLength = propertyInfos.Max(x => x.GetDisplayName().GetASCIILength());
            foreach (var property in propertyInfos)
            {
                //不处理索引器
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                result.Add(property.GetDisplayName(), Convert.ToString(property.GetValue(this, null)));
            }

            return result;
        }

        public String ToTipContent()
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

                sb.AppendFormat("{0}: {1}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '), property.GetValue(this, null));
            }

            return sb.ToString();
        }
        /// <summary>
        /// 将传输对象转换为日志数据
        /// </summary>
        /// <param name="device">设备</param>
        /// <returns>可返回null,返回null表示该数据不需要记录到运行数据库中。</returns>
        //public abstract ReceivedDataLog ToLogData(Device device);

        public virtual Object GetDataGridViewShow()
        {
            return this;
        }
    }
}