using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 设备故障处理程序
    /// </summary>
    public class EquipmentFailureEventHandlersElement:ConfigurationElement
    {
        /// <summary>
        /// 故障被添加（新产生）的事件处理程序
        /// </summary>
        public EquipmentFailureAddedEventHandler AddedEventHandlerProxy { get; private set; }
        /// <summary>
        /// 故障被删除（已恢复）的事件处理程序
        /// </summary>
        public EquipmentFailureRemovedEventHandler RemovedEventHandlerProxy { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public EquipmentFailureEventHandlersElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            AddedEventHandlerProxy = new EquipmentFailureAddedEventHandler();
            RemovedEventHandlerProxy = new EquipmentFailureRemovedEventHandler();

            Deserialize();
        }

        protected override void Deserialize()
        {
            applyHandles(AddedEventHandlerProxy, "onAdded");
            applyHandles(RemovedEventHandlerProxy, "onRemoved");

            var taskableDevices=this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x=>x.DeviceElements)
                .Select(x=>x.Device);
            foreach (var device in taskableDevices)
            {
                device.EquipmentFailureAdded += this.AddedEventHandlerProxy.Handle;
                device.EquipmentFailureRemoved += this.RemovedEventHandlerProxy.Handle;
            }
        }

        void applyHandles<TArgs>(EquipmentFailureEventHandler<TArgs> handlers, String handlersNodeName)
            where TArgs:EquipmentFailureEventArgs
        {
            XmlNode handlersNode = this.Node.SelectSingleNode(handlersNodeName);
            if (handlersNode != null)
            {
                foreach (XmlNode handerNode in handlersNode.SelectNodes("handler"))
                {
                    String typeName = GetAttribute<String>("type", handerNode);
                    if (String.IsNullOrWhiteSpace(typeName))
                    {
                        throw new ConfigurationErrorsException(String.Format("{0} 节点的 {1} 属性值不能为空", handerNode.GetXPath(), "type"));
                    }
                    var handler = ReflectionHelper.CreateInstance<IEquipmentFailureEventHandler<TArgs>>(typeName);
                    handlers.Add(handler);
                }
            }

            if (handlers.Count == 0)
            {
                WcsConfiguration._logger.Warn1(string.Format("{0} 节点未配置 {1} 子节点，这可能导致不正确的系统行为", this.Node.GetXPath(), handlersNodeName), this);
            }
        }
    }
}
