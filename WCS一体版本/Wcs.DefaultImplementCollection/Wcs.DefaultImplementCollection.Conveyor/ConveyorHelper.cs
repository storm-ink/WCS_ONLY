using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ConveyorHelper
    {
        public static ConveyorDevice GetConveyorDevice(string deviceName)
        {
            var deviceElement = WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                .SingleOrDefault(x => string.Equals(x.Device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase));
            if (deviceElement == null)
            {
                throw new Exception(string.Format("未找到名称为 {0} 的设备", deviceName));
            }

            if (!(deviceElement.Device is ConveyorDevice))
            {
                throw new Exception(string.Format("{0} 不是 {1} 类型", deviceName,typeof(ConveyorDevice)));
            }

            return (ConveyorDevice)deviceElement.Device;
        }
        public static ConveyorLocation GetConveyorLocation(LocationInfo locationInfo)
        {
            var locationElement = WcsConfiguration
               .Instance
               .LocationCollection
               .ParticularLocationCollection.SelectMany(x => x.LocationElements)
               .SingleOrDefault(x => 
                   string.Equals(x.Location.UserCode, locationInfo.UserCode, StringComparison.CurrentCultureIgnoreCase)
                   && string.Equals(x.Location.DeviceCode, locationInfo.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
               );
            if (locationElement == null)
            {
                throw new Exception(string.Format("未找到 {0} 描述的位置对象", locationInfo));
            }

            if (!(locationElement.Location is ConveyorLocation))
            {
                throw new Exception(string.Format("{0} 不是 {1} 类型", locationElement.Location, typeof(ConveyorLocation)));
            }

            return (ConveyorLocation)locationElement.Location;
        }
    }
}
