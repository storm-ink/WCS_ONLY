using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.EquipmentActions
{
    /// <summary>
    /// 表示一个堆垛机取货的物理动作
    /// </summary>
    public class CraneLoadAction : EquipmentAction
    {
        protected CraneLoadAction()
            : base()
        {
        }
        public CraneLoadAction(Devices.Device device, Int32 equipmentTaskId, Int16 containerCode, CraneControl.EForkLR loadDirection)
            : base(device, equipmentTaskId,containerCode)
        {
            SetLoadDirection(loadDirection);
        }

        /// <summary>
        /// 获取取货方向
        /// </summary>
        public virtual CraneControl.EForkLR GetLoadDirection()
        {
            String value = this.GetAttribute("LoadDirection");
            return (CraneControl.EForkLR)Enum.Parse(typeof(CraneControl.EForkLR), value);
        }
        /// <summary>
        /// 设置取货方向
        /// </summary>
        public virtual void SetLoadDirection(CraneControl.EForkLR loadDirection)
        {
            this.SetAttribute("LoadDirection", loadDirection.ToString());
        }

        public override string ToReadableDescription()
        {
            return String.Format("{0} {1} 将 {2} 从 {3} 取出",this.DeviceInfo.DeviceType.GetDescription(),this.DeviceInfo.DeviceName, this.ContainerCode, this.GetLoadDirection());
        }

        public override object ToEquipmentTask()
        {
            throw new NotImplementedException();
        }
    }
}
