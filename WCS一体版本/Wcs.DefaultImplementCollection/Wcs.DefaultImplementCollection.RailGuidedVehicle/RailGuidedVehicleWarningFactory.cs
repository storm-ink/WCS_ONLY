using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public sealed class RailGuidedVehicleWarningFactory:Wcs.Framework.DefaultDeviceWarningFactory
    {
        public override Wcs.Framework.DeviceWarning Create(Wcs.Framework.Device device, string code, object source)
        {
            int codeIntValue;
            if (int.TryParse(code, out codeIntValue))
            {
                code = codeIntValue.ToString("00");
            }

            var alarm = Wcs.Framework.DeviceErrorHelper.GetDeviceErrorFromErrorName(typeof(RailGuidedVehicleDevice).GetDisplayName(), code);

            if (alarm == null)
            {
                return new DeviceWarning(device, source, DeviceWarningLevel.Warning, code, null,false);
            }
            else
            {
                return new DeviceWarning(device, source, DeviceWarningLevel.Warning, code, alarm.ErrorName, alarm.IsFault);
            }
        }
    }
}
