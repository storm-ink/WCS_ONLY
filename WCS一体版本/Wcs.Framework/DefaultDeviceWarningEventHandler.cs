using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public class DefaultDeviceWarningEventHandler:IDeviceErrorEventHandler
    {
        protected Logger _logger;
        public DefaultDeviceWarningEventHandler()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }
        public virtual void Handle(Device device, DeviceWarningEventArgs args)
        {
            if (args.Warning.Code == DefaultDeviceWarningFactory.UNABLE_TO_CONNECT_ERROR_CODE)
            {
                UnableToConnectEquipmentFailure unableToConnectEquipmentFailure = new UnableToConnectEquipmentFailure(device);
                device.AddFailure(unableToConnectEquipmentFailure);
            }
            _logger.Warn1(string.Format("{0} 报警,等级 {1},错误码 {2}，描述 {3}", device,args.Warning.Level.GetDescription(), args.Warning.Code, args.Warning.Description), device, args);

            args.Handled = true;
        }
    }
}
