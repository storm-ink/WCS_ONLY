using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示 TransactionMethod 的准备和回滚方法
    /// </summary>
    public delegate void SupportTransactionMethodActionDelegate<TSupportTransactionStateObject>(TSupportTransactionStateObject state) where TSupportTransactionStateObject : class,ISupportTransactionObject;
    /// <summary>
    /// 表示一个支持事务的方法对象<br />
    /// 事务的执行过程先后顺序为：<br />
    /// <para>1、Prepare</para>
    /// <para>2、Rollback / Commit</para>
    /// <para>在事务的所有分支都准备就绪后才会执行所有事务分支的 Commit 方法</para>
    /// <remarks>
    ///Commit	通知登记的对象事务正在提交。 
    ///InDoubt	通知登记的对象事务的状态不确定。 
    ///Prepare	通知登记的对象事务正在为提交做准备。 
    ///Rollback	通知登记的对象事务正在回滚（中止）。 
    /// </remarks>
    /// </summary>
    public sealed class SupportTransactionMethod<TSupportTransactionStateObject> : IEnlistmentNotification
        where TSupportTransactionStateObject : class,ISupportTransactionObject
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 状态数据
        /// </summary>
        public TSupportTransactionStateObject State { get; private set; }
        
        /// <summary>
        /// 事务准备时要执行的方法
        /// </summary>
        SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> PrepareAction { get; set; }
        /// <summary>
        /// 事务回滚时要执行的方法
        /// </summary>
        SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> RollbackAction { get; set; }
        /// <summary>
        /// 事务提交时要执行的方法
        /// </summary>
        SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> CommitAction { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="state">状态对象</param>
        /// <param name="prepareAction">事务准备时要执行的方法(必须)</param>
        /// <param name="rollbackAction">事务回滚时要执行的方法(必须)</param>
        /// <param name="commitAction">事务提交时要执行的方法(可为null)</param>
        SupportTransactionMethod(TSupportTransactionStateObject state,SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> prepareAction, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> rollbackAction, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> commitAction)
        {
            this.State = state;
            if (prepareAction == null)
            {
                throw new ArgumentNullException("prepareAction");
            }
            if (rollbackAction == null)
            {
                throw new ArgumentNullException("rollbackAction");
            }
            this.PrepareAction = prepareAction;
            this.RollbackAction = rollbackAction;
            this.CommitAction = commitAction;

            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);

            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Initialized;
                _logger.Debug1(string.Format("{0} 的 SupportTransactionObjectStatus 被修改为 {1}", this.State, this.State.SupportTransactionObjectStatus), this, this.State);
            }
        }
        public void Commit(Enlistment enlistment)
        {
            if (this.CommitAction != null)
            {
                this.CommitAction(this.State);
            }
            enlistment.Done();
            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Committed;
                _logger.Debug1(string.Format("{0} 的 SupportTransactionObjectStatus 被修改为 {1}", this.State, this.State.SupportTransactionObjectStatus), this, this.State);
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
                this.PrepareAction(this.State);
                preparingEnlistment.Prepared(); 
                if (this.State != null)
                {
                    this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Prepared;
                    _logger.Debug1(string.Format("{0} 的 SupportTransactionObjectStatus 被修改为 {1}", this.State, this.State.SupportTransactionObjectStatus), this, this.State);

                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);

                preparingEnlistment.ForceRollback();
            }
            //try
            //{
            //    this.PrepareAction(this.State);
            //    preparingEnlistment.Prepared();
            //    this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Prepared;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    preparingEnlistment.ForceRollback();
            //}
        }

        public void Rollback(Enlistment enlistment)
        {
            this.RollbackAction(this.State);
            enlistment.Done();
            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Rolledback;
                _logger.Debug1(string.Format("{0} 的 SupportTransactionObjectStatus 被修改为 {1}", this.State, this.State.SupportTransactionObjectStatus), this, this.State);
            }
        }

        public static void Join<TSupportTransactionStateObject>(TSupportTransactionStateObject state, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> prepareAction, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> rollbackAction)
            where TSupportTransactionStateObject : class,ISupportTransactionObject
        {
            Join<TSupportTransactionStateObject>(state, prepareAction, rollbackAction, null);
        }

        public static void Join<TSupportTransactionStateObject>(TSupportTransactionStateObject state, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> prepareAction, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> rollbackAction, SupportTransactionMethodActionDelegate<TSupportTransactionStateObject> commitAction)
            where TSupportTransactionStateObject : class,ISupportTransactionObject
        {
            if (Transaction.Current != null)
            {
                SupportTransactionMethod<TSupportTransactionStateObject> tm = new SupportTransactionMethod<TSupportTransactionStateObject>(state, prepareAction, rollbackAction, commitAction);
            }
            else
            {
                //throw new InvalidOperationException("该操作必须包含上下文事务。");

                try
                {
                    using (TransactionScope tsc = new TransactionScope())
                    {
                        SupportTransactionMethod<TSupportTransactionStateObject> tm = new SupportTransactionMethod<TSupportTransactionStateObject>(state, prepareAction, rollbackAction, commitAction);
                        tsc.Complete();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, null);
                    throw;
                }
            }
        }
    }
}
