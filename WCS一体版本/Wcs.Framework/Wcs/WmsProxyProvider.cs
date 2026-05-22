using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// WmsProxy 提供程序
    /// </summary>
    public class WmsProxyProvider
    {
        static IWmsProxy proxy;
        static WmsProxyProvider()
        {
            lock (typeof(WmsProxyProvider))
            {
                if (proxy != null)
                {
                    return;
                }

                if (!Configuration.Initialized)
                {
                    throw new Exception("尝试在配置未初始化前访问 Configuration 对象");
                }

                XmlNode node = Configuration.GetSelection("wmsProxy");
                if (node == null)
                {
                    throw new Exception("未配置 wmsProxy 节点");
                }

                if (node.Attributes["type"] == null || String.IsNullOrWhiteSpace(node.Attributes["type"].Value))
                {
                    throw new Exception("请先指定 wmsProxy 节点 type 属性");
                }

                String typeName = node.Attributes["type"].Value;
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new Exception("未找到类型 " + typeName);
                }

                proxy = (IWmsProxy)type.Assembly.CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, null, null, null);
            }
        }
        private WmsProxyProvider() { }
        /// <summary>
        /// 获取绑定的 WmsProxy 对象
        /// </summary>
        public static IWmsProxy Proxy
        {
            get
            {
                return proxy;
            }
        }
    }
}
