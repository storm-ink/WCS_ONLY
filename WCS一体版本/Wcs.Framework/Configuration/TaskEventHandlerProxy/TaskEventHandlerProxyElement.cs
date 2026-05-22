using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class TaskEventHandlerProxyElement:ConfigurationElement
    {
        public TaskRunningEventHandlerProxy TaskRunningEventHandlerProxy { get; private set; }
        public TaskCompletedEventHandlerProxy TaskCompletedEventHandlerProxy { get; private set; }
        public TaskErrorEventHandlerProxy TaskErrorEventHandlerProxy { get; private set; }
        public TaskRequestEventHandlerProxy TaskRequestEventHandlerProxy { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TaskEventHandlerProxyElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, false)
        {
            TaskRunningEventHandlerProxy = new TaskRunningEventHandlerProxy();
            TaskCompletedEventHandlerProxy = new TaskCompletedEventHandlerProxy();
            TaskErrorEventHandlerProxy = new TaskErrorEventHandlerProxy();
            TaskRequestEventHandlerProxy = new TaskRequestEventHandlerProxy();

            Deserialize();
        }

        protected override void Deserialize()
        {
            applyHandles(TaskRunningEventHandlerProxy, "onRunning");
            applyHandles(TaskCompletedEventHandlerProxy, "onCompleted");
            applyHandles(TaskErrorEventHandlerProxy, "onError");
            applyHandles(TaskRequestEventHandlerProxy, "onRequest");

            var taskableDevices=this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x=>x.DeviceElements)
                .Where(x=>x.Device is TaskableDevice)
                .Select(x=>x.Device as TaskableDevice);
            foreach (var device in taskableDevices)
            {
                device.TaskRunning += this.TaskRunningEventHandlerProxy.Handle;
                device.TaskCompleted += this.TaskCompletedEventHandlerProxy.Handle;
                device.TaskError += this.TaskErrorEventHandlerProxy.Handle;
                device.TaskRequest += this.TaskRequestEventHandlerProxy.Handle;
            }
        }

        void applyHandles<TArgs>(TaskEventHandlerProxy<TArgs> handlers, String handlersNodeName)
            where TArgs:HandleableEventArgs
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
                    var handler = ReflectionHelper.CreateInstance<ITaskEventHandler<TArgs>>(typeName);
                    handlers.Add(handler);
                }
            }

            if (handlers.Count == 0)
            {
                WcsConfiguration.Logger.Warn1(string.Format("{0} 节点未配置 {1} 子节点，这可能导致不正确的系统行为", this.Node.GetXPath(), handlersNodeName), this);
            }
        }
    }
}
