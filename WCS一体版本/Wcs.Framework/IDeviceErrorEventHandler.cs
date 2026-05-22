using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public interface  IDeviceErrorEventHandler
    {
        void Handle(Device device, DeviceWarningEventArgs args);
    }
}
