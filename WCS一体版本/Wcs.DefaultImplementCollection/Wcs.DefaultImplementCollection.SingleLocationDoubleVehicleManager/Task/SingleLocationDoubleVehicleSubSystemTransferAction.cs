using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 表示一个堆垛机调度的物理动作
    /// </summary>
    public class SingleLocationDoubleVehicleSubSystemTransferAction : EquipmentAction
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        protected SingleLocationDoubleVehicleSubSystemTransferAction()
            : base()
        {
        }
        public SingleLocationDoubleVehicleSubSystemTransferAction(SingleLocationDoubleVehicleSubSystem device, EquipmentActionGroup group, Int32 equipmentTaskId, Int32 routeId, SingleLocationDoubleVehicleSubSystemLocation startLocation, SingleLocationDoubleVehicleSubSystemLocation endLocation)
            : base(device, group, equipmentTaskId, 0)
        {
            this.StartLocation = LocationConverter.ToLocationInfo(startLocation);
            this.EndLocation = LocationConverter.ToLocationInfo(endLocation);
            this.RouteId = routeId;
        }
        /// <summary>
        /// 起点位置
        /// </summary>
        public virtual LocationInfo StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo EndLocation { get; set; }
        /// <summary>
        /// 绑定的车辆名称
        /// </summary>
        public virtual string Vehicle { get; set; }
        /// <summary>
        /// 路径号
        /// </summary>
        public virtual Int32 RouteId { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物(条码：{1})从 {2} 运送到 {3}", string.IsNullOrWhiteSpace(this.Vehicle) ? this.DeviceName : this.Vehicle, String.Join(",", this.Movement.Task.ContainerCodes), this.StartLocation, this.EndLocation);
        }
        static Random _rnd = new Random();
        public override DeviceCommand ToAddCommand()
        {
            //this.StateManager = AbstractStateManager.CreateOrGetContext<CraneSubSystemStateManager>(this, DeviceConverter.ToDevice<TaskableDevice>(this.DeviceName));

            return null;
        }

        UInt16 GetContainerCode()
        {
            if (this.Movement == null)
            {
                return 0;
            }

            if (this.Movement.Task == null)
            {
                return 0;
            }

            if (this.Movement.Task.ContainerCodes == null)
            {
                return 0;
            }
            if (this.Movement.Task.ContainerCodes.Count == 0)
            {
                return 0;
            }

            var containerCode = this.Movement.Task.ContainerCodes.FirstOrDefault(x => !String.IsNullOrWhiteSpace(x));

            if (String.IsNullOrWhiteSpace(containerCode))
            {
                return 0;
            }

            UInt16 ic = 0;
            for (int i = 5; i >= 1; i--)
            {
                if (containerCode.Length < i)
                {
                    continue;
                }

                if (!UInt16.TryParse(containerCode.Substring(containerCode.Length - i), out ic))
                {
                    continue;
                }

                if (ic >= 32767)
                {
                    continue;
                }

                return ic;
            }

            return ic;
        }

        public override DeviceCommand ToCancelCommand()
        {
            return null;
        }
    }
}
