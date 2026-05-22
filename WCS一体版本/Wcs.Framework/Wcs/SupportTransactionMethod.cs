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
    /// </summary>
    public sealed class SupportTransactionMethod<TSupportTransactionStateObject> : IEnlistmentNotification
        where TSupportTransactionStateObject : class,ISupportTransactionObject
    {
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
            }
        }
        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("Commit notification received");
            if (this.CommitAction != null)
            {
                this.CommitAction(this.State);
            }
            enlistment.Done();
            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Committed;
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("InDoubt notification received");
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine("Prepare notification received");
            this.PrepareAction(this.State);
            preparingEnlistment.Prepared(); 
            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Prepared;
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
            Console.WriteLine("Rollback notification received");
            this.RollbackAction(this.State);
            enlistment.Done();
            if (this.State != null)
            {
                this.State.SupportTransactionObjectStatus = SupportTransactionObjectStatus.Rolledback;
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
            using (TransactionScope tsc = new TransactionScope())
            {
                SupportTransactionMethod<TSupportTransactionStateObject> tm = new SupportTransactionMethod<TSupportTransactionStateObject>(state, prepareAction, rollbackAction, commitAction);
                tsc.Complete();
            }
        }
    }
}
