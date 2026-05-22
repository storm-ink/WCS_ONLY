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
    public delegate void TransactionMethodActionDelegate();
    /// <summary>
    /// 表示一个支持事务的方法对象<br />
    /// 事务的执行过程先后顺序为：<br />
    /// <para>1、Prepare</para>
    /// <para>2、Rollback / Commit</para>
    /// <para>在事务的所有分支都准备就绪后才会执行所有事务分支的 Commit 方法</para>
    /// </summary>
    public sealed class TransactionMethod : IEnlistmentNotification
    {
        public static void Apply(TransactionMethodActionDelegate prepareAction, TransactionMethodActionDelegate rollbackAction)
        {
            Apply(prepareAction, rollbackAction, null);
        }

        public static void Apply(TransactionMethodActionDelegate prepareAction, TransactionMethodActionDelegate rollbackAction, TransactionMethodActionDelegate commitAction)
        {
            TransactionMethod tm = new TransactionMethod(prepareAction, rollbackAction, commitAction);
        }

        /// <summary>
        /// 事务准备时要执行的方法
        /// </summary>
        TransactionMethodActionDelegate PrepareAction { get; set; }
        /// <summary>
        /// 事务回滚时要执行的方法
        /// </summary>
        TransactionMethodActionDelegate RollbackAction { get; set; }
        /// <summary>
        /// 事务提交时要执行的方法
        /// </summary>
        TransactionMethodActionDelegate CommitAction { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prepareAction">事务准备时要执行的方法(必须)</param>
        /// <param name="rollbackAction">事务回滚时要执行的方法(必须)</param>
        /// <param name="commitAction">事务提交时要执行的方法(可为null)</param>
        TransactionMethod(TransactionMethodActionDelegate prepareAction, TransactionMethodActionDelegate rollbackAction, TransactionMethodActionDelegate commitAction)
        {
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
        }
        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("Commit notification received");
            if (this.CommitAction != null)
            {
                try
                {
                    this.CommitAction();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("InDoubt notification received");
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine("Prepare notification received");
            try
            {
                this.PrepareAction();
                preparingEnlistment.Prepared();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                preparingEnlistment.ForceRollback();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine("Rollback notification received");
            try
            {
                this.RollbackAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            enlistment.Done();
        }
    }
}
