using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 已使用的输送线设备任务地址索引获取接口。
    /// 用于获取已使用的任务地址索引信息(为输送线分配任务时需要指定具体的任务索引地址)
    /// </summary>
    public interface IUsedConveyorTaskBlockIndexGetter
    {
        /// <summary>
        /// 获取指定的输送线设备已使用的任务地址索引集合
        /// </summary>
        /// <param name="conveyorDevice">输送线设备</param>
        /// <returns>已使用的任务地址索引集合。该值与PLC中的索引地址相同。</returns>
        Int32[] GetUsedIndexs(ConveyorDevice conveyorDevice);
    }
}
