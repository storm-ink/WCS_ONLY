using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.EquipmentActions
{
    /// <summary>
    /// 表示一个堆垛机卸货的物理动作
    /// </summary>
    public class CraneUnloadAction : EquipmentAction
    {
        protected CraneUnloadAction()
            : base()
        {
        }
        public CraneUnloadAction(Devices.Device device, Int32 equipmentTaskId, Int16 containerCode, CraneControl.EForkLR unloadDirection)
            : base(device, equipmentTaskId,containerCode)
        {
            SetUnloadDirection(unloadDirection);
        }

        /// <summary>
        /// 获取卸货方向
        /// </summary>
        public virtual CraneControl.EForkLR GetUnloadDirection()
        {
            String value = this.GetAttribute("UnloadDirection");

            return (CraneControl.EForkLR)Enum.Parse(typeof(CraneControl.EForkLR), value);
        }
        /// <summary>
        /// 设置卸货方向
        /// </summary>
        public virtual void SetUnloadDirection(CraneControl.EForkLR unloadDirection)
        {
            this.SetAttribute("UnloadDirection", unloadDirection.ToString());
        }

        public override string ToReadableDescription()
        {
            return String.Format("{0} {1} 将 {2} 在 {3} 卸下", this.DeviceInfo.DeviceType.GetDescription(), this.DeviceInfo.DeviceName, this.ContainerCode, this.GetUnloadDirection());
        }

        public override object ToEquipmentTask()
        {
            throw new NotImplementedException();
        }
    }
}
