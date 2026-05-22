using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 堆垛机上一次接收到的数据缓存
    /// </summary>
    public class CranePreviousReceivedDataCache
    {
        public const string TASK_RUNNING = "taskRunning";
        public const string TASK_ERROR = "taskError";
        public const string TASK_COMPLETED = "taskCompleted";
        public const string DEVICE_ERROR = "deviceError";
        public const string TASK_LOCATION_CHANGED = "task_Location_Changed";
        List<CompareResult<Devices.CraneStatusInfo>> compareResult;
        Dictionary<CompareResult<Devices.CraneStatusInfo>, Dictionary<string, bool>> handleKeys = new Dictionary<CompareResult<Devices.CraneStatusInfo>, Dictionary<string, bool>>();
        public CranePreviousReceivedDataCache()
        {
            CraneStatusInfo = null;
            compareResult = new List<CompareResult<Devices.CraneStatusInfo>>();
        }
        /// <summary>
        /// 数据包原始字节数组
        /// </summary>
        public Devices.CraneStatusInfo CraneStatusInfo { get; set; }
        /// <summary>
        /// 于上次的状态变化数据
        /// </summary>
        public CompareResult<Devices.CraneStatusInfo>[] CompareResults
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
                return CraneStatusInfo == null;
            }
        }

        /// <summary>
        /// 添加比较结果到缓存
        /// </summary>
        /// <param name="compareResults"></param>
        public void AddCompareResults(params CompareResult<Devices.CraneStatusInfo>[] compareResults)
        {
            compareResult.AddRange(compareResults);
            foreach (var item in compareResults)
            {
                handleKeys.Add(item, new Dictionary<string, bool>());
                handleKeys[item].Add(TASK_RUNNING,false);
                handleKeys[item].Add(TASK_ERROR,false);
                handleKeys[item].Add(TASK_COMPLETED,false);
                handleKeys[item].Add(DEVICE_ERROR, false);
                handleKeys[item].Add(TASK_LOCATION_CHANGED, false);
            }                      
        }                          
                                   
        public bool IsHandled(CompareResult<Devices.CraneStatusInfo> compareResult, string key)
        {
            return handleKeys[compareResult][key];
        }

        public void SetHandled(CompareResult<Devices.CraneStatusInfo> compareResult, string key,bool handled)
        {
            handleKeys[compareResult][key] = handled;
        }

        /// <summary>
        /// 从比较结果中移除所有已处理的对象
        /// </summary>
        public void RemoveAllHandledCompareResults()
        {
            while (compareResult.Any(x => handleKeys[x].Values.All(v=>v==true)))
            {
                compareResult.Remove(compareResult.First(x => handleKeys[x].Values.All(v => v == true)));
            }
        }
    }
}
