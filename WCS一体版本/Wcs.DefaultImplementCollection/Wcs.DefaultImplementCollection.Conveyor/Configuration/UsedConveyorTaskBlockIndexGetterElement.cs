using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class UsedConveyorTaskBlockIndexGetterElement:Wcs.Framework.Cfg.ConfigurationElement
    {
        public IUsedConveyorTaskBlockIndexGetter Getter { get; private set; }

        public UsedConveyorTaskBlockIndexGetterElement(System.Xml.XmlNode node, Wcs.Framework.Cfg.WcsConfiguration configuration)
            :base(node,configuration,true)
        {

        }
        protected override void Deserialize()
        {
            var typeName = GetAttribute<string>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (type.GetInterface(typeof(IUsedConveyorTaskBlockIndexGetter).Name) == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 “{3}” 接口", path, "type", typeName, typeof(IUsedConveyorTaskBlockIndexGetter)));
            }

            this.Getter = ReflectionHelper.CreateInstance<IUsedConveyorTaskBlockIndexGetter>(type);
        }
    }
}
