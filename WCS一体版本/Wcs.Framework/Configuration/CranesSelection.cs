using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Cfg
{
    public class CranesSelection:ParticularDeviceSelection
    {
        public CraneElement[] CraneElements
        {
            get
            {
                return DeviceElements.Cast<CraneElement>().ToArray();
            }
        }
        public CranesSelection(XmlNode node)
            : base(node)
        {
        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            base.Deserialize(node);

            if (!CraneControl.Config.Initialized)
            {
                CraneControl.Config.Init();
            }

            var rackLocations = from o in CraneElements.Select(x => x.Crane)
                                join x in CraneControl.Config.Crans.Where(crane => crane != null && crane.Shelf != null) on o.DeviceName.ToLower() equals x.CName.ToLower()
                                select RackLocation.ConvertTo((Crane)o, x.Shelf.dtbPBit.Rows);
            foreach (var deviceGrouping in rackLocations.SelectMany(x => x).GroupBy(x => x.Device))
            {
                Crane crane = (Crane)deviceGrouping.Key;
                crane.Locations = new RackLocation[] { new RackLocationWildcard(crane) }.Concat(deviceGrouping).ToArray();
            }
        }

        public override Type DeviceElementType
        {
            get { return typeof(CraneElement); }
        }
    }
}
