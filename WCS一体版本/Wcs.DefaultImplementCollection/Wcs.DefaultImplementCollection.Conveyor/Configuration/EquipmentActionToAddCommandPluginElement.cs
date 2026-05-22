using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class EquipmentActionToAddCommandPluginElement : ConfigurationElement
    {
        public Type Type { get; private set; }
        public ConveyorEquipmentActionToAddCommandPlugin EquipmentActionToAddCommandPlugin { get; private set; }
        
        public Device Device { get; private set; }
        public EquipmentActionToAddCommandPluginElement(Device device, XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, false)
        {
            this.Device = device;

            Deserialize();
        }
        protected override void Deserialize()
        {
            var typeName = GetAttributeOrDefault<String>("type", typeof(ConveyorEquipmentActionToAddCommandPlugin).FullName);
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (!type.IsSubclassOf(typeof(ConveyorEquipmentActionToAddCommandPlugin)) && type != typeof(ConveyorEquipmentActionToAddCommandPlugin))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型不是 {3} 的子类", path, "type", typeName, typeof(EquipmentActionScheduler)));
            }

            this.Type = type;

            this.EquipmentActionToAddCommandPlugin = CreatePlugin();
        }

        protected virtual ConveyorEquipmentActionToAddCommandPlugin CreatePlugin()
        {
            if (this.EquipmentActionToAddCommandPlugin != null)
            {
                throw new InvalidOperationException("EquipmentActionScheduler 已被创建，不允许重复调用该方法。");
            }
            this.EquipmentActionToAddCommandPlugin = ReflectionHelper.CreateInstance<ConveyorEquipmentActionToAddCommandPlugin>(this.Type);

            return this.EquipmentActionToAddCommandPlugin;
        }
    }
}
