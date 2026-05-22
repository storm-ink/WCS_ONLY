using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public class SingleLocationDoubleVehicleSubSystemLocationElement : LocationElement
    {
        public SingleLocationDoubleVehicleSubSystemLocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration, true)
        {

        }
        /// <summary>
        /// 取货链条动作
        /// </summary>
        public ChainAction PickingChainAction { get; private set; }
        /// <summary>
        /// 放货链条动作
        /// </summary>
        public ChainAction PuttingChainAction { get; private set; }
        /// <summary>
        /// 站点位置
        /// </summary>
        public UInt16 StationNo { get; private set; }
        /// <summary>
        /// 坐标
        /// </summary>
        public Int32 Position { get; private set; }
        /// <summary>
        /// 是否是设备位置
        /// </summary>
        public bool IsDevice { get; private set; }

        protected override Wcs.Framework.Location CreateLocation()
        {
            Device device;
            if (String.Equals(this.DeviceName, this.ParticularLocationCollection.DeviceName, StringComparison.CurrentCultureIgnoreCase))
            {
                device = this.ParticularLocationCollection.GetDeviceElement().Device;
            }
            else
            {
                device = this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Where(x => string.Equals(x.Device.Name, this.DeviceName, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Device)
                .SingleOrDefault();

                if (device == null)
                {
                    string msg = string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 中标识的设备",
                        this.Node.GetXPath(),
                        "device",
                        this.DeviceName);
                    throw new ConfigurationErrorsException(msg);
                }
            }

            if (!(device is SingleLocationDoubleVehicleSubSystem))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(SingleLocationDoubleVehicleSubSystem)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            SingleLocationDoubleVehicleSubSystemLocation loc;
            if (!IsWildcard)
            {
                loc = new SingleLocationDoubleVehicleSubSystemLocation(this.PickingChainAction, this.PuttingChainAction, this.StationNo, this.Position, this.DeviceCode, this.UserCode, (SingleLocationDoubleVehicleSubSystem)device, IsDevice);
            }
            else
            {
                loc = new SingleLocationDoubleVehicleSubSystemLocationWildcard((SingleLocationDoubleVehicleSubSystem)device, this.AbleAsOnlyLocation);
            }
            ((SingleLocationDoubleVehicleSubSystem)device).AcceptStation(loc);

            return loc;
        }

        protected override void Deserialize()
        {
            if (!this.IsWildcard)
            {
                this.PickingChainAction = this.GetAttributeOrDefault<ChainAction>("pickingChainAction", ChainAction.None);
                this.PuttingChainAction = this.GetAttributeOrDefault<ChainAction>("puttingChainAction", ChainAction.None);
                this.StationNo = this.GetAttributeOrDefault<UInt16>("stationNo", 0);
                this.Position = this.GetAttributeOrDefault<Int32>("position", 0);
                this.IsDevice = this.GetAttributeOrDefault<bool>("isDevice", false);
            }
        }
    }
}
