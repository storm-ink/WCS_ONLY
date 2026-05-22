using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class CompareResultHandlerElement:Wcs.Framework.Cfg.ConfigurationElement
    {
        public CompareResultHandlerElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {

        }

        public ICompareResultHandler Handler { get; private set; }
        protected override void Deserialize()
        {
            var typeName = GetAttribute<string>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (type.GetInterface(typeof(ICompareResultHandler).Name)==null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 “{3}” 接口", path, "type", typeName,typeof(ICompareResultHandler)));
            }

            this.Handler = ReflectionHelper.CreateInstance<ICompareResultHandler>(type);
        }
    }
}
