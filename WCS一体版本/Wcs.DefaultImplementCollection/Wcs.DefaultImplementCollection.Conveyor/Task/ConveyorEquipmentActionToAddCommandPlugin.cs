using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线命令外挂程序
    /// </summary>
    public abstract class ConveyorEquipmentActionToAddCommandPlugin
    {
        /// <summary>
        /// 外挂处理程序
        /// 着力于解决非常规命令问题
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cmd"></param>
        public abstract void EquipmentActionToAddCommandPlugin(EquipmentAction action, ref TaskCommand cmd);
    }
}