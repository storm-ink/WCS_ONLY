using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.EquipmentActions
{
    /// <summary>
    /// 表示一个堆垛机的全自动物理动作
    /// </summary>
    public class CraneAutomaticTransferAction : EquipmentAction
    {
        protected CraneAutomaticTransferAction()
            : base()
        {
        }
        public CraneAutomaticTransferAction(Devices.Device device, Int32 equipmentTaskId, Int16 containerCode,RackLocation loadLocation,RackLocation unloadLocation)
            : base(device, equipmentTaskId,containerCode)
        {
            SetLoadLocation(loadLocation);
            SetUnloadLocation(unloadLocation);
        }
        /// <summary>
        /// 获取取货位置
        /// </summary>
        public virtual RackLocation GetLoadLocation()
        {
            string value = this.GetAttribute("LoadLocation");
            var location = Location.TryParse(value);

            return location as RackLocation;
        }
        /// <summary>
        /// 设置取货位置
        /// </summary>
        public virtual void SetLoadLocation(RackLocation loadLocation)
        {
            this.SetAttribute("LoadLocation",loadLocation.GetConvertibleCode());
        }
        /// <summary>
        /// 获取卸货位置
        /// </summary>
        public virtual RackLocation GetUnloadLocation()
        {
            string value = this.GetAttribute("UnloadLocation");
            var location = Location.TryParse(value);

            return location as RackLocation;
        } 
        /// 设置取货位置
        /// </summary>
        public virtual void SetUnloadLocation(RackLocation unloadLocation)
        {
            this.SetAttribute("UnloadLocation", unloadLocation.GetConvertibleCode());
        }

        public override string ToReadableDescription()
        {
            return String.Format("{0} {1} 将 {2} 从 {3} 取下运送到 {4}",this.DeviceInfo.DeviceType.GetDescription(),this.DeviceInfo.DeviceName, this.ContainerCode, this.GetLoadLocation().UserCode, this.GetUnloadLocation().UserCode);
        }

        public override object ToEquipmentTask()
        {
            CraneControl.HB result;
            string startPosition, endPostion;
            if (this.GetLoadLocation().Device.DeviceType == Devices.DeviceType.Crane)
            {
                startPosition = this.GetLoadLocation().DeviceCode;
            }
            else
            {
                var location = this.GetLoadLocation().SameAs.Where(x => x.Device == this.DeviceInfo.GetDevice()).SingleOrDefault();
                if (location == null)
                {
                    throw new Exception(String.Format("{0} 未转换为 {1} 能识别的位置", this.GetLoadLocation(), this.DeviceInfo.GetDevice()));
                }
                startPosition = location.DeviceCode;
            }

            if (this.GetUnloadLocation().Device.DeviceType == Devices.DeviceType.Crane)
            {
                endPostion = this.GetUnloadLocation().DeviceCode;
            }
            else
            {
                var location = this.GetUnloadLocation().SameAs.Where(x => x.Device == this.DeviceInfo.GetDevice()).SingleOrDefault();
                if (location == null)
                {
                    throw new Exception(String.Format("{0} 未转换为 {1} 能识别的位置", this.GetUnloadLocation(), this.DeviceInfo.GetDevice()));
                }

                endPostion = location.DeviceCode;
            }
            startPosition = System.Text.RegularExpressions.Regex.Replace(startPosition, @"(\d{2})(\d{3})(\d{3})", "$1-$2-$3");
            endPostion = System.Text.RegularExpressions.Regex.Replace(endPostion, @"(\d{2})(\d{3})(\d{3})", "$1-$2-$3");

            result = new CraneControl.HB(new CraneControl.SHB(this.DeviceInfo.DeviceName, startPosition, endPostion, this.EquipmentTaskId.ToString("00000000")));
            
            return result;
        }
    }
}
