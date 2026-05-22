using System;
using System.Collections.Generic;
using System.Xml;
using Wcs.Framework.Cfg;

namespace BOE.设备相关.AGV.GSAGV
{
    public class AGVDefaultSubSystemDataReceiverElement : Wcs.Framework.Cfg.DataReceiverElement
    {
        public AGVDefaultSubSystemDataReceiverElement(System.Xml.XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }

        /// <summary>
        /// 返回一个 DefaultSubSystemDataReceiver 实例
        /// </summary>
        /// <returns></returns>
        public override Wcs.Framework.IDataReceiver CreateDataReceiver()
        {
            return new AGVDefaultSubSystemDataReceiver(this.Name);
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

                if (!type.IsSubclassOf(typeof(AGVSubSystemTelexTransferObject)))
                {
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 不是 SubSystemTelexTransferObject 的子类", type), telexTypeNode);
                }

                telexTypes.Add(type);
            }

            //if (telexTypes.Count == 0)
            //{
            //    throw new System.Configuration.ConfigurationErrorsException("必须需要指定 1 个报文类型（telex子节点）", this.Node);
            //}

            //this.TelextTypes = telexTypes.ToArray();

            base.Deserialize();
        }
    }
}
