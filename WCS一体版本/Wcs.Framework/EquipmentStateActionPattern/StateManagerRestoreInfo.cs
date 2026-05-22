using System;

namespace Wcs.Framework
{
    /// <summary>
    /// 状态上下文保存的恢复信息
    /// </summary>
    public class StateManagerRestoreInfo
    {
        public StateManagerRestoreInfo(AbstractStateManager stateManager)
            : this()
        {
            this.EquipmentActionId = stateManager.EquipmentAction.Id;
            this.CurrentStateName = stateManager.CurrentState.Name;
            this.OwnDeviceName = stateManager.Device.Name;
            this.ActuatingDeviceName = stateManager.Device.Name;
        }

        public StateManagerRestoreInfo(int equipmentActionId, string currentStateName, string ownDeviceName, string actuatingDeviceName)
            : this()
        {
            this.EquipmentActionId = equipmentActionId;
            this.CurrentStateName = currentStateName;
            this.OwnDeviceName = ownDeviceName;
            this.ActuatingDeviceName = actuatingDeviceName;
        }

        protected StateManagerRestoreInfo()
        {
            this.CreatedAt = DateTime.Now;
            this.LastUpdateAt = DateTime.Now;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; protected set; }
        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public virtual DateTime LastUpdateAt { get; set; }

        /// <summary>
        /// 当前状态名称
        /// </summary>
        public virtual String CurrentStateName { get; set; }

        /// <summary>
        /// 关联的物理动作Id（约束）
        /// </summary>
        public virtual Int32 EquipmentActionId { get; protected set; }
        /// <summary>
        /// 所属设备名称
        /// </summary>
        public virtual string OwnDeviceName { get; set; }
        /// <summary>
        /// 执行设备名称
        /// </summary>
        public virtual string ActuatingDeviceName { get; set; }

        public override string ToString()
        {
            return String.Format($"状态上下文恢复信息#ActionId:{this.EquipmentActionId}_CurrentState:{CurrentStateName}_OwnDevice:{OwnDeviceName}_ActuatingDevice:{ActuatingDeviceName}_CreatedAt:{CreatedAt}_LastUpdateAt:{LastUpdateAt}");
        }
    }
}