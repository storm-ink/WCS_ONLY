using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备报警记录
    /// </summary>
    [DataContract]
    public class WarningRecordView
    {
        /// <summary>
        /// 发生时间：报警发生的时间
        /// </summary>
        [DataMember]
        public virtual DateTime BeginingAt { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [DataMember]
        public virtual String Category { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        [DataMember]
        public virtual String Code { get; set; }

        /// <summary>
        /// 发生设备
        /// </summary>
        [DataMember]
        public virtual String Device { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [DataMember]
        public virtual String DeviceType { get; set; }

        /// <summary>
        /// 消失时间：报警在设备上消失的时间
        /// </summary>
        [DataMember]
        public virtual DateTime? EndingAt { get; set; }

        /// <summary>
        /// 是否属于故障
        /// </summary>
        [DataMember]
        public virtual Boolean IsFault { get; set; }

        /// <summary>
        /// 结束时间：报警恢复正常的时间，由人工填写，默认使用 EndingAt 值。
        /// </summary>
        [DataMember]
        public virtual DateTime? FinishedAt { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [DataMember]
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [DataMember]
        public virtual String Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [DataMember]
        public virtual String Description { get; set; }
        /// <summary>
        /// 原因
        /// </summary>
        [DataMember]
        public virtual String Reason { get; set; }

        /// <summary>
        /// 指示是否已彻底修复该故障
        /// </summary>
        [DataMember]
        public virtual Boolean Repaired { get; set; }

        /// <summary>
        /// 处理结果
        /// </summary>
        [DataMember]
        public virtual String Result { get; set; }


        /// <summary>
        /// 处理人
        /// </summary>
        [DataMember]
        public virtual String UserName { get; set; }

        /// <summary>
        /// 持续时长（毫秒）
        /// </summary>
        [DataMember]
        public virtual Int64 TotalMilliseconds { get; set; }
    }
}
