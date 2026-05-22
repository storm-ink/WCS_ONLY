using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public abstract class EquipmentAction : Comparable<EquipmentAction>,IComparer<EquipmentAction>
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
        /// 动作当前状态
        /// </summary>
        public virtual EquipmentActionStatus Status { get; set; }
        /// <summary>
        /// 对象属性集合
        /// </summary>
        public virtual IDictionary<String, String> Attributes { get; set; }
        /// <summary>
        /// 所属动作组
        /// </summary>
        public virtual EquipmentActionGroup Group { get; protected set; }
        /// <summary>
        /// 逻辑移动动作
        /// </summary>
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
        public virtual ISet<EquipmentActionWarning> Warnings { get; protected set; }

        protected EquipmentAction()
        {
            this.Attributes = new Dictionary<String, String>();
        }
        public EquipmentAction(TaskableDevice device, EquipmentActionGroup group, Int32 equipmentTaskId, Int16 containerCode)
        {
            this.CreatedAt = DateTime.Now;
            this.DeviceName = device.Name;
            this.EquipmentTaskId = equipmentTaskId;
            this.ContainerCode = containerCode;
            this.Attributes = new Dictionary<String, String>();
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
        /// 转换为设备可识别的命令，以便用做发送操作
        /// </summary>
        /// <returns></returns>
        public abstract DeviceCommand ToCommand();

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
        /// 获取指定名称的属性值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual String GetAttribute(String name)
        {
            if (Attributes == null)
            {
                return null;
            }

            if (Attributes.ContainsKey(name))
            {
                return Attributes[name];
            }

            return null;
        }

        /// <summary>
        /// 设备属性值，如果属性不存在将被创建
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void SetAttribute(String name, String value)
        {
            lock (this)
            {
                if (Attributes == null)
                {
                    Attributes = new Dictionary<string, string>();
                }

                if (Attributes.ContainsKey(name))
                {
                    Attributes[name] = value;
                }
                else
                {
                    Attributes.Add(name, value);
                }
            }
        }

        /// <summary>
        /// 判断当前任务在逻辑动作范围内是否可以执行了
        /// </summary>
        /// <returns></returns>
        public virtual Boolean CanPerform()
        {
            //如果状态不为 new，不允许执行
            if (this.Status != EquipmentActionStatus.New)
            {
                return false;
            }

            //设备不空闲也不能发
            if (!IocContainer.Resolve<IWcsTypeConverter>().ToDevice<Device>(this.DeviceName).IsIdle)
            {
                return false;
            }

            //如果在第一位可以执行
            int index = this.Movement.EquipmentActions.OrderBy(x => x.Ordering).ToList().IndexOf(this);
            if (index == 0)
            {
                return true;
            }

            //如果前一个动作已经完成，可以执行
            if (this.Movement.EquipmentActions.OrderBy(x => x.Ordering).ElementAt(index - 1).Status == EquipmentActionStatus.Completed)
            {
                return true;
            }

            return false;
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
                || x.Attributes != y.Attributes
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
    }
}
