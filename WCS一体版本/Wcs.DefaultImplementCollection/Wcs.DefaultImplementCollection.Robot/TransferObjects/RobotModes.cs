using System.ComponentModel;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手工作模式
    /// </summary>
    public enum RobotModes
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        [Description("未知状态")]
        UnKnown=0,
        /// <summary>
        /// 手动
        /// </summary>
        [Description("手动")]
        Manual =1,
        /// <summary>
        /// 待使能
        /// </summary>
        [Description("待使能")]
        WaitingEnable =2,
        /// <summary>
        /// 待命
        /// </summary>
        [Description("待命")]
        Waiting =3,
        /// <summary>
        /// 运行中
        /// </summary>
        [Description("运行中")]
        Running =4,
        /// <summary>
        /// 报警停机
        /// </summary>
        [Description("报警停机")]
        AlarmDown =5,
        /// <summary>
        ///暂停
        /// </summary>
        [Description("暂停")]
        Pause = 6
    }
}