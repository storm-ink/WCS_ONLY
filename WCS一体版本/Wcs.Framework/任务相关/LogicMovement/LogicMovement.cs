using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 逻辑动作
    /// 表示一个任务的一个移动步骤，相当于原来的 子任务 概念
    /// </summary>
    /// <example>
    /// <h1>该示例演示了如何创建一个新的逻辑动作类型</h1>
    /// <ul>
    /// <li>
    /// 1、创建一个新的类，并继承 <see cref="T:Wcs.Framework.LogicMovement"/>.
    /// <code>
    ///public class SampleLogicMovemnt : LogicMovement
    ///{
    ///    protected override void InitializeEquipmentActions()
    ///    {
    ///        
    ///    }
    ///}
    /// </code>
    /// </li>
    /// <li>
    /// 2、在 extensions.hbm.xml 文件中添加 <see cref="T:Wcs.Framework.LogicMovement"/> 的子类映射。 
    /// <code lang="xml">
    /// <subclass name="Samples.SampleLogicMovemnt" discriminator-value="SampleLogicMovemnt" extends="LogicMovement" />
    /// </code>
    /// </li>
    /// <li>
    /// 如果该类型位于框架应用程序集以为，那么还应该在 hibernate.cfg.xml 的 session-factory 节点添加一个新的 mapping 节点，将此应用程序集加入其中。
    /// <code lang="xml">
    /// <session-factory name="wcs"><map assembly="Samples" /></session-factory>
    /// </code>
    /// </li>
    /// </ul>
    /// </example>
    [JsonObject]
    public abstract class LogicMovement : Comparable<LogicMovement>, IComparer<LogicMovement>
    {
        Iesi.Collections.Generic.ISet<EquipmentAction> _equipmentActions;
        protected Int16 _containerCode;
        protected LogicMovement()
        {
            _equipmentActions = new Iesi.Collections.Generic.HashedSet<EquipmentAction>();
        }
        public LogicMovement(TaskableDevice device, Int32? routeId, Location startLocation, Location endLocation, Int16 containerCode)
            : this()
        {
            this._containerCode = containerCode;
            this.RouteId = routeId;
            this.DeviceName = device.Name;
            this.StartLocation = LocationConverter.ToLocationInfo(startLocation);
            this.EndLocation = LocationConverter.ToLocationInfo(endLocation);
            this.CreatedAt = DateTime.Now;

            CreateEquipmentActions();

            int ordering = 0;
            foreach (var action in EquipmentActions)
            {
                action.Ordering = ordering;
                action.Movement = this;
                ordering++;

                var systemEquipmentActionScheduler = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.SystemEquipmentActionScheduler;
                if (systemEquipmentActionScheduler != null)
                    systemEquipmentActionScheduler.Add(action);

                device.EquipmentActionScheduler.Add(action);
            }
        }

        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 逻辑动作在任务中的排序位置
        /// </summary>
        public virtual Int32 Ordering { get; set; }
        /// <summary>
        /// 所属设备名称
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 路径号(有可能为 null，不是所有逻辑动作都有路径)
        /// </summary>
        public virtual Int32? RouteId { get; set; }
        /// <summary>
        /// 逻辑动作所属任务
        /// </summary>
        [JsonIgnore]
        public virtual Task Task { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public virtual DateTime? FinishedAt { get; set; }
        /// <summary>
        /// 该逻辑动作依次需要执行的物理动作
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<EquipmentAction> EquipmentActions
        {
            get
            {
                return _equipmentActions;
            }
            protected set
            {
                _equipmentActions = value;
            }
        }
        /// <summary>
        /// 对物理动作属性进行赋值
        /// </summary>
        protected abstract void CreateEquipmentActions();
        /// <summary>
        /// 起点位置
        /// </summary>
        public virtual LocationInfo StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo EndLocation { get; set; }

        /// <summary>
        /// 获取逻辑动作的状态
        /// </summary>
        public virtual LogicMovementStatus Status
        {
            get;
            set;
        }

        public virtual int Compare(LogicMovement x, LogicMovement y)
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

            if (
                x.DeviceName.Equals(y.DeviceName, StringComparison.CurrentCultureIgnoreCase)
                || x.EndLocation.Compare(x.EndLocation, y.EndLocation) != 0
                || x.Id != y.Id
                || x.Ordering != y.Ordering
                || x.RouteId != x.RouteId
                || x.StartLocation.Compare(x.StartLocation, y.StartLocation) != 0
                || x.Status != y.Status
                )
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("逻辑动作#{0}", this.Id);
        }
    }
}
