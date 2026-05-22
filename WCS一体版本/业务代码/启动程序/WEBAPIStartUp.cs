using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.Cfg;
//using WEBAPI;

namespace ZHQXC
{
    public class WEBAPIStartUp : IApplicationStartup
    {
        public void Initialize(StartupElement element)
        {
        }
        public void Run(IWcsApplication application)
        {
            var port = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("wcsWebApiAddressPort", 9898);
            WebApiSelfHostHelper.StartWebApi<HttpService>(port);
        }
    }
}
