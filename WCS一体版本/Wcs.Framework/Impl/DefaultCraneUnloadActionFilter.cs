using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Xml;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的堆垛机卸货动作过滤器<br />
    /// 用来在放货的目标点有货时不再执行
    /// </summary>
    public class DefaultCraneUnloadActionFilter : EquipmentSequenceActionFilter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filterNode">配置节点</param>
        public DefaultCraneUnloadActionFilter(XmlNode filterNode) : base(filterNode) { }
        /// <summary>
        /// 判断指定的动作是否可以发送
        /// </summary>
        /// <param name="action">要发送的动作</param>
        /// <returns>返回 true 表示动作可以发送；false 表示动作不可以发送。</returns>
        public override bool CanSend(EquipmentAction action)
        {
            //如果不是堆垛机动作，允许执行
            if (action.DeviceInfo.DeviceType != Devices.DeviceType.Crane)
            {
                return true;
            }

#warning 此处强制只处理了全自动的堆垛机取放货动作，并且只处了交互点是输送线位置的任务
            //如果不是全自动堆垛机放货任务，允许执行
            if (!(action is EquipmentActions.CraneAutomaticTransferAction))
            {
                return true;
            }


            var craneAutomaticTransferAction = action as EquipmentActions.CraneAutomaticTransferAction;

            var unloadLocation = craneAutomaticTransferAction.GetUnloadLocation();
            var occupiedSignalLocation = Cfg.Configuration
                .Locations
                .SingleOrDefault(location =>
                    location is ConveyorLocation && location.SameAs!=null && location.SameAs.Length>0 && location.Equals(unloadLocation)
                    ) as ConveyorLocation;
            if (occupiedSignalLocation == null)
            {
                //如果未找到对应的输送线位置信息，说明是入库，不再处理
                return true;
            }

            NewConveyor conveyor = (NewConveyor)occupiedSignalLocation.Device;
            if (conveyor.OccupiedSignals == null)
            {
                return false;
            }
            
            //输送线货位上的占位握手为‘空’
            return conveyor
                .OccupiedSignals
                .Any(x =>
                    x.PosNo == Convert.ToUInt16(occupiedSignalLocation.DeviceCode)
                    && x.HandShake == OccupiedSignalHandShake.Empty);
        }
    }
}
