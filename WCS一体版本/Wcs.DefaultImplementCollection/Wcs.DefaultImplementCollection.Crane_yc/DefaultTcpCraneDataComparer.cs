using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 默认的堆垛机状态数据对比器
    /// </summary>
    public class DefaultTcpCraneDataComparer : IDataComparer<NetTransferObject, RequestStateCommandReplyTelexTransferObject>
    {
        public Logger Logger { get; private set; }
        public event DataChangedEventHandler<NetTransferObject> DataChanged;
        public DefaultTcpCraneDataComparer()
        {
            this.Logger = LogManager.GetCurrentClassLogger();
        }

        private void OnDataChanged(CompareResult<NetTransferObject> compareResult)
        {
            if (DataChanged != null)
            {
                if (!needFireDataChanged(compareResult))
                {
                    return;
                }

                DataChanged.Invoke(compareResult);
            }
        }

        Boolean needFireDataChanged(CompareResult<NetTransferObject> compareResult)
        {
            //string typeFullName=((object)compareResult.newObject ?? (object)compareResult.oldObject).GetType().FullName;
            //string settings = Wcs.Framework.Cfg.Configuration.GetSetting<string>("deviceDataChangedEventTraceSettings/" + typeFullName, "");
            //if (string.IsNullOrWhiteSpace(settings))
            //{
            //    return false;
            //}

            //var tracePropertys = settings.Split(';')
            //    .Where(x=>x.Split('=').Length == 2)
            //    .Select(x => new 
            //    {
            //        name = x.Split('=')[0].Trim(),
            //        trace= x.Split('=')[1].Trim()=="1" || x.Split('=')[1].Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase)
            //    });

            //var diffs=from o in compareResult.differences
            //          join x in tracePropertys.Where(setting=>setting.trace==true)
            //          on o.propertyName.ToLower() equals x.name.ToLower()
            //          select o;

            //if(diffs.Count()==0)
            //{
            //    return false;
            //}

            return true;
        }

        CompareResult<NetTransferObject>[] IDataComparer<NetTransferObject, RequestStateCommandReplyTelexTransferObject>.Compare(RequestStateCommandReplyTelexTransferObject oldData, RequestStateCommandReplyTelexTransferObject newData)
        {
            if (oldData == null && newData == null)
            {
                return new CompareResult<NetTransferObject>[0];
            }


            var compareResult = oldData.Compare(newData);
            List<CompareResult<NetTransferObject>> result = new List<CompareResult<NetTransferObject>>();
            if (compareResult != null && compareResult.differences.Length > 0)
            {
                result.Add(compareResult);
                OnDataChanged(compareResult);
            }

            return result.ToArray();
        }
    }
}
