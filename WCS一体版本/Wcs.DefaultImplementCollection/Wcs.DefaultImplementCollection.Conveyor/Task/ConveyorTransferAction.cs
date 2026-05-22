using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;
using NLog;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 表示一个输送线的输送的物理动作
    /// </summary>
    public class ConveyorTransferAction : EquipmentAction
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        protected ConveyorTransferAction()
            : base()
        {
        }
        public ConveyorTransferAction(ConveyorDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, Int32 routeId, ConveyorLocation startLocation, ConveyorLocation endLocation)
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
        /// 路径号
        /// </summary>
        public virtual Int32 RouteId { get; set; }
        /// <summary>
        /// 此动作分配的在设备数据块中存放的索引位置（从1开始）
        /// </summary>
        public virtual Int32? AtPlcDBIndex { get; protected set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0} 将货物从 {1} 运送到 {2}", this.DeviceName, this.StartLocation, this.EndLocation);
        }
        static Random _rnd = new Random();
        public override DeviceCommand ToAddCommand()
        {
            ConveyorDevice conveyorDevice = ConveyorHelper.GetConveyorDevice(this.DeviceName);
            conveyorDevice.SetTaskBlockIndex(this, x => x.AtPlcDBIndex);

            var currentLocation = LocationConverter.ToLocation(this.Movement.Task.CurrentLocation);
            if (!(currentLocation is ConveyorLocation))
            {
                currentLocation = currentLocation.Synonymous.FirstOrDefault(x => x is ConveyorLocation);
            }
            if (currentLocation == null)
            {
                currentLocation = LocationConverter.ToLocation(this.Movement.StartLocation);
            }

            var startLocation = LocationConverter.ToLocation(this.StartLocation);
            if (currentLocation.Equals(startLocation))
            {
                currentLocation = startLocation;
            }

            var route = RouteHelper.RouteHeads.First(x => x.Id == this.RouteId);
            var cmd = new TaskCommand(
                 TaskHandShakes.New,
                Convert.ToUInt32(this.EquipmentTaskId),
                "",
                new UInt16[10],
                Convert.ToUInt16(route.No),
                Convert.ToUInt16(currentLocation.DeviceCode),
                //Convert.ToUInt16(this.StartLocation.DeviceCode),
                Convert.ToUInt16(this.EndLocation.DeviceCode),
                Convert.ToUInt16(this.AtPlcDBIndex)
                );

            if (conveyorDevice.ConveyorEquipmentActionToAddCommandPlugin != null)
                conveyorDevice.ConveyorEquipmentActionToAddCommandPlugin.EquipmentActionToAddCommandPlugin(this, ref cmd);

            return cmd;
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
            var route = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == this.RouteId);
            if (route == null)
            {
                throw new Exception(String.Format("未找到 {0} 使用的路径 {1}(id)，这可能是由于在任务形成后修改配置文件引起的", this, this.RouteId));
            }

            TaskCommand cmd = new TaskCommand(
                TaskHandShakes.ApplyForDelete,
                Convert.ToUInt32(this.EquipmentTaskId),
                "",
                new UInt16[10],
                Convert.ToUInt16(route.No),
                Convert.ToUInt16(this.StartLocation.DeviceCode),
                Convert.ToUInt16(this.EndLocation.DeviceCode),
                Convert.ToUInt16(this.AtPlcDBIndex));

            return cmd;
        }
    }
}
