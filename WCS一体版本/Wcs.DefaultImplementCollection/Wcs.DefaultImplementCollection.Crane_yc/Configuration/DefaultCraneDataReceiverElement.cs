using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public sealed class DefaultCraneDataReceiverElement : DataReceiverElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DefaultCraneDataReceiverElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }

        public override IDataReceiver CreateDataReceiver(string deviceName)
        {
            return new DefaultTcpCraneDataReceiver(deviceName + "_" + this.Name, TelextTypes);
        }

        Type[] TelextTypes { get; set; }

        protected override void Deserialize()
        {
            List<Type> telexTypes = new List<Type>();
            foreach (XmlNode telexTypeNode in this.Node.SelectNodes("telex"))
            {
                if (telexTypeNode.Attributes["type"] == null || String.IsNullOrWhiteSpace(telexTypeNode.Attributes["type"].Value))
                {
                    throw new System.Configuration.ConfigurationErrorsException("必须指定 type 属性值", telexTypeNode);
                }

                var typeName = telexTypeNode.Attributes["type"].Value;
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 type 属性值指定的 {0} 类型", typeName), telexTypeNode);
                }

                if (!type.IsSubclassOf(typeof(TelexTransferObject)))
                {
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 不是 Crane TelexTransferObject 的子类", type), telexTypeNode);
                }

                telexTypes.Add(type);
            }

            if (telexTypes.Count == 0)
            {
                throw new System.Configuration.ConfigurationErrorsException("必须需要指定 1 个报文类型（telex子节点）", this.Node);
            }

            this.TelextTypes = telexTypes.ToArray();

            base.Deserialize();
        }
    }
}
