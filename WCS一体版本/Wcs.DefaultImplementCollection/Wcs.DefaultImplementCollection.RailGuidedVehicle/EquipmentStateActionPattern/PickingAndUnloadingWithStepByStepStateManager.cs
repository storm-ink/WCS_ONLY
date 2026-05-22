using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public sealed class PickingAndUnloadingWithStepByStepStateManager : NhAbstractStateManager
    {
        static NLog.Logger m_logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化 <seealso cref="T:PickingAndUnloadingWithStepByStepStateManager"/> 类的新实例。
        /// </summary>
        /// <param name="equipmentAction">物理动作</param>
        /// <param name="device">关联设备</param>
        public PickingAndUnloadingWithStepByStepStateManager(EquipmentAction equipmentAction, TaskableDevice device)
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
            NotArriveToPickingLocationState step1;
            NotPickingState step2;
            NotArriveToUnloadingLocationState step3;
            NotUnloadingState step4;
            List<AbstractState> stepList;
            var _start = LocationConverter.ToLocation(this.EquipmentAction.Movement.StartLocation);
            if (_start is RailGuidedVehicleStationWildcard)
            {
                step1 = new NotArriveToPickingLocationState(this);
                step2 = new NotPickingState(this);
                stepList = new List<AbstractState>(new AbstractState[] { step1, step2 });
                this.StateLink = stepList.AsReadOnly();
                return;
            }
            var _end = LocationConverter.ToLocation(this.EquipmentAction.Movement.EndLocation);
            if (_end is RailGuidedVehicleStationWildcard)
            {
                step3 = new NotArriveToUnloadingLocationState(this);
                step4 = new NotUnloadingState(this);
                stepList = new List<AbstractState>(new AbstractState[] { step3, step4 });
                this.StateLink = stepList.AsReadOnly();
                return;
            }

            step1 = new NotArriveToPickingLocationState(this);
            step2 = new NotPickingState(this);
            step3 = new NotArriveToUnloadingLocationState(this);
            step4 = new NotUnloadingState(this);
            stepList = new List<AbstractState>(new AbstractState[] { step1, step2, step3, step4 });

            this.StateLink = stepList.AsReadOnly();
        }
    }
}
