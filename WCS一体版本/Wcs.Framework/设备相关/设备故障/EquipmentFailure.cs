using System;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备故障
    /// </summary>
    public abstract class EquipmentFailure : ISupportTransactionObject
    {
        public EquipmentFailure(Device device)
        {
            this.CreatedAt = DateTime.Now;
            this.Device = device;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual Device Device { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public virtual DateTime? FinishedAt { get; set; }

        /// <summary>
        /// 指示当前故障是否已处于过期状态
        /// </summary>
        public abstract Boolean IsOverdued { get; }

        /// <summary>
        /// 故障名称
        /// </summary>
        public abstract String Name { get; protected set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}{1}", this.Device.Name, this.Name);
        }

        /// <summary>
        /// 获取该故障引起的所有不能使用的位置
        /// </summary>
        /// <returns></returns>
        public abstract Location[] GetUnserviceableLocations();

        #region ISupportTransactionObject

        public virtual SupportTransactionObjectStatus SupportTransactionObjectStatus { get; set; }

        #endregion ISupportTransactionObject
    }
}