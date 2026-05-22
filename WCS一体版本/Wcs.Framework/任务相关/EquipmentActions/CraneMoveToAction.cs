using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using Wcs.Framework.CraneControl;

namespace Wcs.Framework.EquipmentActions
{
    /// <summary>
    /// 表示一个堆垛机到 X 位置的动作
    /// </summary>
    public class CraneMoveToAction : EquipmentAction
    {
        protected CraneMoveToAction()
            : base()
        {
        }
        public CraneMoveToAction(Devices.Device device, Int32 equipmentTaskId, Int16 containerCode, RackLocation destination)
            : base(device, equipmentTaskId,containerCode)
        {
            SetDestination(destination);
        }

        /// <summary>
        /// 获取目的地
        /// </summary>
        public virtual RackLocation GetDestination()
        {
            string value = this.GetAttribute("Destination");
            var location = Location.TryParse(value);

            return location as RackLocation;
        }
        /// 设置目的地
        /// </summary>
        public virtual void SetDestination(RackLocation destination)
        {
            this.SetAttribute("Destination", destination.GetConvertibleCode());
        }

        public override string ToReadableDescription()
        {
            return String.Format("{0} {1} 移动到 {2}",this.DeviceInfo.DeviceType.GetDescription(),this.DeviceInfo.DeviceName, this.GetDestination().UserCode);
        }

        public override object ToEquipmentTask()
        {
            var location = this.GetDestination() as RackLocation;
            var p = new Wcs.Framework.CraneControl.Position
            {
                UCol = location.Column.ToString("000"),
                URow = location.Level.ToString("000"),
                MCol = 0,
                MRow = 0
            };
            Config.Get(this.DeviceInfo.DeviceName).Shelf.ParseUTMColRow(ref p);
            var hb = new HB(p);

            return hb;
        }
    }
}
