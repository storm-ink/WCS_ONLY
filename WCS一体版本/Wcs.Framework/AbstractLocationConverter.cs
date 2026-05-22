using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public abstract class AbstractLocationConverter
    {
        public AbstractLocationConverter(String deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("deviceName");
            }

            this.DeviceName = deviceName;
        }
        public virtual String DeviceName { get; private set; }
        public abstract Location DeviceCodeToLocation(String deviceCode);
        public abstract Location ConvertibleCodeToLcation(String convertibleCode);
        public abstract Location UserCodeToLcation(String userCode);

    }
}
