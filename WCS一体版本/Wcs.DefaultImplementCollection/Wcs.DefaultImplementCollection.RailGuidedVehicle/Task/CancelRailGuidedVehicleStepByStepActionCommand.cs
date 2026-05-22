using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 表示一个取消穿梭车的一个单步执行全自动物理动作命令指令
    /// </summary>
    [DeviceCommandAttribute(IsEquipmentCommand=false)]
    public class CancelRailGuidedVehicleStepByStepActionCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public CancelRailGuidedVehicleStepByStepActionCommand()
        {
        }

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            return true;
        }
    }
}
