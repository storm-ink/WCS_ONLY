using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示堆垛机正处于手动模式的设备故障。严格的请，它应该是一个状态。
    /// </summary>
    /// <remarks>此故障将导致目标设备所有位置不可用</remarks>
    public class CraneManulModeEquipmentFailure : EquipmentFailure
    {
        public CraneManulModeEquipmentFailure(CraneDevice craneDevice)
            : base(craneDevice)
        {

        }

        public override bool IsOverdued
        {
            get
            {
                var craneDevice = (CraneDevice)this.Device;

                return craneDevice.WorkMode != CraneDeviceWorkMode.Manual;
            }
        }

        public override string Name
        {
            get
            {
                return "手动模式";
            }
            protected set
            {
                
            }
        }

        public override Location[] GetUnserviceableLocations()
        {
            var craneDevice = (CraneDevice)this.Device;

            return craneDevice.Locations ;
        }
    }
}
