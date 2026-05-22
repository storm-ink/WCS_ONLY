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
    public class WarningRecord
    {
        DateTime? m_EndingAt;
        DateTime? m_FinishedAt;
        String m_Code;
        public WarningRecord()
        {
            BeginingAt = DateTime.Now;
        }
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
        /// 任务号
        /// </summary>
        [DataMember]
        public virtual String TaskCode { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        [DataMember]
        public virtual String CurrentAt { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        [DataMember]
        public virtual String Code
        {
            get
            {
                return m_Code;
            }
            set
            {
                m_Code = value;
                if (String.IsNullOrWhiteSpace(Name))
                {
                    Name = value;
                }
            }
        }

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
        public virtual DateTime? EndingAt
        {
            get
            {
                return m_EndingAt;
            }
            set
            {
                m_EndingAt = value;
                if (FinishedAt == null)
                {
                    FinishedAt = value;
                }
            }
        }

        /// <summary>
        /// 是否属于故障
        /// </summary>
        [DataMember]
        public virtual Boolean IsFault { get; set; }

        /// <summary>
        /// 结束时间：报警恢复正常的时间，由人工填写，默认使用 EndingAt 值。
        /// </summary>
        [DataMember]
        public virtual DateTime? FinishedAt
        {
            get
            {
                return m_FinishedAt;
            }
            set
            {
                m_FinishedAt = value;
                if (m_FinishedAt != null && this.BeginingAt != null)
                {
                    TotalMilliseconds = Convert.ToInt64(m_FinishedAt.Value.Subtract(BeginingAt).TotalMilliseconds);
                }
            }
        }

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
        public virtual Int64 TotalMilliseconds
        {
            get
            {
                if (m_FinishedAt != null && this.BeginingAt != null)
                {
                    return Convert.ToInt64(m_FinishedAt.Value.Subtract(BeginingAt).TotalMilliseconds);
                }

                return 0;
            }
            set
            {
            }
        }
    }
}
