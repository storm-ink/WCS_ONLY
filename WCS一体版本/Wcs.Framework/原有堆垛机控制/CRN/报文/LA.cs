
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;

namespace Wcs.Framework.CraneControl
{
    /// <summary>回复 堆垛机状态 GOT &lt;- CRN  &lt;LA 垛机状态(2) + 垛机列层(6) + 货叉左右位(1) + 货叉高低位(1) + 站点位置(1) + 错误码(4) + 事件(1) + 任务号(8)&gt;</summary>
    [Serializable]
    [DataContract]
    public sealed class LA : ResponseTelex,IComparer<LA>
    {
        /// <summary>回复 堆垛机状态</summary>
        public LA(ECraneState State, Position Position, ECraneLR LR, ECraneTL TL, bool AtPosition, string ErrorCode, ECraneEvent Event, string TaskId)
        {
            this.State = State;
            this.ForkLR = LR;
            this.ForkTL = TL;
            this.Position = Position;
            this.AtPosition = AtPosition;
            this.ErrorCode = ErrorCode;
            this.Event = Event;
            this.TaskId = TaskId;

            this.Head = "LA";
            this.Body = new StringBuilder()
                .AppendFormat("{0:00}", (int)State)
                .AppendFormat("{0:000}", Position.MCol)
                .AppendFormat("{0:000}", Position.MRow)
                .AppendFormat("{0}", (int)LR)
                .AppendFormat("{0}", (int)TL)
                .Append(this.AtPosition ? "0" : "1")
                .Append(ErrorCode)
                .AppendFormat("{0}", (int)@Event)
                .Append(TaskId)
                .ToString();
        }

        /// <summary>堆垛机名</summary>
        [DataMember]
        public string CName { get; set; }
        /// <summary>连接状态</summary>
        [DataMember]
        public bool Connected { get; set; }
        /// <summary>手动操作终端锁 GUID</summary>
        [DataMember]
        public string LockGuid { get; set; }
        /// <summary>手动操作终端锁的客户机</summary>
        [DataMember]
        public string LockUser { get; set; }
        /// <summary>最后一次下发的任务信息</summary>
        [DataMember]
        public SHB SHB { get; set; }
        /// <summary>错误信息</summary>
        [DataMember]
        public string ErrorInfo { get; set; }

        /// <summary>堆垛机状态</summary>
        [DataMember]
        public ECraneState State { get; private set; }

        /// <summary>堆垛机事件</summary>
        [DataMember]
        public ECraneEvent Event { get; private set; }

        /// <summary>货叉左右位</summary>
        [DataMember]
        public ECraneLR ForkLR { get; private set; }

        /// <summary>货叉高低位</summary>
        [DataMember]
        public ECraneTL ForkTL { get; private set; }

        /// <summary>堆垛机位置</summary>
        [DataMember]
        public Position Position { get; private set; }

        /// <summary>站点位置</summary>
        [DataMember]
        public bool AtPosition { get; private set; }

        /// <summary>错误码</summary>
        [DataMember]
        public string ErrorCode { get; private set; }

        /// <summary>错误信息</summary>
        [DataMember]
        public List<string> ErrorList { get; set; }

        /// <summary>任务号</summary>
        [DataMember]
        public string TaskId { get; private set; }

        /// <summary>
        /// 指示堆垛机上是否有货物
        /// </summary>
        public bool HasCargoes
        {
            get
            {
                return this.State == ECraneState.E03 || this.State == ECraneState.E05;
            }
        }
        /// <summary>
        /// 指示堆垛机是否有错误发生
        /// </summary>
        public bool HasError
        {
            get
            {
                return this.State == ECraneState.E08 || (this.ErrorCode != "0000" && !string.IsNullOrEmpty(this.ErrorCode)) || this.State == ECraneState.E11;
            }
        }

