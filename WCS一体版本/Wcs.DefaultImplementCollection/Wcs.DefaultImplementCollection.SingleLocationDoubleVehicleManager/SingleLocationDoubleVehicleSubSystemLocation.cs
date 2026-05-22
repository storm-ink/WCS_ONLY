using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 单货位一轨双车位置
    /// </summary>
    public class SingleLocationDoubleVehicleSubSystemLocation : Location
    {
        /// <summary>
        /// 取货链条动作
        /// </summary>
        public ChainAction PickingChainAction { get; private set; }
        /// <summary>
        /// 放货链接动作
        /// </summary>
        public ChainAction PuttingChainAction { get; private set; }
        /// <summary>
        /// 所在位置
        /// </summary>
        public Int32 Position { get; private set; }

        /// <summary>
        /// 站点位置
        /// </summary>
        public UInt16 StationNo { get; private set; }
        /// <summary>
        /// 是否是设备
        /// </summary>
        public Boolean IsDevice { get; set; }
        public SingleLocationDoubleVehicleSubSystemLocation(ChainAction pickingChainAction, ChainAction puttingChainAction, UInt16 stationNo, Int32 position, string deviceCode, string userCode, SingleLocationDoubleVehicleSubSystem device, Boolean isDevice = false)
            : base(deviceCode, userCode, device)
        {
            this.PickingChainAction = pickingChainAction;
            this.PuttingChainAction = puttingChainAction;
            this.StationNo = stationNo;
            this.Position = position;
            this.IsDevice = isDevice;
        }
    }
}
