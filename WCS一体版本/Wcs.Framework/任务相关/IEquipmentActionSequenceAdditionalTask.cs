using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备动作序列额外的要执行的任务接口
    /// 当动作序列轮询发现当前无可下发的动作时会此行此操作
    /// </summary>
    public interface IEquipmentActionSequenceAdditionalTask
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sequence">动作序列</param>
        /// <param name="unitOfWork">执久化上下文</param>
        void Execute(EquipmentActionSequence sequence,NHUnitOfWork unitOfWork);
    }
}
