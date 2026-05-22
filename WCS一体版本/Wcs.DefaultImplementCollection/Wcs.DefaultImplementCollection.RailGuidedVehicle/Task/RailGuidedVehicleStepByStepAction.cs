using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleStepByStepAction:EquipmentAction
    {
         protected RailGuidedVehicleStepByStepAction()
            : base()
        {
        }
         public RailGuidedVehicleStepByStepAction(RailGuidedVehicleDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, RailGuidedVehicleStation loadLocation, RailGuidedVehicleStation unloadLocation, Int16 containerCode)
            : base(device, group, equipmentTaskId, containerCode)
        {
            this.LoadLocation = LocationConverter.ToLocationInfo(loadLocation); 
            this.UnloadLocation = LocationConverter.ToLocationInfo(unloadLocation);            
         }
        /// <summary>
        /// 取货位置
        /// </summary>
        public virtual LocationInfo LoadLocation { get; set; }
        /// <summary>
        /// 卸货位置
        /// </summary>
        public virtual LocationInfo UnloadLocation { get; set; }

        /// <summary>
        /// 状态上下文
        /// </summary>
        public virtual PickingAndUnloadingWithStepByStepStateManager StateManager { get; set; }

        public override DeviceCommand ToAddCommand()
        {
            this.StateManager = AbstractStateManager.CreateOrGetContext<PickingAndUnloadingWithStepByStepStateManager>(this, DeviceConverter.ToDevice<TaskableDevice>(this.DeviceName));

            return null;
        }

        public override DeviceCommand ToCancelCommand()
        {
            var context = AbstractStateManager.GetContext(this);
            if (context == null)
            {
                return new CancelRailGuidedVehicleStepByStepActionCommand();
            }

            RailGuidedVehicleDevice railGuidedVehicleDevice = DeviceConverter.ToDevice<RailGuidedVehicleDevice>(this.DeviceName);

            if (railGuidedVehicleDevice.LastStatus == null)
            {
                throw new InvalidOperationException(string.Format("{0} 未连接", railGuidedVehicleDevice));
            }

            if (railGuidedVehicleDevice.LastStatus.TaskId==this.EquipmentTaskId.ToString("00000000"))
            {
                throw new InvalidOperationException(string.Format("{0} 不支持取消设备任务", railGuidedVehicleDevice));
            }

            return new CancelRailGuidedVehicleStepByStepActionCommand();
        }

        public override string ToReadableDescription()
        {
            return String.Format("将 {0} 从 {1} 运送到 {2}", this.ContainerCode, this.LoadLocation, this.UnloadLocation);
        }
    }
}
