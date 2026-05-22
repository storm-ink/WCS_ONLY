using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class DefaultCraneDeviceWarningFactory:DefaultDeviceWarningFactory
    {
        public override DeviceWarning Create(Device device, string code, object source)
        {
            int codeIntValue;
            if (int.TryParse(code, out codeIntValue))
            {
                code = codeIntValue.ToString("0000");
            }

            var alarm = Wcs.Framework.DeviceErrorHelper.GetDeviceErrorFromErrorName(typeof(CraneDevice).GetDisplayName(), code);

            DeviceWarning warning;
            
            if (alarm == null)
            {
                warning = new DeviceWarning(device, source, DeviceWarningLevel.Warning, code, null,false);
            }
            else
            {
                warning = new DeviceWarning(device, source, DeviceWarningLevel.Warning, code, alarm.ErrorName, alarm.IsFault);
                warning.Category = alarm.ErrorName;
            }

            return warning;
        }   
    }
}
