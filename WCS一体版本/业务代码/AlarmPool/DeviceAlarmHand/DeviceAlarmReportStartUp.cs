using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImplementCollection.Business;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;
using NLog;
using System.Threading;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 设备故障记录归档启动线程
    /// </summary>
    public class DeviceAlarmReportStartUp : IApplicationStartup
    {
        Logger _logger = LogManager.CreateNullLogger();
        Thread _thread;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
        }

        public void Run(IWcsApplication application)
        {
            ParameterizedThreadStart Start = new ParameterizedThreadStart(AlarmRecordHelper.Proc);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }
    }
}
