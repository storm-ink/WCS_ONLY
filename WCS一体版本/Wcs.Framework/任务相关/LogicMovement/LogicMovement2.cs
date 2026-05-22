using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 统一的Logicmovement
    /// </summary>
    public class LogicMovement2 : LogicMovement
    {
        protected LogicMovement2()
            : base()
        {

        }
        public LogicMovement2(TaskableDevice device, Int32 routeId, Location start, Location end, Int16 containerCode)
            : base(device, routeId, start, start, containerCode)
        {

        }
        protected override void CreateEquipmentActions()
        {
            throw new NotImplementedException();
        }
    }
}
