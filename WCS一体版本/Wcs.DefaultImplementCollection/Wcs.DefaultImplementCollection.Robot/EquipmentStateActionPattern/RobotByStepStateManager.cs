using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    public class RobotByStepStateManager : NhAbstractStateManager
    {
        Random _random = new Random();
        static NLog.Logger m_logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化 <seealso cref="T:PickingAndUnloadingWithStepByStepStateManager"/> 类的新实例。
        /// </summary>
        /// <param name="equipmentAction">物理动作</param>
        /// <param name="device">关联设备</param>
        public RobotByStepStateManager(EquipmentAction equipmentAction, TaskableDevice device)
            : base(equipmentAction, device)
        {
        }
        protected override NLog.Logger _logger
        {
            get { return m_logger; }
        }

        /// <summary>
        /// 创建状态链
        /// </summary>
        /// <remarks>
        /// <para>1、<see cref="T:NotArriveToPickingLocationState"/>未到达取货点</para>
        /// <para>2、<see cref="T:NotPickingState"/>未取货</para>
        /// <para>3、<see cref="T:NotArriveToUnloadingLocationState"/>未到达放货点</para>
        /// <para>4、<see cref="T:NotUnloadingState"/>未放货</para>
        /// </remarks>
        protected override void BuildStateLink()
        {
            RobotWorkState step1 = new RobotWorkState(this);

            var stepList = new List<AbstractState>(new AbstractState[] { step1});

            this.StateLink = stepList.AsReadOnly();

            ((RobotDevice)this.Device).PublicFireTaskRunningEvent(new TaskRunningEventArgs(this.EquipmentAction.EquipmentTaskId));
        }
    }
}
