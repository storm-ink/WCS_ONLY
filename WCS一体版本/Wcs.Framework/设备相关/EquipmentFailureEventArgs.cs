namespace Wcs.Framework
{
    public abstract class EquipmentFailureEventArgs : HandleableEventArgs
    {
        public EquipmentFailureEventArgs(EquipmentFailure equipmentFailure)
        {
            this.EquipmentFailure = equipmentFailure;
        }

        /// <summary>
        /// 故障
        /// </summary>
        public EquipmentFailure EquipmentFailure { get; protected set; }
    }
}