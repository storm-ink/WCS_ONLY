using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class SinevaAGVSubSystemLogicMovement:LogicMovement
    {

      

        protected SinevaAGVSubSystemLogicMovement()
            : base()
        {
        }


        public SinevaAGVSubSystemLogicMovement(SinevaAGVDevice device, Int32 routeId, Location startLocation, Location endLocation, Int16 containerCode)
            : base(device, routeId, startLocation, endLocation, containerCode)
        {
        }

        protected override void CreateEquipmentActions()
        {
            Location startLocation = Wcs.Framework.LocationConverter.ToLocation(this.StartLocation);
            Location endLocation = Wcs.Framework.LocationConverter.ToLocation(this.EndLocation);
            SinevaAGVDevice device = Wcs.Framework.DeviceConverter.ToDevice<SinevaAGVDevice>(this.DeviceName);
            EquipmentActionGroup group = new EquipmentActionGroup();
            int equipmentTaskId = SerialNumberFactory.GenerateEquipmentTaskId();
            //String sendAGVTaskId = equipmentTaskId+"-"+ SinevaAGVHelper.GetSendAGVTaskId(this.DeviceName);
            String sendAGVTaskId =DateTime.Now.ToString("yyyyMMddHHmmss-")+ equipmentTaskId;


            AGVSubSystemLocation startStation, endStation;
            if (startLocation is AGVSubSystemLocation)
            {
                startStation = (AGVSubSystemLocation)startLocation;
            }
            else
            {
                startStation = (AGVSubSystemLocation)startLocation.Synonymous.Single(x => x is AGVSubSystemLocation);
            }

            if (endLocation is AGVSubSystemLocation)
            {
                endStation = (AGVSubSystemLocation)endLocation;
            }
            else
            {
                endStation = (AGVSubSystemLocation)endLocation.Synonymous.Single(x => x is AGVSubSystemLocation);
            }

            SinevaAGVSubSystemAction action = new SinevaAGVSubSystemAction(
                device,
                group,
                equipmentTaskId,
                startStation,
                endStation,
                _containerCode,
                sendAGVTaskId);

            action.Ordering = 0;

            this.EquipmentActions.Add(action);
        }
    }
}
