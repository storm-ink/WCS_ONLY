using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
#warning 此玩意有问题
    public class ConveyorDataReceiverElement : DataReceiverElement
    {
        public XmlNode SenderEncodingNode { get; private set; }
        public XmlNode ReceiverEncodingNode { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConveyorDataReceiverElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }

        protected override void Deserialize()
        {
            base.Deserialize();

            var db1Node = Node.SelectSingleNode("senderEncoding");
            if (db1Node == null)
            {
                var path = Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点必须配置 db1 子节点", path));
            }
            SenderEncodingNode = this.GetNodeOrLoadXmlFile(db1Node);

            var db2Node = Node.SelectSingleNode("receiverEncoding");
            if (db2Node == null)
            {
                var path = Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点必须配置 db2 子节点", path));
            }
            ReceiverEncodingNode = this.GetNodeOrLoadXmlFile(db2Node);
        }

        public override Framework.IDataReceiver CreateDataReceiver(string deviceName)
        {
            return new DefaultConveyorTcpProtocolDataReceiver(deviceName + "_" + this.Name, SenderEncodingNode, ReceiverEncodingNode);
        }
    }
}
