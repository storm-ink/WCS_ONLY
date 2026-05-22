using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 单货位一轨双车帮助程序
    /// 配置文件格式：SingleLocationDoubleVehicleSubSystemConfig
    /// 路径:.\系统配置\穿梭车\<调度系统名>设置.xml
    /// </summary>
    public static class SingleLocationDoubleVehicleHelper
    {

        /// <summary>
        /// 获取设置车辆状态
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static RailGuidVehicleSetStatus GetVehicleState(this SingleLocationDoubleVehicleSubSystem subSystem, string deviceName)
        {
            if (subSystem.Config.Vehicles.AbleList.Contains(deviceName))
                return RailGuidVehicleSetStatus.Working;
            else if (subSystem.Config.Vehicles.StopList.Contains(deviceName))
                return RailGuidVehicleSetStatus.Stoping;
            else if (subSystem.Config.Vehicles.RepairList.Contains(deviceName))
                return RailGuidVehicleSetStatus.Repairing;
            else
                return RailGuidVehicleSetStatus.Unknow;
        }

        /// <summary>
        /// 设置启用
        /// </summary>
        /// <param name="deviceName"></param>
        public static void SetAbleWork(this SingleLocationDoubleVehicleSubSystem subSystem, string deviceName)
        {
            if (subSystem.Config.Vehicles.StopList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.StopList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.StopList = string.Join(",", list);
            }

            if (subSystem.Config.Vehicles.RepairList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.RepairList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.RepairList = string.Join(",", list);
            }

            if (!subSystem.Config.Vehicles.AbleList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.AbleList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Add(deviceName);
                subSystem.Config.Vehicles.AbleList = string.Join(",", list);
            }
            subSystem.Config.Save();
        }

        /// <summary>
        /// 设置停用
        /// </summary>
        /// <param name="deviceName"></param>
        public static void SetStopWork(this SingleLocationDoubleVehicleSubSystem subSystem, string deviceName)
        {
            if (subSystem.Config.Vehicles.AbleList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.AbleList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.AbleList = string.Join(",", list);
            }

            if (subSystem.Config.Vehicles.RepairList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.RepairList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.RepairList = string.Join(",", list);
            }

            if (!subSystem.Config.Vehicles.StopList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.StopList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Add(deviceName);
                subSystem.Config.Vehicles.StopList = string.Join(",", list);
            }
            subSystem.Config.Save();
        }

        /// <summary>
        /// 设置维修
        /// </summary>
        /// <param name="deviceName"></param>
        public static void SetRepair(this SingleLocationDoubleVehicleSubSystem subSystem, string deviceName)
        {
            if (subSystem.Config.Vehicles.AbleList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.AbleList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.AbleList = string.Join(",", list);
            }

            if (subSystem.Config.Vehicles.StopList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.StopList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Remove(deviceName);
                subSystem.Config.Vehicles.StopList = string.Join(",", list);
            }

            if (!subSystem.Config.Vehicles.RepairList.Contains(deviceName))
            {
                var list = subSystem.Config.Vehicles.RepairList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                list.Add(deviceName);
                subSystem.Config.Vehicles.RepairList = string.Join(",", list);
            }
            subSystem.Config.Save();
        }

        /// <summary>
        /// 设置维修
        /// </summary>
        /// <param name="deviceName"></param>
        public static void SetConvert(this SingleLocationDoubleVehicleSubSystem subSystem, string deviceName, Boolean convert)
        {
            if (convert)
            {
                if (!subSystem.Config.Vehicles.ConvertList.Contains(deviceName))
                {
                    var list = subSystem.Config.Vehicles.ConvertList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    list.Add(deviceName);
                    subSystem.Config.Vehicles.ConvertList = string.Join(",", list);
                }

                int sum = 0;
                int stationCount = 0;
                foreach (var item in subSystem.RailGuidedVehicles)
                {
                    var locations = item.Locations.Where(x => !(x is RailGuidedVehicleStationWildcard) && !((RailGuidedVehicleStation)x).IsFilterConvert).Select(x => ((RailGuidedVehicleStation)x).Position);
                    foreach (var _item in locations)
                    {
                        sum += _item;
                    }
                    stationCount += locations.Count();
                }

                subSystem.Config.Vehicles.AverageTotalDistance = (int)Math.Round((decimal)sum / (decimal)(stationCount / 2));
            }
            else
            {
                if (subSystem.Config.Vehicles.ConvertList.Contains(deviceName))
                {
                    var list = subSystem.Config.Vehicles.AbleList.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    list.Remove(deviceName);
                    subSystem.Config.Vehicles.ConvertList = string.Join(",", list);
                }
                if (string.IsNullOrWhiteSpace(subSystem.Config.Vehicles.ConvertList))
                    subSystem.Config.Vehicles.AverageTotalDistance = 0;
            }
            subSystem.Config.Save();
        }
    }
}
