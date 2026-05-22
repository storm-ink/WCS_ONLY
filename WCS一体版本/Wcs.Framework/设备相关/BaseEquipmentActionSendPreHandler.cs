using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public abstract class BaseEquipmentActionSendPreHandler
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public abstract void Hand(EquipmentAction action);
    }
}
