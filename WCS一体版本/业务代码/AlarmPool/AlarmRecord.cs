using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 报警记录
    /// </summary>
    public class AlarmRecord
    {
        /// <summary>
        /// ID key
        /// 格式 202212312359599999
        /// </summary>
        [DataMember]
        public virtual long Id { get; set; }
        /// <summary>
        /// ErrorCode
        /// </summary>
        [DataMember]
        public virtual String WcsAlarmCode { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [DataMember]
        public virtual AlarmCategorys AlarmCategory { get; set; }
        /// <summary>
        /// 故障等级
        /// </summary>
        public virtual AlarmLevels AlarmLevel { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [DataMember]
        public virtual String AlarmName { get; set; }
        /// <summary>
        /// ErrorCode
        /// </summary>
        [DataMember]
        public virtual String DeviceErrorCode { get; set; }

        /// <summary>
        /// 发生设备名称
        /// </summary>
        [DataMember]
        public virtual String Device { get; set; }
        /// <summary>
        /// 所属设备名
        /// </summary>
        public virtual string OwnerDevice { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [DataMember]
        public virtual String DeviceType { get; set; }
        /// <summary>
        /// 发生时间：报警发生的时间
        /// </summary>
        [DataMember]
        public virtual DateTime BeginingAt { get; set; }

        /// <summary>
        /// 消失时间：报警在设备上消失的时间
        /// </summary>
        [DataMember]
        public virtual DateTime? EndingAt { get; set; }

        /// <summary>
        /// 持续时长（毫秒）
        /// </summary>
        [DataMember]
        public virtual Int64 TotalMilliseconds { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public virtual string Remarks { get; set; }

        /// <summary>
        /// 获取异常主体
        /// </summary>
        /// <returns></returns>
        public virtual string GetAlarmHost()
        {
            if (this.Device == this.OwnerDevice)
                return $"{this.Device}";
            else
                return $"{this.Device}@{this.OwnerDevice}";
        }
    }
}
