using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NHibernate;
using NLog;
namespace Wcs
{
    public class NHDeviceTrackingDataUnitOfWork:IDisposable,INHUnitOfWork
    {
        ISession _session;
        public static readonly ISessionFactory _factory;
        public static NHibernate.Cfg.Configuration _configuration;
        static Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _isAlreadyDisposed;
        static NHDeviceTrackingDataUnitOfWork()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(NHBackupServerUnitOfWork).Assembly.Location), "devicetrackingdata_hibernate.cfg.xml");
            if (System.IO.File.Exists(path))
            {
                _configuration=new NHibernate.Cfg.Configuration().Configure(path);
            }
            else
            {
                var appConfig=System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                String filePath=appConfig.FilePath;
                appConfig=null;
                
                XmlDocument doc=new XmlDocument();
                doc.Load(filePath);
                XmlNode configurationNode=doc.SelectSingleNode("/configuration").ChildNodes.Cast<XmlNode>().Single(x=>x.Name=="deviceTrackingData-hibernate-configuration");
                XmlTextReader reader = new XmlTextReader(configurationNode.OuterXml, XmlNodeType.Document, null);
                _configuration = new NHibernate.Cfg.Configuration().Configure(reader);
            }

            _factory = _configuration.BuildSessionFactory();
        }

        public NHDeviceTrackingDataUnitOfWork()
            : this(System.Data.IsolationLevel.ReadCommitted)
        {
        }

        public NHDeviceTrackingDataUnitOfWork(System.Data.IsolationLevel solationLevel)
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
            Dispose(true);

            GC.SuppressFinalize(this);

        }

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
