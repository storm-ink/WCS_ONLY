using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个具体的设备动作
    /// </summary>
    /// <example>
    /// <h1>该示例演示了如何创建一个新的物理动作类型</h1>
    /// <ul>
    /// <li>
    /// 1、创建一个新的类，并继承 <see cref="T:Wcs.Framework.EquipmentAction"/>.
    /// <code>
    /// public class SampleEquipmentAction : EquipmentAction
    ///{
    ///    public override string ToReadableDescription()
    ///    {
    ///        throw new NotImplementedException();
    ///    }
    ///
    ///    public override object ToEquipmentTask()
    ///    {
    ///        throw new NotImplementedException();
    ///    }
    ///}
    /// </code>
    /// </li>
    /// <li>
    /// 2、在 extensions.hbm.xml 文件中添加 <see cref="T:Wcs.Framework.EquipmentAction"/> 的子类映射。 
    /// <code lang="xml">
    /// <subclass name="Samples.SampleEquipmentAction" discriminator-value="SampleEquipmentAction" extends="EquipmentAction" />
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
    public abstract class EquipmentAction : Comparable<EquipmentAction>,IComparer<EquipmentAction>,ISupportTransactionObject
    {
        public virtual Int32 Id { get; set; }

        public virtual Int32 Ordering { get; set; }
        /// <summary>
        /// 此动作在序列中的排序位置
        /// </summary>
        public virtual Int32? SequenceOrdering { get; set; }
        /// <summary>
        /// 设备任务号
        /// </summary>
        public virtual Int32 EquipmentTaskId { get; set; }
        /// <summary>
        /// 容器编码
        /// </summary>
        public virtual Int16 ContainerCode { get; set; }
        /// <summary>
        /// 物理动作设备信息
        /// </summary>
        public virtual String DeviceName { get; set; }
        /// <summary>
        /// 物理动作执行设备信息
        /// </summary>
        public virtual String ActuatingDeviceName { get; set; }
        /// <summary>
        /// 动作当前状态
        /// </summary>
        public virtual EquipmentActionStatus Status { get; set; }
        /// <summary>
        /// 所属动作组
        /// </summary>
        public virtual EquipmentActionGroup Group { get; protected set; }
        /// <summary>
        /// 逻辑移动动作
        /// </summary>
        [JsonIgnore]
        public virtual LogicMovement Movement { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        /// <value>
        /// 物理动作被创建的时间
        /// </value>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 发送时间.
        /// </summary>
        /// <value>
        /// 物理动作被成功发送给设备的时间.
        /// </value>
        public virtual DateTime? SentAt { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        /// <value>
        /// 接收到设备报的任务完成信号的时间.这个时间在极少数的情况下并不能体现真正的任务完成时间，比如在设备任务报完成期间程序被关闭。
        /// </value>
        public virtual DateTime? FinishedAt { get; set; }

        /// <summary>
        /// 物理动作的报警信息
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<EquipmentActionWarning> Warnings { get; protected set; }
        /// <summary>
        /// 锁明细
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<RouteLock> Locks { get; protected set; }

        protected EquipmentAction()
        {
            this.Warnings = new Iesi.Collections.Generic.HashedSet<EquipmentActionWarning>();
            this.SupportTransactionObjectStatus = Framework.SupportTransactionObjectStatus.Committed;
        }
        public EquipmentAction(TaskableDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, Int16 containerCode):this()
        {
            this.CreatedAt = DateTime.Now;
            this.DeviceName = device.Name;
            this.ActuatingDeviceName = device.Name;
            this.EquipmentTaskId = equipmentTaskId;
            this.ContainerCode = containerCode;
            this.Group = group;

#warning 此处需要调整
            var maxSequenceOrdering = device.EquipmentActionScheduler.Actions.Max(x => x.SequenceOrdering);
            if (maxSequenceOrdering == null)
            {
                this.SequenceOrdering = 1;
            }
            else
            {
                this.SequenceOrdering = maxSequenceOrdering + 1;
            }
        }

        /// <summary>
        /// 将对象转换为用户可读的描述信息
        /// </summary>
        /// <returns></returns>
        public abstract String ToReadableDescription();
        
        /// <summary>
        /// 转换为向设备发送任务的指令
        /// </summary>
        /// <returns></returns>
        public abstract DeviceCommand ToAddCommand();
        /// <summary>
        /// 转换为向设备取消任务的指令
        /// </summary>
        /// <returns></returns>
        public abstract DeviceCommand ToCancelCommand();

        /// <summary>
        /// 添加一个警告信息
        /// </summary>
        /// <param name="warning">警告信息</param>
        public virtual void AddWarning(EquipmentActionWarning warning)
        {
            warning.EquipmentAction = this;
            this.Warnings.Add(warning);
        }

        /// <summary>
        /// 判断当前任务在逻辑动作范围内是否可以执行了
        /// </summary>
        /// <returns></returns>
        public virtual Boolean CanPerform(out String reason)
        {
            //如果状态不为 new，不允许执行
            if (this.Status != EquipmentActionStatus.New)
            {
                reason = string.Format("状态为 {0},预期应为 {1}", this.Status.GetDescription(), EquipmentActionStatus.New.GetDescription());
                return false;
            }

            //设备不空闲也不能发
            var device = WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                .Single(x => String.Equals(x.Device.Name, this.DeviceName, StringComparison.CurrentCultureIgnoreCase))
                .Device;
            var isidle = device.IsIdle;
            if (!isidle.Result)
            {
                reason = string.Format("{0} 处于繁忙状态（tips:{1}）", device, isidle.Information);
                return false;
            }

            //如果在第一位可以执行
            int index = this.Movement.EquipmentActions.OrderBy(x => x.Ordering).ToList().IndexOf(this);
            if (index == 0)
            {
                reason = string.Format("{0} 处于 {1} 物理动作序列的第一位", this,this.Movement);
                return true;
            }

            //如果前一个动作已经完成，可以执行
            var prevAction = this.Movement.EquipmentActions.OrderBy(x => x.Ordering).ElementAt(index - 1);
            if (prevAction.Status == EquipmentActionStatus.Completed)
            {
                reason = string.Format("处于 {0} 前面的 {1} 已处于完成状态", this, prevAction);
                return true;
            }
            else
            {
                reason = string.Format("处于 {0} 前面的 {1} 状态为 {2}，预期值应为 {3}", this, prevAction, prevAction.Status.GetDescription(), EquipmentActionStatus.Completed.GetDescription());
                return false;
            }
        }

        public virtual int Compare(EquipmentAction x, EquipmentAction y)
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

            if (x.Id != y.Id
                || x.ContainerCode != y.ContainerCode
                || !x.DeviceName.Equals(y.DeviceName)
                || x.EquipmentTaskId != y.EquipmentTaskId
                || x.Group.Compare(x.Group, y.Group) != 0
                || x.Ordering != y.Ordering
                || x.SequenceOrdering != y.SequenceOrdering
                || x.Status != y.Status)
            {
                return 1;
            }

            return 0;
        }

        #region ISupportTransactionObject
        public virtual SupportTransactionObjectStatus SupportTransactionObjectStatus { get; set; }
        #endregion

        public override string ToString()
        {
            return String.Format("物理动作#{0}({1})", this.Id, this.EquipmentTaskId);
        }
    }
}
