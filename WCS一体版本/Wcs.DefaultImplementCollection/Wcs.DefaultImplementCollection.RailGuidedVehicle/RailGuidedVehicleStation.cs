using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class RailGuidedVehicleStation:Location
    {
        /// <summary>
        /// 取货链条动作
        /// </summary>
        public ChainAction PickingChainAction{get;private set;}
        /// <summary>
        /// 放货链接动作
        /// </summary>
        public ChainAction PuttingChainAction { get; private set; }
        /// <summary>
        /// 所在位置
        /// </summary>
        public Int32 Position { get; private set; }

        /// <summary>
        /// 是否过滤参与调度转换条码值计算（默认false 参与计算 配置true不参与计算）
        /// </summary>
        public bool IsFilterConvert { get; private set; }
        /// <summary>
        /// 存货位置（可转换编码集合）。这些位置如果都未空闲时将无法再向该站点输送货物。
        /// </summary>
        public String[] StockingLocations { get; private set; }

        /// <summary>
        /// 站点位置
        /// </summary>
        public UInt16 StationNo { get; private set; }
        public RailGuidedVehicleStation(ChainAction pickingChainAction, ChainAction puttingChainAction, UInt16 stationNo,Int32 position, String[] stockingLocations,string deviceCode, string userCode,bool isFilterConvert, RailGuidedVehicleDevice device)
            : base(deviceCode, userCode, device)
        {
            //if (!(this is ILocationWildcard))
            //{
            //    if (stockingLocations == null || stockingLocations.Length == 0)
            //    {
            //        throw new ArgumentNullException("stockingLocations", "存货位置不能为空。");
            //    }
            //}

            this.PickingChainAction = pickingChainAction;
            this.PuttingChainAction = puttingChainAction;
            this.StationNo = stationNo;
            this.Position = position;
            this.StockingLocations = stockingLocations;
            this.IsFilterConvert = isFilterConvert;
        }

        /// <summary>
        /// 获取此站点于指定站点之前的距离
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public Int32 GetDistance(RailGuidedVehicleStation station)
        {
            return Math.Abs(station.Position - this.Position);
        }

        /// <summary>
        /// 计算从此站点行走到取货站点，然后再从取货站点行走到放货站点的总距离
        /// </summary>
        /// <param name="loadStation">取货站点</param>
        /// <param name="unloadStation">放货站点</param>
        /// <returns></returns>
        public Int32 GetDistance(RailGuidedVehicleStation loadStation, RailGuidedVehicleStation unloadStation)
        {
            return Math.Abs(loadStation.Position - this.Position)
                + Math.Abs(loadStation.Position - unloadStation.Position);
        }
    }
}
