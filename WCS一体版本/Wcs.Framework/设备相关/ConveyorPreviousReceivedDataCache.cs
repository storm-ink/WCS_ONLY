using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 输送线上一次接收到的数据缓存
    /// </summary>
    public class ConveyorPreviousReceivedDataCache
    {
        List<CompareResult<Devices.NetTransferObject>> compareResult;
        public ConveyorPreviousReceivedDataCache()
        {
            ReceivedBytes = new byte[] { };
            compareResult = new List<CompareResult<Devices.NetTransferObject>>();
        }
        /// <summary>
        /// 数据包原始字节数组
        /// </summary>
        public byte[] ReceivedBytes { get; set; }
        /// <summary>
        /// 于上次的状态变化数据
        /// </summary>
        public CompareResult<Devices.NetTransferObject>[] CompareResults
        {
            get
            {
                return compareResult.ToArray();
            }
        }
        /// <summary>
        /// 是否是首次加载
        /// </summary>
        public Boolean IsFirstLoad
        {
            get
            {
                return ReceivedBytes.Length == 0;
            }
        }

        /// <summary>
        /// 添加比较结果到缓存
        /// </summary>
        /// <param name="compareResults"></param>
        public void AddCompareResults(params CompareResult<Devices.NetTransferObject>[] compareResults)
        {
            compareResult.AddRange(compareResults);
        }

        /// <summary>
        /// 从比较结果中移除所有已处理的对象
        /// </summary>
        public void RemoveAllHandledCompareResults()
        {
            while (compareResult.Any(x => x.Handled))
            {
                compareResult.Remove(compareResult.First(x => x.Handled));
            }
        }
    }
}
