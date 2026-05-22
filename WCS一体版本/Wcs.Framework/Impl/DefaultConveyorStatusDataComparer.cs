using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Threading;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的输送线状态数据对比器
    /// </summary>
    public class DefaultConveyorStatusDataComparer : IDataComparer<NetTransferObject, Byte[]>
    {
        public DefaultConveyorNetPackageDecoder Decoder { get; private set; }
        public event DataChangedEventHandler<NetTransferObject> DataChanged;
        public DefaultConveyorStatusDataComparer(DefaultConveyorNetPackageDecoder decoder)
        {
            if (decoder == null)
            {
                throw new ArgumentNullException("decoder");
            }

            this.Decoder = decoder;
        }

        /// <summary>
        /// 最后一次用于对比的收到的数据包字
        /// </summary>
        byte[] _lastReceivedDataPartBytes;

        /// <summary>
        /// 
        /// </summary>
        Dictionary<Type, NetTransferObject[]> _lastValues;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPartBytes"></param>
        public CompareResult<NetTransferObject>[] Compare(byte[] dataPartBytes)
        {
            if (dataPartBytes == null)
            {
                throw new ArgumentNullException("dataPartBytes");
            }

            if (dataPartBytes.Length <4)
            {
                throw new ArgumentOutOfRangeException("dataPartBytes 长度必须大于 4");
            }

            if (_lastValues == null || _lastValues.Count == 0)
            {
                _lastReceivedDataPartBytes = dataPartBytes;
                _lastValues = Decoder.DecodePackage(dataPartBytes);
                return null;
            }

            if (dataPartBytes.SequenceEqual(_lastReceivedDataPartBytes))
            {
                return null;
            }

            List<CompareResult<NetTransferObject>> result = new List<CompareResult<NetTransferObject>>();
            var newValues = Decoder.DecodePackage(dataPartBytes);
            foreach (var item in _lastValues)
            {
                if (item.Value.Length != newValues[item.Key].Length)
                {
                    throw new Exception(String.Format("{0} 旧集合的长度 {1} 和新集合的长度 {2} 不一致", item.Key, item.Value.Length, newValues[item.Key].Length));
                }

                for (int i = 0; i < item.Value.Length; i++)
                {
                    var compareResult = item.Value[i].Compare(newValues[item.Key][i]);
                    if (compareResult != null && compareResult.differences.Length > 0)
                    {
                        result.Add(compareResult);
                        OnDataChanged(compareResult);
                    }
                }
            }

            _lastReceivedDataPartBytes = dataPartBytes;
            _lastValues = newValues;

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
                //仅为支持在绑定多处理程序时的异常调用
                //ThreadPool.QueueUserWorkItem(delegate
                //{
                //    DataChanged.Invoke(compareResult);
                //});
                DataChanged.Invoke(compareResult);
            }
        }

        Boolean needFireDataChanged(CompareResult<NetTransferObject> compareResult)
        {
            string typeFullName=((object)compareResult.newObject ?? (object)compareResult.oldObject).GetType().FullName;
            string settings = Wcs.Framework.Cfg.Configuration.GetSetting<string>("deviceDataChangedEventTraceSettings/" + typeFullName, "");
            if (string.IsNullOrWhiteSpace(settings))
            {
                return false;
            }

            var tracePropertys = settings.Split(';')
                .Where(x=>x.Split('=').Length == 2)
                .Select(x => new 
                {
                    name = x.Split('=')[0].Trim(),
                    trace= x.Split('=')[1].Trim()=="1" || x.Split('=')[1].Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase)
                });

            var diffs=from o in compareResult.differences
                      join x in tracePropertys.Where(setting=>setting.trace==true)
                      on o.propertyName.ToLower() equals x.name.ToLower()
                      select o;

            if(diffs.Count()==0)
            {
                return false;
            }

            return true;
        }
    }
}
