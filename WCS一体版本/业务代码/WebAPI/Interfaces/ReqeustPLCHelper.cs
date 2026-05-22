using ZHQXC.WebAPI.Entity;
using Newtonsoft.Json;
using NHibernate.Linq;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using NLog;


namespace ZHQXC.WebAPI
{
    class ReqeustPLCHelper
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        private static object lockObj = new object();

        public static string IpPort
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("wmsWebApiAddress", "127.0.0.1:8189");
            }
        }
        public static int numCheck = 0;


        static ThreadRunningLog PLCWriteLogs
        {
            get
            {
                ThreadRunningLog log = new ThreadRunningLog();
                log.Init("PLCWriteLogs");
                return log;
            }
        }

    }
}

