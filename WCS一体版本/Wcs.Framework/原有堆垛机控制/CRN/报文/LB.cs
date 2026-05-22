
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Wcs.Framework.CraneControl
{
    /// <summary>回复 接收任务确认状态 GOT &lt;- CRN  &lt;LB 接收任务状态(1) + 任务号(8)&gt;</summary>
    [Serializable]
    [DataContract]
    public sealed class LB : ResponseTelex
    {
        /// <summary>回复 接收任务确认状态</summary>
        public LB(ETaskState lbState, string sTaskId)
        {
            this.LBState = lbState;
            this.TaskId = sTaskId;

            this.Head = "LB";
            this.Body = new StringBuilder().
                AppendFormat("{0:0}", (int)lbState).
                Append(sTaskId).ToString();
        }

        /// <summary>接收任务状态</summary>
        [DataMember]
        public ETaskState LBState { get; private set; }

        /// <summary>任务号</summary>
        [DataMember]
        public string TaskId { get; private set; }
    }

    /// <summary>接收任务状态</summary>
    [DataContract]
    public enum ETaskState
    {
        /// <summary>接收正确</summary>
        [Description("接收正确")]
        [EnumMember]
        E01 = 1,

        /// <summary>接收不正确</summary>
        [Description("接收不正确")]
        [EnumMember]
        E02 = 2,

        /// <summary>设备忙</summary>
        [Description("设备忙")]
        [EnumMember]
        E03 = 3,

        /// <summary>起点位置不正确</summary>
        [Description("起点位置不正确")]
        [EnumMember]
        E04 = 4,

        /// <summary>终点位置不正确</summary>
        [Description("终点位置不正确")]
        [EnumMember]
        E05 = 5
    }
}
