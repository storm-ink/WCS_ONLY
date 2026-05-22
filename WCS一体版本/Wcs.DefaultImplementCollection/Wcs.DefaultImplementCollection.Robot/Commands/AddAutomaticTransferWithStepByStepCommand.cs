using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 表示一个添加Robot的一个单步执行全自动物理动作命令指令
    /// </summary>
    [DeviceCommandAttribute(IsEquipmentCommand=false)]
    public class AddAutomaticTransferWithStepByStepCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public AddAutomaticTransferWithStepByStepCommand()
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
