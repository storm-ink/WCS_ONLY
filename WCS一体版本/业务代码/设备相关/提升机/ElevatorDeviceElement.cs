using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using Wcs;

namespace BOE
{
    public class ElevatorDeviceElement : TaskableDeviceElement
    {
        public String OwnerConveyorDeviceName { get; private set; }

        public String PosNo { get; private set; }
        public ElevatorDeviceElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, false)
        {
            this.OwnerConveyorDeviceName = this.GetAttribute<String>("ownerConveyorDevice");

            if (string.IsNullOrWhiteSpace(this.OwnerConveyorDeviceName))
            {
                throw new ConfigurationErrorsException(String.Format("未指定 {0} 节点的 {1} 属性值", this.Node.GetXPath(), "ownerConveyorDevice"), this.Node);
            }
            this.PosNo = this.GetAttribute<String>("posNo");

            if (string.IsNullOrWhiteSpace(this.PosNo))
            {
                throw new ConfigurationErrorsException(String.Format("未指定 {0} 节点的 {1} 属性值", this.Node.GetXPath(), "posNo"), this.Node);
            }
            this.Deserialize();
        }

        protected override Device CreateDevice()
        {

            return new ElevatorDevice(this.Name, this.No, this.ReceiveTimeout, this.ConnectTimeout, this.SendTimeout, this.OwnerConveyorDeviceName, this.PosNo);
        }
    }
}
