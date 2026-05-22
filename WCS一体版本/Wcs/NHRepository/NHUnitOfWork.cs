using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
namespace Wcs
{
    public class NHUnitOfWork:IDisposable, INHUnitOfWork
    {
        ISession _session;
        public static readonly ISessionFactory _factory;
        static NHUnitOfWork()
        {
            _factory = new NHibernate.Cfg.Configuration().Configure().BuildSessionFactory();
        }

        public NHUnitOfWork()
            : this(System.Data.IsolationLevel.ReadCommitted)
        {
        }

        public NHUnitOfWork(System.Data.IsolationLevel solationLevel)
        {
            _session = _factory.OpenSession();
            _session.BeginTransaction(solationLevel);
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
            if (!session.IsOpen)
            {
                return;
            }

            if (session.Transaction != null && session.Transaction.IsActive)
            {
                session.Transaction.Rollback();
            }
            
            _session.Dispose();
        }

        #endregion
    }
}
