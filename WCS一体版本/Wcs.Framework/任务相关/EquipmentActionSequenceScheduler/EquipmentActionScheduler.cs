using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NHibernate.Linq;

namespace Wcs.Framework
{
    public class EquipmentActionScheduler
    {
        List<EquipmentAction> _actions = new List<EquipmentAction>();
        /// <summary>
        /// 所属设备
        /// </summary>
        public TaskableDevice Device { get; private set; }
        /// <summary>
        /// 当前拥有的物理动作
        /// </summary>
        public EquipmentAction[] Actions
        {
            get
            {
                return _actions.ToArray();
            }
        }
        /// <summary>
        /// 日志
        /// </summary>
        public Logger Logger { get; protected set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="device">所属设备</param>
        /// <remarks>
        /// 将自动从数据库中读取状态为 <see cref="F:Wcs.Framework.EquipmentActionStatus.New"/> 的 <see cref="T:Wcs.Framework.EquipmentAction"/>
        /// </remarks>
        public EquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
        {
            this.Logger = LogManager.GetCurrentClassLogger();
            this.Device = device;

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _actions = unitOfWork.session.Query<EquipmentAction>()
                    .Where(x => x.DeviceName == device.Name && x.Status == EquipmentActionStatus.New)
                    .ToList();
            }
        }

        /// <summary>
        /// 将指定的物理动作添加到任务队列中
        /// </summary>
        /// <param name="action">物理动作</param>
        public void Add(EquipmentAction action)
        {
            lock (_actions)
            {
                if (_actions.Any(x => x.EquipmentTaskId == action.EquipmentTaskId))
                {
                    this.Logger.Warn1(string.Format("尝试向 {0} 添加 {1} 时发现其已存在。", this, action), this, action);
                    return;
                }

                _actions.Add(action);
                this.Logger.Info1(string.Format("{0} 被加入到 {1} 当中。", action, this), this, action);
            }
        }

        /// <summary>
        /// 将指定的物理动作从任务队列中移除
        /// </summary>
        /// <param name="action">物理动作</param>
        public void Remove(EquipmentAction action)
        {
            lock (_actions)
            {
                EquipmentAction act = this.Actions.SingleOrDefault(x => x.EquipmentTaskId == action.EquipmentTaskId);
                if (act == null)
                {
                    this.Logger.Warn1(string.Format("尝试移除 {0},但在 {1} 中未找到 {0}", action, this),this,action);
                }

                _actions.Remove(act);
                this.Logger.Info1(string.Format("{0} 被从 {1} 当中移除。", action, this), this, action);
            }
        }
    }
}
