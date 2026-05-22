using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public delegate void DeviceEventHandler<TDevice, TEventArgs>(TDevice device, TEventArgs args)
    where TDevice:Device
    where TEventArgs:HandleableEventArgs;
}
