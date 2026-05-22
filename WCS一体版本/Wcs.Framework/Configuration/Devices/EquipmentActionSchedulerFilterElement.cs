using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public sealed class EquipmentActionSchedulerFilterElement : ConfigurationElement
    {
        public EquipmentActionSchedulerFilter EquipmentActionSchedulerFilter { get; private set; }
        public EquipmentActionSchedulerFilterElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {

        }

        protected override void Deserialize()
        {
            var typeName = GetAttribute<String>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (!type.IsSubclassOf(typeof(EquipmentActionSchedulerFilter)))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型不是 {3} 的子类", path, "type", typeName, typeof(EquipmentActionSchedulerFilter)));
            }

            this.EquipmentActionSchedulerFilter=ReflectionHelper.CreateInstance<EquipmentActionSchedulerFilter>(type);
        }
    }
}
