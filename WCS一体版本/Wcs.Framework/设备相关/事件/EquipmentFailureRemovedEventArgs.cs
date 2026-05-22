namespace Wcs.Framework
{
    public class EquipmentFailureRemovedEventArgs : EquipmentFailureEventArgs
    {
        public EquipmentFailureRemovedEventArgs(EquipmentFailure equipmentFailure)
            : base(equipmentFailure)
        {
        }
    }
}