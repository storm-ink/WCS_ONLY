using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework.Devices;
using NHibernate.Linq;
using NLog;
namespace Wcs.Framework
{
    /// <summary>
    /// 物理动作序列（任务序列）<br />
    /// 所有的设备任务由该对象来派发，即所有设备由其来驱动
    /// </summary>
    public class EquipmentActionSequence : IComparer<EquipmentActionSequence>
    {
        #region Properities
        public virtual Int32 Id { get; protected set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual String DeviceName { get; protected set; }
        /// <summary>
        /// 获取序列中的当前所有动作
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<EquipmentAction> Actions { get; protected set; }
        /// <summary>
        /// 当前序列正在执行的物理动作信息(可能为null)<br />
        /// 通常在单任务设备中，如果一个任务在执行过程中发生了错误,也必须保证在下一个任务下发时该任务在第一个第发送
        /// </summary>
        public virtual EquipmentAction CurrentEquipmentAction { get; protected set; }
        /// <summary>
        /// 日志对象
        /// </summary>
        public virtual Logger Logger { get; protected set; }
        /// <summary>
        /// 调度程序
        /// </summary>
        public virtual EquipmentActionSequenceScheduler Scheduler { get; private set; }
        #endregion

        #region ctr
        /// <summary>
        /// 默认构造函数.
        /// </summary>
        protected EquipmentActionSequence() 
        {
            this.Actions = new Iesi.Collections.Generic.HashedSet<EquipmentAction>();
            this.Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="device">           此序列关联的设备. </param>
        public EquipmentActionSequence(Device device, EquipmentActionSequenceScheduler scheduler)
            : this()
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (!(device is TaskableDevice))
            {
                throw new ArgumentException(string.Format("{0} 继承 {1}，无法创建任务序列。", device, typeof(TaskableDevice)));
            }

            if (((TaskableDevice)device).ActionSequence!=null)
            {
                throw new InvalidOperationException(string.Format("{0} 已存在一个动作序列对象 {1}", device, ((TaskableDevice)device).ActionSequence));
            }

            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            this.Scheduler = scheduler;

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                EquipmentActionSequence sequence = unitOfWork.session.Get<EquipmentActionSequence>(device.Name);

                if (sequence == null)
                {
                    sequence = new EquipmentActionSequence();
                    sequence.DeviceName = device.Name;
                    unitOfWork.session.Save(sequence);
                }

                this.Id = sequence.Id;
                this.DeviceName = sequence.DeviceName;
                this.Actions = sequence.Actions;

                unitOfWork.Commit();
            }
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0} 的动作序列 #{1}", this.DeviceName, this.Id);
        }

        public int Compare(EquipmentActionSequence x, EquipmentActionSequence y)
        {
            throw new NotImplementedException();
        }

        public void Pop(EquipmentAction action)
        {
            lock (this.Actions)
            {
                EquipmentAction act;
                if (action.Id == 0)
                {
                    act = this.Actions.SingleOrDefault(x => x.EquipmentTaskId == action.EquipmentTaskId);
                }
                else
                {
                    act = this.Actions.SingleOrDefault(x => x.Id == action.Id);
                }

                if (act == null)
                {
                    this.Logger.Warn1("在队列中未找到 {0} 对象", this, action);
                }
                else
                {
                    this.Actions.Remove(act);
                }
            }
        }

        public void Push(EquipmentAction action)
        {
            lock (this.Actions)
            {
                EquipmentAction act;
                if (action.Id == 0)
                {
                    act = this.Actions.SingleOrDefault(x => x.EquipmentTaskId == action.EquipmentTaskId);
                }
                else
                {
                    act = this.Actions.SingleOrDefault(x => x.Id == action.Id);
                }

                if (act != null)
                {
                    throw new InvalidOperationException(string.Format("{0} 已存在于动作序列 {1} 当中。",action,this));
                }

                action.SequenceOrdering = this.Actions.Count + 1;
                this.Actions.Add(action);
            }
        }
    }
}
