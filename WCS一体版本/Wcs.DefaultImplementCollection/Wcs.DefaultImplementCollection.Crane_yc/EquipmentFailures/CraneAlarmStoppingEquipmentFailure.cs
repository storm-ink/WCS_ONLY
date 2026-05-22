using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CraneAlarmStoppingEquipmentFailure : EquipmentFailure
    {
        public CraneAlarmStoppingEquipmentFailure(CraneDevice craneDevice)
            : base(craneDevice)
        {
        }

        public override bool IsOverdued
        {
            get
            {
                var craneDevice = (CraneDevice)this.Device;

                if(craneDevice.LastStatus==null){
                    return false;
                }

                if (craneDevice.LastStatus.State == CraneStatus.AlarmAndShutdown)
                {
                    return false;
                }

                if (craneDevice.LastStatus.State == CraneStatus.ResetAlarm)
                {
                    return false;
                }

                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "报警停机";
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
