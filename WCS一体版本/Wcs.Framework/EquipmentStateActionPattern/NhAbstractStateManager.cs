using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.Framework
{
    /// <summary>
    /// 使用数据库保存恢复信息的状态上下文
    /// </summary>
    public abstract class NhAbstractStateManager:AbstractStateManager
    {
        /// <summary>
        /// 初始化 <seealso cref="T:NhAbstractStateManager"/> 类的新实例。
        /// </summary>
        /// <param name="equipmentAction">物理动作</param>
        /// <param name="device">关联设备</param>
        protected NhAbstractStateManager(EquipmentAction equipmentAction, TaskableDevice device)
            : base(equipmentAction, device)
        {
        }
        /// <summary>
        /// 从数据库中已保存的恢复信息（如果存在）中恢复当前上下文
        /// </summary>
        protected override void Restore()
        {
            _logger.Debug1(string.Format("准备恢复 {0} 的状态信息...", this), this, this.EquipmentAction);

            StateManagerRestoreInfo smri;

            using(NHUnitOfWork unitOfWork=new NHUnitOfWork())
            {
                smri=unitOfWork
                    .session
                    .Query<StateManagerRestoreInfo>()
                    .SingleOrDefault(x => x.EquipmentActionId == this.EquipmentAction.Id);

                unitOfWork.Commit();
            }

            if (smri == null)
            {
                _logger.Debug1("未找到已保存的恢复信息.", this, this.EquipmentAction);
            }
            else
            {
                _logger.Debug1(string.Format("找到恢复信息：物理动作#{0}，当前状态#{1}，创建@{2}，开始恢复...", smri.EquipmentActionId, smri.CurrentStateName, smri.CreatedAt), this);

                var state = this.StateLink.SingleOrDefault(x => x.Name == smri.CurrentStateName);
                if (state == null)
                {
                    throw new Exception(string.Format("在当前状态链中未找到存储的恢复信息中描述的当前状态“{0}”，请检查是否在状态恢复信息保存后修改过上下文的状态链信息。", smri.CurrentStateName));
                }

                this.CurrentState = state;
            }

            _logger.Debug1(string.Format("{0} 状态恢复结束.", this), this);
        }

        /// <summary>
        /// 将当前上下文的恢复信息保存到数据库
        /// </summary>
        protected override void Save()
        {
            _logger.Debug1(string.Format("开始保存 {0}...", this), this, this.EquipmentAction);
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                var smri = unitOfWork.session.Query<StateManagerRestoreInfo>().FirstOrDefault(x => x.EquipmentActionId == this.EquipmentAction.Id);
                if (smri == null)
                {
                    smri = new StateManagerRestoreInfo(this);

                    unitOfWork.session.Save(smri);
                }
                else
                {
                    smri.CurrentStateName = this.CurrentState.Name;
                    unitOfWork.session.Update(smri);
                }
                unitOfWork.Commit();
            }
            _logger.Debug1(string.Format("{0} 保存成功.", this), this, this.EquipmentAction);
        }

        public override void Remove()
        {
            _logger.Debug1(string.Format("开始移除 {0}...", this), this, this.EquipmentAction);
            if (this.EquipmentAction != null)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var smri = unitOfWork
                           .session
                           .Query<StateManagerRestoreInfo>()
                           .SingleOrDefault(x => x.EquipmentActionId == this.EquipmentAction.Id);
                    if (smri != null)
                        unitOfWork.session.Delete(smri);

                    unitOfWork.Commit();
                }
                _logger.Debug1(string.Format("{0} 移除成功.", this), this, this.EquipmentAction);
            }
            else
                _logger.Debug1(string.Format("未查询到任务 本次未移除成功.", this), this, this.EquipmentAction);

            //清理内存对象
            Dispose();
        }

        protected override void OnEquipmentActionStatusChanged(Events.EquipmentActionStatusChangedEvent args)
        {
            base.OnEquipmentActionStatusChanged(args);
        }

        protected override void StateLineVaild()
        {
            base.StateLineVaild();
        }
    }
}
