using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class DeviceWarningEventHandlersElement:ConfigurationElement
    {
        public DeviceWarningEventHandler DeviceWarningEventHandler { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DeviceWarningEventHandlersElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            Deserialize();
        }

        protected override void Deserialize()
        {
            List<IDeviceErrorEventHandler> handlers = new List<IDeviceErrorEventHandler>();
            foreach (XmlNode handerNode in this.Node.SelectNodes("handler"))
            {
                String typeName = GetAttribute<String>("type", handerNode);
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                }

                var handler = ReflectionHelper.CreateInstance<IDeviceErrorEventHandler>(typeName);

                handlers.Add(handler);
            }

            this.DeviceWarningEventHandler = new Framework.DeviceWarningEventHandler(handlers);
            
            var devices = this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Select(x => x.Device);

            foreach (var device in devices)
            {
                device.DeviceWarning += this.DeviceWarningEventHandler.Handle;
            }
        }
    }
}
