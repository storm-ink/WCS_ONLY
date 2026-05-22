using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的 Wms 通讯接口代理实现
    /// </summary>
    public class DefaultWmsProxy:IWmsProxy
    {
        public void Request(DC2.WcsRequestInfo requestInfo)
        {
            using (WmsServiceForWcs.WcfWmsServiceForWcsClient client = new WmsServiceForWcs.WcfWmsServiceForWcsClient())
            {
                client.Request(requestInfo);
                client.Close();
            }
        }

        public void CompleteTask(DC2.WcsTaskInfo taskInfo)
        {
            using (WmsServiceForWcs.WcfWmsServiceForWcsClient client = new WmsServiceForWcs.WcfWmsServiceForWcsClient())
            {
                client.CompleteTask(taskInfo);
                client.Close();
            }
        }
    }
}
