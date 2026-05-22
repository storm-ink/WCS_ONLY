using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleStationElement : LocationElement
    {
        public RailGuidedVehicleStationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration,true)
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
        /// 存货位置集合
        /// </summary>
        public String[] StockingLocations { get; private set; }

        /// <summary>
        /// 是否过滤参与调度转换条码值计算（默认false 参与计算 配置true不参与计算）
        /// </summary>
        public bool IsFilterConvert { get; private set; }


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

            if (!(device is RailGuidedVehicleDevice))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(RailGuidedVehicleDevice)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            RailGuidedVehicleStation loc ;
            if (!IsWildcard)
            {
                loc = new RailGuidedVehicleStation(this.PickingChainAction, this.PuttingChainAction, this.StationNo, this.Position,this.StockingLocations, this.DeviceCode, this.UserCode, IsFilterConvert, (RailGuidedVehicleDevice)device);
            }
            else
            {
                loc = new RailGuidedVehicleStationWildcard((RailGuidedVehicleDevice)device, this.AbleAsOnlyLocation);
            }
            ((RailGuidedVehicleDevice)device).AcceptStation(loc);

            return loc;
        }

        protected override void Deserialize()
        {
            if (!this.IsWildcard)
            {
                this.PickingChainAction = this.GetAttribute<ChainAction>("pickingChainAction");
                this.PuttingChainAction = this.GetAttribute<ChainAction>("puttingChainAction");
                this.StationNo = this.GetAttribute<UInt16>("stationNo");
                this.Position = this.GetAttribute<Int32>("position");
                this.IsFilterConvert = this.GetAttributeOrDefault<bool>("isFilterConvert");

                var sls = this.GetAttributeOrDefault<String>("stockingLocations","");
                if (String.IsNullOrWhiteSpace(sls))
                {
                    WcsConfiguration._logger.Warn1(String.Format("节点 {0} 中指定的 stockingLocations 属性值为空", this.Node.GetXPath()), this);
                    //throw new ConfigurationErrorsException(String.Format("节点 {0} 中指定的 stockingLocations 属性值为空", this.Node.GetXPath()), this.Node);
                }

                var items = sls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length == 0)
                {
                    WcsConfiguration._logger.Warn1(String.Format("节点 {0} 中指定的 stockingLocations 属性值 “{1}”不包含任何有效的位置对象", this.Node.GetXPath(), sls), this);
                    //throw new ConfigurationErrorsException(String.Format("节点 {0} 中指定的 stockingLocations 属性值 “{1}”不包含任何有效的位置对象", this.Node.GetXPath(), sls), this.Node);
                }

                this.StockingLocations = items;
            }
        }
    }
}
