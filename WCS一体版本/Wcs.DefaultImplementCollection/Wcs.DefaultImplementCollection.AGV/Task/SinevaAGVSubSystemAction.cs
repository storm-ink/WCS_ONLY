using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 表示一个堆垛机的半自动移动动作
    /// </summary>
    public class SinevaAGVSubSystemAction : EquipmentAction
    {   
        protected SinevaAGVSubSystemAction()
            : base()
        {
        }
        public SinevaAGVSubSystemAction(SinevaAGVDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, AGVSubSystemLocation fromLocation, AGVSubSystemLocation toLocation, Int16 containerCode,String sendAGVTaskId)
            : base(device, group, equipmentTaskId, containerCode)
        {
            this.LoadLocation = LocationConverter.ToLocationInfo(fromLocation);
            this.UnloadLocation = LocationConverter.ToLocationInfo(toLocation); 
            this.SendAGVTaskId = sendAGVTaskId;
        }
        /// <summary>
        /// 起点位置
        /// </summary>
        public virtual LocationInfo LoadLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo UnloadLocation { get; set; }
        /// <summary>
        /// 发送给AGV的任务号
        /// </summary>
        public virtual String SendAGVTaskId { get; set; }

        public override string ToReadableDescription()
        {
            return String.Format("{0}({1}) 将货物从 {2} 移动到 {3}", this.DeviceName, this.SendAGVTaskId, this.LoadLocation, this.UnloadLocation);
        }

        public override DeviceCommand ToAddCommand()
        {
            return null;
        }

        public override DeviceCommand ToCancelCommand()
        {
            var agvDevice = DeviceConverter.ToDevice<SinevaAGVDevice>(this.DeviceName);
            var equipmentTaskIds = SinevaAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.错误, this.DeviceName);
            if (equipmentTaskIds == null)
            {
                return null;
            }
            else if (!equipmentTaskIds.Any(x=> x.Contains(this.SendAGVTaskId)))
            {
                return null;
            }

            return new CancleTaskCommand(this.SendAGVTaskId, "");
        }

        public override string ToString()
        {
            return String.Format("物理动作#{0}({1}<>)", this.Id, this.EquipmentTaskId, this.SendAGVTaskId);
        }
    }
}
