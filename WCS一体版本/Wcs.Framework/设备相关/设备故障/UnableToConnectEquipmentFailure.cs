using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个无法连接的设备故障
    /// </summary>
    /// <remarks>此故障将导致目标设备所有位置不可用</remarks>
    public class UnableToConnectEquipmentFailure : EquipmentFailure
    {
        public UnableToConnectEquipmentFailure(Device device)
            : base(device)
        {
        }

        public override string Name
        {
            get
            {
                return "无法连接到设备";
            }
            protected set
            {

            }
        }
        /// <summary>
        /// 获取该故障引起的所有不能使用的位置
        /// </summary>
        /// <returns>设备的所有位置</returns>
        public override Location[] GetUnserviceableLocations()
        {
            var locations = Cfg.WcsConfiguration
                .Instance
                .LocationCollection
                .Locations
                .Where(x => x.Device == this.Device)
                .ToArray();

            return locations;
        }

        public override bool IsOverdued
        {
            get
            {
                return this.Device.IsConnected;
            }
        }
    }
}
