using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 表示一个堆垛机的一个单步执行全自动物理动作
    /// </summary>
    public class CraneAutomaticTransferWithStepByStepAction : EquipmentAction
    {
        protected CraneAutomaticTransferWithStepByStepAction()
            : base()
        {
        }
        public CraneAutomaticTransferWithStepByStepAction(CraneDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, RackLocation loadLocation, RackLocation unloadLocation, Int16 containerCode)
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

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物从 {1} 运送到 {2}", this.DeviceName, this.LoadLocation, this.UnloadLocation);
        }

        public override DeviceCommand ToAddCommand()
        {
            this.StateManager = AbstractStateManager.CreateOrGetContext<PickingAndUnloadingWithStepByStepStateManager>(this, DeviceConverter.ToDevice<TaskableDevice>(this.DeviceName));
            
            return null;
        }

        public override DeviceCommand ToCancelCommand()
        {
            return null;
        }
    }
}
