using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
namespace Wcs
{
    public class NHBackupServerUnitOfWork:IDisposable,INHUnitOfWork
    {
        ISession _session;
        public static readonly ISessionFactory _factory;
        static NHBackupServerUnitOfWork()
        {
            _factory = new NHibernate.Cfg.Configuration().Configure(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(NHBackupServerUnitOfWork).Assembly.Location), "backup_server_hibernate.cfg.xml")).BuildSessionFactory();
        }

        public NHBackupServerUnitOfWork()
            : this(System.Data.IsolationLevel.ReadCommitted)
        {
        }

        public NHBackupServerUnitOfWork(System.Data.IsolationLevel solationLevel)
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
    }
}
