using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot命令外挂程序
    /// </summary>
    public abstract class RobotEquipmentActionToAddCommandPlugin
    {
        /// <summary>
        /// 外挂处理程序
        /// 着力于解决非常规命令问题
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cmd"></param>
        public abstract void EquipmentActionToAddCommandPlugin(EquipmentAction action, ref RobotTaskCommand cmd);

        /// <summary>
        /// 外挂处理程序
        /// 着力于解决非常调度过程中额外的处理
        /// </summary>
        /// <param name="task"></param>
        /// <param name="args"></param>
        public abstract void EquipmentActionUpdatePlugin(ref Task task, params object[] args);

        /// <summary>
        /// 外挂处理程序
        /// 着力于解决反向从应用程序中获取箱子号
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        //public abstract List<BoxInfo_Received> GetAllBoxInfo_ReceivedFormTask(EquipmentAction action);
    }
}