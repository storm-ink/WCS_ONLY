using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    public class CraneElement : DeviceElement
    {
        public UpdateStatusActionsSelection UpdateStatusActionsSelection { get; private set; }
        public Crane Crane
        {
            get
            {
                return (Crane)this.Device;
            }
        }
        public CraneElement(XmlNode node)
            : base(node)
        {
        }
        

        public override Location CreateLocation(XmlNode locationNode)
        {
            throw new NotImplementedException();
        }

        public override void ResolveLocationSameAs()
        {
            //throw new NotImplementedException();
        }

        protected override void Deserialize(System.Xml.XmlNode node)
        {
            bool useRemoteCraneWorkModeKeyInfo = true;
            if (node.Attributes["useRemoteCraneWorkModeKeyInfo"] != null)
            {
                bool.TryParse(node.Attributes["useRemoteCraneWorkModeKeyInfo"].Value, out useRemoteCraneWorkModeKeyInfo);
            }
            Crane crane = new Crane(DeviceName, Ip, Port, ReceiveTimeout, ConnectTimeout, SendTimeout, DeviceNo, LogTarget, useRemoteCraneWorkModeKeyInfo)
            {
                AllowConcurrency = AllowConcurrency
            };
            this.Device = crane;

            this.UpdateStatusActionsSelection = new UpdateStatusActionsSelection(node.SelectSingleNode("updateStatusActions"));
            crane.UpdateStatusActions = this.UpdateStatusActionsSelection.UpdateStatusActionElements.Select(x => x.UpdateStatusAction).ToArray();

            this.SequenceSelection = new SequenceSelection(node.SelectSingleNode("sequence"), this);
        }

        public override Location[] GetLocations()
        {
            return this.Crane.Locations;
        }
    }
}
