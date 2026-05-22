using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示实现此接口的对象是一个可编辑任务的父对象
    /// </summary>
    public interface IEditableTaskOwner
    {
        /// <summary>
        /// 继续执行任务
        /// </summary>
        /// <param name="task"></param>
        void Resume(EquipmentAction task,out Boolean cancelled);
        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="task"></param>
        void Suspend(EquipmentAction task, out Boolean cancelled);
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="task"></param>
        void Cancel(EquipmentAction task, out Boolean cancelled);

        /// <summary>
        /// 强制完成任务
        /// </summary>
        /// <param name="task"></param>
        void Complete(EquipmentAction task, out Boolean cancelled);
    }
}
