using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NLog;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
namespace Wcs
{
    public class NHUnitOfWork : IDisposable, INHUnitOfWork
    {
        ISession _session;
        public static readonly ISessionFactory _factory;
        public static NHibernate.Cfg.Configuration _configuration;
        private bool _isAlreadyDisposed;
        static NHUnitOfWork()
        {
            _configuration = new NHibernate.Cfg.Configuration().Configure();
            _factory = _configuration.BuildSessionFactory();
        }

        public NHUnitOfWork()
            : this(System.Data.IsolationLevel.ReadCommitted)
        {
        }

        public NHUnitOfWork(System.Data.IsolationLevel solationLevel)
        {
            _session = _factory.OpenSession();
            _session.BeginTransaction(solationLevel);

            PushCurrentSession(_session);
        }


        public ISession session
        {
            get
            {
                return _session;
            }
        }

        public void Commit()
        {
            if (_isAlreadyDisposed)
            {
                throw new ObjectDisposedException("NhUnitOfWork");
            }

            if (!session.IsOpen)
            {
                return;
            }
            if (session.Transaction == null || !session.Transaction.IsActive)
            {
                return;
            }

            session.Transaction.Commit();
        }

        public void Rollback()
        {
            if (_isAlreadyDisposed)
            {
                throw new ObjectDisposedException("NhUnitOfWork");
            }

            if (!session.IsOpen)
            {
                return;
            }
            if (session.Transaction == null || !session.Transaction.IsActive)
            {
                return;
            }

            session.Transaction.Rollback();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);

            PopCurrentSession();

        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (this._isAlreadyDisposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    if (session.Transaction != null && session.Transaction.IsActive)
                    {
                        session.Transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
                
                _session.Dispose();

            }
            _isAlreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        #region 调用上下文中的 Session 管理

        const String CallContextSessionDataName = "WcsISessionStack";
        static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 获取当前调用上下文当中的session对象
        /// </summary>
        /// <param name="callContext">当前调用上下文</param>
        /// <returns></returns>
        public static ISession CurrentSession
        {
            get
            {
                Stack<ISession> stack = (Stack<ISession>)CallContext.GetData(CallContextSessionDataName);
                if (stack == null)
                {
                    _logger.Warn("数据槽中的堆栈项的个数为 0 。", CallContextSessionDataName);
                    StackTrace st = new StackTrace();
                    _logger.Trace(st.ToString(), CallContextSessionDataName);

                    return null;
                }

                ISession session = stack.Peek();

                if (!session.IsOpen)
                {
                    throw new InvalidOperationException("当前 session 对象已处于关闭状态。");
                }

                if (session.Transaction != null && !session.Transaction.IsActive)
                {
                    throw new InvalidOperationException("当前 session 存在一个事务，并且该事务已处于不可用状态。");
                }

                return session;
            }
        }

        /// <summary>
        /// 在当前调用上下文当压入一个活动的session对象
        /// </summary>
        /// <param name="callContext">当前调用上下文</param>
        /// <param name="session"></param>
        static void PushCurrentSession(ISession session)
        {
            if (!session.IsOpen)
            {
                throw new InvalidOperationException("压入的 session 对象已处于关闭状态。");
            }

            if (session.Transaction != null && !session.Transaction.IsActive)
            {
                throw new InvalidOperationException("压入的 session 存在一个事务，并且该事务已处于不可用状态。");
            }

            Stack<ISession> stack = (Stack<ISession>)CallContext.GetData(CallContextSessionDataName);
            if (stack == null)
            {
                stack = new Stack<ISession>();
                CallContext.SetData(CallContextSessionDataName, stack);
            }

            stack.Push(session);

            //if (stack.Count > 1)
            //{
            //    _logger.Warn1(String.Format("数据槽中的堆栈项的个数为 {0} 。如有可能，应使堆栈中的项数不大于 1。", stack.Count), CallContextSessionDataName);
            //    StackTrace st = new StackTrace();
            //    _logger.Trace(st.ToString(), CallContextSessionDataName);
            //}
        }

        /// <summary>
        /// 从当前调用上下文存在的session栈中弹出一个
        /// </summary>
        /// <param name="callContext"></param>
        static void PopCurrentSession()
        {
            Stack<ISession> stack = (Stack<ISession>)CallContext.GetData(CallContextSessionDataName);
            ISession context = stack.Pop();

            if (stack.Count == 0)
            {
                CallContext.FreeNamedDataSlot(CallContextSessionDataName);
            }
        }
        #endregion
    }
}
