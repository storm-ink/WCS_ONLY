using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Threading;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的堆垛机状态数据对比器
    /// </summary>
    public class DefaultCranStatusDataComparer : IDataComparer<CraneStatusInfo, CraneStatusInfo>
    {
        CraneStatusInfo _lastStatus;
        public event DataChangedEventHandler<CraneStatusInfo> DataChanged;

        public CompareResult<CraneStatusInfo>[] Compare(CraneStatusInfo data)
        {
            if (data == null || _lastStatus==null)
            {
                _lastStatus = data;
                return null;
            }

            if (String.Equals(_lastStatus._CraneProxyStatus.Text, data._CraneProxyStatus.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                _lastStatus = data;
                return null;
            }

            var compareResult = _lastStatus.Compare(data);
            _lastStatus = data;

            if (compareResult.differences.Length > 0)
            {
                onDataChanged(compareResult);

                return new CompareResult<CraneStatusInfo>[] { compareResult };
            }
            else
            {
                return null;
            }
        }

        private void onDataChanged(CompareResult<CraneStatusInfo> compareResult)
        {
            if (DataChanged != null)
            {
                //ThreadPool.QueueUserWorkItem((state) =>
                //{
                //    DataChanged.Invoke(compareResult);
                //});
                DataChanged.Invoke(compareResult);
            }
        }
    }
}
