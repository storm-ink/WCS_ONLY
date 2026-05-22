using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public abstract class DataReceiverElement:ConfigurationElement
    {
        public String Name { get; private set; }
        public Type Type { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DataReceiverElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {
        }

        /// <summary>
        /// 创建一个新的数据接收器
        /// </summary>
        /// <returns></returns>
        public abstract IDataReceiver CreateDataReceiver(string deviceName);

        protected override void Deserialize()
        {
            this.Name = GetAttribute<String>("name");
            var typeName = GetAttribute<String>("type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (!type.IsSubclassOf(typeof(DataReceiverElement)))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型未实现 {3} 接口", path, "type", typeName, typeof(DataReceiverElement)));
            }

            this.Type = type;
        }
    }
}
