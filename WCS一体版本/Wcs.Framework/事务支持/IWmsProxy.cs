using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// wms 代理，wcs 通过此代理与 wms 通信。
    /// </summary>
    public interface IWmsProxy
    {
        /// <summary>
        /// 任务申请
        /// </summary>
        /// <returns></returns>
        void Request(DC2.WcsRequestInfo requestInfo);

        /// <summary>
        /// 任务完成
        /// </summary>
        /// <param name="args"></param>
        void CompleteTask(DC2.WcsTaskInfo taskInfo);
    }
}
