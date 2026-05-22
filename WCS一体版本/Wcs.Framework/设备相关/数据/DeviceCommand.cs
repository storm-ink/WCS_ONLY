using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个设备命令
    /// </summary>
    [Serializable]
    public abstract class DeviceCommand : NetTransferObject
    {
        /// <summary>
        /// 获取当前命令的 <see cref="T:DeviceCommandAttribute"/> 特性
        /// </summary>
        /// <returns>如果存在则返回定义，如果不存在将返回默认特性。</returns>
        public DeviceCommandAttribute GetDeviceCommandAttribute()
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(DeviceCommandAttribute), false);
            if (attrs == null || attrs.Length == 0)
            {
                return new DeviceCommandAttribute();
            }
            else
            {
                return (DeviceCommandAttribute)attrs.Single();
            }
        }

        /// <summary>
        /// 根据XmlDocument创建command
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public virtual DeviceCommand Create(DeviceCommand cmd, XmlDocument xml)
        {
            foreach (XmlNode childNode in xml.ChildNodes)
            {
                foreach (XmlNode item in childNode.ChildNodes)
                {
                    var properInfo = cmd.GetType().GetProperties().FirstOrDefault(x => x.Name == item.Attributes["name"].Value);
                    if (properInfo != null)
                    {
                        var value = item.InnerText.Trim();
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            if (properInfo.PropertyType.Name.ToLower() == "string")
                                cmd[item.Attributes["name"].Value] = "";
                            else if (properInfo.PropertyType.Name.ToLower() == "boolean")
                                cmd[item.Attributes["name"].Value] = false;
                            else
                                cmd[item.Attributes["name"].Value] = 0;
                        }
                        else
                            cmd[item.Attributes["name"].Value] = value;
                    }
                }
            }
            return cmd;
        }
    }
}
