using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using NHibernate.Linq;
namespace Wcs.Framework.EquipmentActions
{
    /// <summary>
    /// 表示一个输送线的输送的物理动作
    /// </summary>
    public class ConveyorTransferAction:EquipmentAction
    {
        protected ConveyorTransferAction()
            : base()
        {
        }
        public ConveyorTransferAction(Devices.Device device, Int32 equipmentTaskId, Int16 containerCode, Int32 routeId, ConveyorLocation startLocation, ConveyorLocation endLocation)
            : base(device, equipmentTaskId, containerCode)
        {
//#warning 强制 equipmentTaskId 和任务号一样 containerCode，如果不需要可以删以下两名
//            this.EquipmentTaskId = equipmentTaskId;
//            this.ContainerCode = Convert.ToInt16(equipmentTaskId);


            this.SetStartLocation(startLocation);
            this.SetEndLocation(endLocation);
            this.SetRouteId(routeId);
        }
        /// <summary>
        /// 获取起点位置
        /// </summary>
        public virtual ConveyorLocation GetStartLocation()
        {
            string startLocationValue = this.GetAttribute("StartLocation");
            var location = Location.TryParse(startLocationValue);

            return location as ConveyorLocation;
        }

        /// <summary>
        /// 设置起点位置
        /// </summary>
        public virtual void SetStartLocation(ConveyorLocation startLocation)
        {
            this.SetAttribute("StartLocation", startLocation.GetConvertibleCode());
        }
        /// <summary>
        /// 获取结束位置
        /// </summary>
        public virtual ConveyorLocation GetEndLocation()
        {
            string endLocationValue = this.GetAttribute("EndLocation");
            var location = Location.TryParse(endLocationValue);

            return location as ConveyorLocation;
        }

        /// <summary>
        /// 设置结束位置
        /// </summary>
        public virtual void SetEndLocation(ConveyorLocation endLocation)
        {
            this.SetAttribute("EndLocation", endLocation.GetConvertibleCode());
        }

        /// <summary>
        /// 设置为此动作分配的PLC块地址
        /// </summary>
        /// <param name="index"></param>
        public virtual void SetAtPlcDBIndex(Int32? index)
        {
            this.SetAttribute("AtPlcDBIndex", Convert.ToString(index));
        }
        /// <summary>
        /// 获取为此动作分配的
        /// </summary>
        /// <returns></returns>
        public virtual Int32? GetAtPlcDBIndex()
        {
            string value = this.GetAttribute("AtPlcDBIndex");

            if (value == null) return null;

            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 设置为此动作对应的路径号
        /// </summary>
        /// <param name="index"></param>
        public virtual void SetRouteId(Int32 routeId)
        {
            this.SetAttribute("RouteId", routeId.ToString());
        }
        /// <summary>
        /// 获取此动作对应的路径号
        /// </summary>
        /// <returns></returns>
        public virtual Int32 GetRouteId()
        {
            string value = this.GetAttribute("RouteId");

            return Convert.ToInt32(value);
        }

        public override string ToReadableDescription()
        {
            return String.Format("{0} {1} 将 {2} 从 {3} 运送到 {4}", this.DeviceInfo.DeviceType.GetDescription(),this.DeviceInfo.DeviceName,this.ContainerCode, this.GetStartLocation().UserCode, this.GetEndLocation().UserCode);
        }


        /// <summary>
        /// 为指定的输送线子任务分配一个地址
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns>为任务分配的在PLC任务块中存放的索引位置</returns>
        /// <remarks>
        /// 如果该物理动作已经被分配过一个索引位置，
        /// 则对该索引位置进行可用性验证，
        /// 不可用则重新分配一个。否则直接返回已分配的索引位置。
        /// 如果从未分配过，则直接分配一个新的索引位置并返回。</remarks>
        protected virtual UInt16 AllotTaskBlockIndex(NHUnitOfWork unitOfWork)
        {
            Devices.NewConveyor conveyor = this.DeviceInfo.GetDevice() as Devices.NewConveyor;
            if (conveyor == null)
            {
                throw new InvalidOperationException(string.Format("{0} 不支持该操作.",this));
            }

            if (conveyor.Tasks == null)
            {
                throw new Exception(String.Format("为 {0} 分配任务地址时失败，原因是 {1} 未连接或状态同步失败", this, conveyor));
            }

            var oldIndex=this.GetAtPlcDBIndex().GetValueOrDefault(0);
            if (oldIndex > 0)
            {
                if (conveyor.Tasks[oldIndex - 1].HandShake == HandShake.Empty)
                {
                    return Convert.ToUInt16(oldIndex);
                }
            }

            var usedIndexs = unitOfWork
                .session
                .Query<ConveyorTransferAction>()
                .Where(x => x.Status == EquipmentActionStatus.New || x.Status == EquipmentActionStatus.Executing || x.Status == EquipmentActionStatus.Error)
                .ToList()
                .Where(x => x.GetAtPlcDBIndex().GetValueOrDefault(0) > 0)
                .Select(x => Convert.ToUInt16(x.GetAtPlcDBIndex()))
                .ToList();

            UInt16[] allIndexs = new UInt16[conveyor.Tasks.Length];
            for (UInt16 i = 0; i < conveyor.Tasks.Length; i++)
            {
                allIndexs[i] = Convert.ToUInt16(i + 1);
            }

            UInt16? result = allIndexs.Except(usedIndexs).FirstOrDefault();
            if (result == null)
            {
                throw new Exception(String.Format("{0} 任务块已被写满，当前无法为 {1} 分配任务地址", conveyor, this));
            }
            while (conveyor.Tasks[result.Value - 1].HandShake != HandShake.Empty)
            {
                usedIndexs.Add(result.Value);
                result = allIndexs.Except(usedIndexs).FirstOrDefault();
                if (result == null)
                {
                    throw new Exception(String.Format("{0} 任务块已被写满，当前无法为 {1} 分配任务地址", conveyor,this));
                }
            }

            this.SetAtPlcDBIndex(result.Value);

            return result.Value;
        }

        public override object ToEquipmentTask()
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                AllotTaskBlockIndex(unitOfWork);

                unitOfWork.Commit();
            }

            var result = new SendTaskBlock();
            result.AssignmentID = Convert.ToUInt32(this.EquipmentTaskId);
            result.DestinationNo = Convert.ToUInt16(this.GetEndLocation().DeviceCode);
            /*
             * 为防止任务错误，直接被注释后的代码更可靠。
            //如果任务的的终点位置和当前位置不一样，则使用当前位置为起点
            //如果和终点位置一样，则使用自任务的起点们当前位置
            if (this.Movement.Task.CurrentLocation.DeviceCode != this.GetEndLocation().DeviceCode)
            {
                result.StartMotorNo = Convert.ToUInt16(this.Movement.Task.CurrentLocation.DeviceCode);
            }
            else
            {
                result.StartMotorNo = Convert.ToUInt16(this.GetStartLocation().DeviceCode);
            }*/
            result.StartMotorNo = Convert.ToUInt16(this.Movement.Task.CurrentLocation.DeviceCode);

            result.HandShake = HandShake.New;
            result.RotingNo = Convert.ToUInt16(Cfg.Configuration.Routes.Single(x => x.Id == this.GetRouteId()).No);
            result.TU_ID = Convert.ToUInt16(this.ContainerCode);
            result.Index = Convert.ToUInt16(this.GetAtPlcDBIndex().Value);

            return result;
        }
    }
}