        /// <summary>
        /// 指示堆垛机是否繁忙
        /// <para>繁忙状态的相关成立条件（任意一个）：</para>
        /// <para>1、堆垛机未连接</para>
        /// <para>2、已切换至手动模式</para>
        /// <para>3、被其它用户锁定</para>
        /// <para>4、错误码不为 String.Empty 并且不为 0000</para>
        /// <para>5、事件不等于 完成、出错完成、已初始化</para>
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return !(
                    (this.Event == ECraneEvent.E06 || this.Event == ECraneEvent.E00 || this.Event == ECraneEvent.E08) 
                    && (string.IsNullOrEmpty(this.ErrorCode) || this.ErrorCode == "0000")
                    && this.Connected
                    && !this.IsManual
                    && !this.IsBrake
                    && string.IsNullOrEmpty(LockGuid)
                    );
            }
        }

        /// <summary>
        /// 指示堆垛机是否处于手动模式
        /// </summary>
        public bool IsManual
        {
            get
            {
                //未获取到远程信息，默认为手动
                return this.State == ECraneState.E12 || CraneWorkModeKeyInfo == null || CraneWorkModeKeyInfo.IsManual;
            }
        }

        /// <summary>
        /// 远程急停按下
        /// </summary>
        public bool IsBrake
        {
            get
            {
                //必须有远程信息该标志位才有效
                return CraneWorkModeKeyInfo != null && CraneWorkModeKeyInfo.IsBrake;
            }
        }
        /// <summary>
        ///堆垛机相关工作模式切换信号
        /// </summary>
        [DataMember]
        public Devices.CraneWorkModeKeyInfo CraneWorkModeKeyInfo { get; set; }

        public int Compare(LA x, LA y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (!String.Equals(x.Text,y.Text,StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return this.Text;
        }
    }

    /// <summary>货叉高低位</summary>
    [DataContract]
    public enum ECraneTL
    {
        /// <summary>中位</summary>
        [Description("中位")]
        [EnumMember]
        E0 = 0,

        /// <summary>高位</summary>
        [Description("高位")]
        [EnumMember]
        E1,

        /// <summary>低位</summary>
        [Description("低位")]
        [EnumMember]
        E2
    }

    /// <summary>货叉左右位</summary>
    [DataContract]
    public enum ECraneLR
    {
        /// <summary>中位</summary>
        [Description("中位")]
        [EnumMember]
        E1 = 1,

        /// <summary>左位</summary>
        [Description("左")]
        [EnumMember]
        E2,

        /// <summary>左极限</summary>
        [Description("左极限")]
        [EnumMember]
        E3,

        /// <summary>右位</summary>
        [Description("右")]
        [EnumMember]
        E4,

        /// <summary>右极限</summary>
        [Description("右极限")]
        [EnumMember]
        E5
    }

    /// <summary>垛机状态 (报警复位后 04 -> 02; 05 -> 03)</summary>
    [DataContract]
    public enum ECraneState
    {
        /// <summary>初始化</summary>
        [Description("初始化")]
        [EnumMember]
        E00,

        /// <summary>回原点</summary>
        [Description("回原点")]
        [EnumMember]
        E01,

        /// <summary>无货待命</summary>
        [Description("无货待命")]
        [EnumMember]
        E02,

        /// <summary>有货待命</summary>
        [Description("有货待命")]
        [EnumMember]
        E03,

        /// <summary>无货运行</summary>
        [Description("无货运行")]
        [EnumMember]
        E04,

        /// <summary>有货运行</summary>
        [Description("有货运行")]
        [EnumMember]
        E05,

        /// <summary>取货</summary>
        [Description("取货")]
        [EnumMember]
        E06,

        /// <summary>放货</summary>
        [Description("放货")]
        [EnumMember]
        E07,

        /// <summary>报警停机</summary>
        [Description("报警停机")]
        [EnumMember]
        E08,

        /// <summary>报警复位</summary>
        [Description("报警复位")]
        [EnumMember]
        E09,

        /// <summary>回原点</summary>
        [Description("回原点")]
        [EnumMember]
        E10,

        /// <summary>未连接</summary>
        [Description("未连接")]
        [EnumMember]
        E11,

        /// <summary>手动操作</summary>
        [Description("手动操作")]
        [EnumMember]
        E12
    }

    /// <summary>堆垛机事件</summary>
    [DataContract]
    public enum ECraneEvent
    {
        /// <summary>已初始化</summary>
        [Description("初始化")]
        [EnumMember]
        E00 = 0,

        /// <summary>正在运行</summary>
        [Description("开始运行")]
        [EnumMember]
        E01 = 1,

        /// <summary>正在取货</summary>
        [Description("开始取货")]
        [EnumMember]
        E02 = 2,

        /// <summary>取货完成</summary>
        [Description("取货完成")]
        [EnumMember]
        E03 = 3,

        /// <summary>正在放货</summary>
        [Description("开始放货")]
        [EnumMember]
        E04 = 4,

        /// <summary>放货完成</summary>
        [Description("放货完成")]
        [EnumMember]
        E05 = 5,

        /// <summary>完成</summary>
        [Description("完成")]
        [EnumMember]
        E06 = 6,

        /// <summary>急停</summary>
        [Description("急停")]
        [EnumMember]
        E07 = 7,

        /// <summary>出错完成</summary>
        [Description("出错完成")]
        [EnumMember]
        E08 = 8,

        /// <summary>回原点</summary>
        [Description("回原点")]
        [EnumMember]
        E09 = 9
    }

    /// <summary>堆垛机货叉方向</summary>
    [DataContract]
    public enum EForkLR
    {
        /// <summary>左排</summary>
        [Description("左排")]
        [EnumMember]
        L = 1,

        /// <summary>右排</summary>
        [Description("右排")]
        [EnumMember]
        R = 2
    }
}
