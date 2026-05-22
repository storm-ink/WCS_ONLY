using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个取消堆垛机的一个全自动物理动作命令指令
    /// </summary>
    [DeviceCommandAttribute(IsEquipmentCommand=false)]
    public class CancelCraneAutomaticTransferActionCommand : DeviceCommand, IDeviceCommandAdjudicator
    {
        public CancelCraneAutomaticTransferActionCommand()
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
