using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Robot
{
    internal class RobotElement : Wcs.Framework.Cfg.TcpProtocolTaskableDeviceElement
    {
        public EquipmentActionToAddCommandPluginElement EquipmentActionToAddCommandPluginElement { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RobotElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {
        }

        protected override Framework.Device CreateDevice()
        {
            RobotDevice robotDevice = new RobotDevice(Name, No, ReceiveTimeout, ConnectTimeout, SendTimeout, IPEndPoint, BindEndPoint, DataReceiver);
            return robotDevice;
        }
        protected override void Deserialize()
        {
            base.Deserialize();

            var equipmentActionToAddCommandPluginNode = this.Node.SelectSingleNode("equipmentActionToAddCommandPlugin");
            if (equipmentActionToAddCommandPluginNode == null)
            {
                this.EquipmentActionToAddCommandPluginElement = null;
                ((RobotDevice)this.Device).EquipmentActionToAddCommandPlugin = null;
            }
            else
            {
                this.EquipmentActionToAddCommandPluginElement = new EquipmentActionToAddCommandPluginElement(this.Device, equipmentActionToAddCommandPluginNode, this.WcsConfiguration);
                ((RobotDevice)this.Device).EquipmentActionToAddCommandPlugin = this.EquipmentActionToAddCommandPluginElement.EquipmentActionToAddCommandPlugin;
            }
        }
    }
}
