using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class DataReceiverCollection:ConfigurationElement
    {
        public DataReceiverElement[] DataReceiverElements { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DataReceiverCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration,true)
        {
            if (this.DataReceiverElements == null)
            {
                DataReceiverElements = new DataReceiverElement[0];
            }
        }

        protected override void Deserialize()
        {
            List<DataReceiverElement> dataReceiverElements = new List<DataReceiverElement>();
            foreach (XmlNode item in this.Node.SelectNodes("receiver"))
            {
                var typeName = GetAttribute<string>("type", item);
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    var path = item.GetXPath();
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
                }

                if (!type.IsSubclassOf(typeof(DataReceiverElement)))
                {
                    var path = item.GetXPath();
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 {3} 接口", path, "type", typeName, typeof(DataReceiverElement)));
                }

                var receiverElement = ReflectionHelper.CreateInstance<DataReceiverElement>(type, item,this.WcsConfiguration);
                if(dataReceiverElements.Any(x=>string.Equals(x.Name,
                    receiverElement.Name,
                    StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 重复",item.GetXPath(),"name",receiverElement.Name));
                }

                dataReceiverElements.Add(receiverElement);
            }

            this.DataReceiverElements = dataReceiverElements.ToArray();
        }
    }
}
