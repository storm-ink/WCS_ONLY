namespace Wcs.Framework.Lock
{
    /// <summary>
    /// 物理动作信息
    /// </summary>
    public class ActionInfo
    {
        /// <summary>
        /// 主任务Id
        /// </summary>
        public virtual int TaskId { get; set; }
        /// <summary>
        /// 主任务号
        /// </summary>
        public virtual string TaskCode { get; set; }
        /// <summary>
        /// 主任务起点
        /// </summary>
        public virtual string TaskStartLocation { get; set; }
        /// <summary>
        /// 主任务终点
        /// </summary>
        public virtual string TaskEndLocation { get; set; }
        /// <summary>
        /// 逻辑动作Id
        /// </summary>
        public virtual int LogicMovementId { get; set; }
        /// <summary>
        /// 逻辑动作taskId
        /// </summary>
        public virtual int LogicMovementTaskCode { get; set; }
        /// <summary>
        /// 逻辑动作起点
        /// </summary>
        public virtual string LogicMovementStartLocation { get; set; }
        /// <summary>
        /// 逻辑动作终点
        /// </summary>
        public virtual string LogicMovementLocation { get; set; }
        /// <summary>
        /// 物理动作Id
        /// </summary>
        public virtual int ActionId { get; set; }
        /// <summary>
        /// 物理动作taskId
        /// </summary>
        public virtual int ActionTaskCode { get; set; }
        /// <summary>
        /// 物理动作起点
        /// </summary>
        public virtual string ActionStartLocation { get; set; }
        /// <summary>
        /// 物理动作终点
        /// </summary>
        public virtual string ActionEndLocation { get; set; }
        /// <summary>
        /// 物理动作所属设备名
        /// </summary>
        public virtual string ActionDeviceName { get; set; }
    }
}