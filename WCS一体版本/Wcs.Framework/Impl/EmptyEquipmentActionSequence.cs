using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 一个空的物理动作序列，该序列不会启动任务队列监控线程驱动设备
    /// </summary>
    public class EmptyEquipmentActionSequence : EquipmentActionSequence
    {
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="device">           此序列关联的设备. </param>
        /// <param name="sortComparer">     排序对比器. </param>
        /// <param name="logTarget">        日志输出对象. </param>
        /// <param name="actionFilters">    任务过滤器集合. </param>
        /// <param name="additionalTasks">  设备动作序列额外的要执行的任务 当动作序列轮询发现当前无可下发的动作时会此行这些操作. </param>
        /// <param name="group">            所属序列组. </param>
        public EmptyEquipmentActionSequence(Device device, EquipmentActionSortComparer sortComparer, LogTarget logTarget, EquipmentSequenceActionFilter[] actionFilters, IEquipmentActionSequenceAdditionalTask[] additionalTasks, EquipmentActionSequenceGroup group)
            : base(device, sortComparer, logTarget, actionFilters, additionalTasks, group)
        {
        }
        /// <summary>
        /// 一个空的序列主线程.执行后直接返回方法体
        /// </summary>
        protected override void Process()
        {

        }
    }
}
