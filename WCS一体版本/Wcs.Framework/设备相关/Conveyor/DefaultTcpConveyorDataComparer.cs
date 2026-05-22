using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Threading;

namespace Wcs.Framework.Devices.Conveyor
{
    /// <summary>
    /// 默认的输送线状态数据对比器
    /// </summary>
    public class DefaultTcpConveyorDataComparer : IDataComparer<NetTransferObject,_DB1>
    {
        public event DataChangedEventHandler<NetTransferObject> DataChanged;

        public CompareResult<NetTransferObject>[] Compare(_DB1 oldData,_DB1 newData)
        {
            List<CompareResult<NetTransferObject>> result = new List<CompareResult<NetTransferObject>>();

            if (oldData == null || newData == null)
            {
                return result.ToArray();
            }

            foreach (var oldItem in oldData.Items)
            {
                if (oldItem.Value.Length != newData.Items[oldItem.Key].Length)
                {
                    throw new Exception(String.Format("{0} 旧集合的长度 {1} 和新集合的长度 {2} 不一致", oldItem.Key, oldItem.Value.Length, newData.Items[oldItem.Key].Length));
                }

                for (int i = 0; i < oldItem.Value.Length; i++)
                {
                    var compareResult = oldItem.Value[i].Compare(newData.Items[oldItem.Key][i]);
                    if (compareResult != null && compareResult.differences.Length > 0)
                    {
                        result.Add(compareResult);
                        OnDataChanged(compareResult);
                    }
                }
            }

            return result.ToArray();
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
    }
}
