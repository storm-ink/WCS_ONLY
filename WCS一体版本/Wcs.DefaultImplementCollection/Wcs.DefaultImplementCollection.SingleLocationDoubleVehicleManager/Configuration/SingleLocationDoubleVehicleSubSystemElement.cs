using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class SingleLocationDoubleVehicleSubSystemElement : Wcs.Framework.Cfg.TaskableDeviceElement
    {
        public String[] railGuidVehileDeviceNames { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SingleLocationDoubleVehicleSubSystemElement(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration,true)
        {
        }

        protected override void Deserialize()
        {
            var vehileDeviceNames = GetAttributeOrDefault<String>("vehileDeviceNames");
            if (!string.IsNullOrWhiteSpace(vehileDeviceNames))
                this.railGuidVehileDeviceNames = vehileDeviceNames.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            else
                this.railGuidVehileDeviceNames = new String[0];

            base.Deserialize();
        }

        protected override Framework.Device CreateDevice()
        {
            SingleLocationDoubleVehicleSubSystem subSystemDevice = new SingleLocationDoubleVehicleSubSystem(Name, No, railGuidVehileDeviceNames.ToList());
            return subSystemDevice;
        }
    }
}
