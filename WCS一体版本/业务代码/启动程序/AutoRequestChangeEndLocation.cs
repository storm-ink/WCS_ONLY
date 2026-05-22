using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NHibernate.Linq;
using NLog;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace BOE.启动程序
{
    class AutoRequestChangeEndLocation : IApplicationStartup
    {
        Logger _logger;
        static Thread _thread;
        public void Initialize(StartupElement element)
        {
           
        }

        public void Run(IWcsApplication application)
        {
            _logger = LogManager.CreateNullLogger();

            _thread = new Thread(autoReqeustChangeEndLocation);
            _thread.IsBackground = true;
            _thread.Name = "自动请求变换目的地线程";
            _thread.StartAndManaged();

            _logger.Info(String.Format("自动请求变换目的地线程已启动"));
        }

        private void autoReqeustChangeEndLocation()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }
    }
}
