using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public abstract class TaskableDeviceElement :DeviceElement
    {
        /// <summary>
        /// 是否允许并发任务（同时接受多个任务）
        /// </summary>
        public Boolean AllowConcurrency { get; protected set; }
        /// <summary>
        /// 任务发送超时时间
        /// </summary>
        public Int32 SendTimeout { get; protected set; }
        /// <summary>
        /// 任务执行器配置
        /// </summary>
        public EquipmentActionSchedulerElement EquipmentActionSchedulerElement { get; private set; }
        /// <summary>
        /// 任务逻辑动作配置
        /// </summary>
        public LogicMovementElement[] LogicMovementElements { get; private set; }
        /// <summary>
        /// 任务逻辑动作选择器
        /// </summary>
        public LogicMovementSelectorElement LogicMovementSelectorElement { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TaskableDeviceElement(XmlNode node, WcsConfiguration configuration)
            : this(node,configuration, true)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public TaskableDeviceElement(XmlNode node, WcsConfiguration configuration, Boolean deserialize)
            : base(node,configuration,false)
        {
            AllowConcurrency = GetAttributeOrDefault<Boolean>("allowConcurrency");
            SendTimeout = GetAttributeOrDefault<Int32>("sendTimeout", 2000);
            

            if (deserialize)
            {
                Deserialize();
            }
        }
        protected override void Deserialize()
        {
            base.Deserialize();

            var schedulerNode = this.Node.SelectSingleNode("equipmentActionScheduler");
            if (schedulerNode == null)
            {
                schedulerNode = Node.OwnerDocument.CreateNode(XmlNodeType.Element, "equipmentActionScheduler", null);
                var schedulerNodeTypeAttr = Node.OwnerDocument.CreateAttribute("type");
                schedulerNodeTypeAttr.Value = GetAttributeOrDefault<String>("equipmentActionScheduler", typeof(EquipmentActionScheduler).FullName);
                schedulerNode.Attributes.Append(schedulerNodeTypeAttr);
                this.Node.AppendChild(schedulerNode);
            }

            this.EquipmentActionSchedulerElement = new EquipmentActionSchedulerElement(this.Device, schedulerNode, this.WcsConfiguration);
            ((TaskableDevice)this.Device).EquipmentActionScheduler = this.EquipmentActionSchedulerElement.EquipmentActionScheduler;

            var logicMovementSelectorNode = this.Node.SelectSingleNode("logicMovementSelector");
            if (logicMovementSelectorNode == null)
            {
                logicMovementSelectorNode = this.Node.OwnerDocument.CreateNode(XmlNodeType.Element, "logicMovementSelector", null);
                String typeName = GetAttributeOrDefault<String>("logicMovementSelector", typeof(DefaultLogicMovementSelector).FullName);
                XmlAttribute logicMovementSelectorNodeAttr = this.Node.OwnerDocument.CreateAttribute("type");
                logicMovementSelectorNodeAttr.Value = typeName;
                logicMovementSelectorNode.Attributes.Append(logicMovementSelectorNodeAttr);
                this.Node.AppendChild(logicMovementSelectorNode);
            }
            this.LogicMovementSelectorElement = new LogicMovementSelectorElement(logicMovementSelectorNode, this.WcsConfiguration);
            ((TaskableDevice)this.Device).LogicMovementSelector = this.LogicMovementSelectorElement.LogicMovementSelector;
            ((TaskableDevice)this.Device).LogicMovementTypes = this.LogicMovementSelectorElement.LogicMovementElement.Select(x => x.Type).ToArray();
        }
    }
}
