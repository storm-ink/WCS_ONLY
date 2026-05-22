using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImpls.SingleLocationDoubleVehicleManager;
using Wcs.Framework;

namespace BOE.PreTaskHand
{
    public class Between1004AndC004VsBetween2018AndC001VsVechileIsDoublePreTaskSchedulerFilter : Wcs.Framework.PreTaskSchedulerFilter
    {
        KeyValuePair<string, string> kv1 = new KeyValuePair<string, string>("1004", "C004");
        KeyValuePair<string, string> kv2 = new KeyValuePair<string, string>("2018", "C001");
        KeyValuePair<string, string> kv3 = new KeyValuePair<string, string>("C004", "1001");
        KeyValuePair<string, string> kv4 = new KeyValuePair<string, string>("C001", "2015");
        KeyValuePair<string, string> kv5 = new KeyValuePair<string, string>("C001", "C004");
        public override ActionSchedulerFilterResult Filter(PreTask preTask)
        {
            //35	输送线1	1025,1033 入C004垛机
            //36  输送线1    1033,1025 出C004垛机
            //还需增加相应的路径选择器
            if (kv1.Key == preTask.StartLocation.DeviceCode && kv1.Value == preTask.EndLocation.DeviceName)
            {
                var deviceName = "A区一轨双车";
                var device = DeviceConverter.ToDevice<SingleLocationDoubleVehicleSubSystem>(deviceName);
                if (device.RailGuidedVehicleDeviceNames.Count > 1 && device.RailGuidedVehicleDeviceNames.Count - (device.Config.Vehicles.RepairList.Length) > 1)
                {
                    var routeId = 35;
                    if (RouteHelper.DisableRouteIds.Contains(routeId))
                        return new ActionSchedulerFilterResult(true, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，但是当前35号路径处于禁用状态，当前预任务无法下发");
                    else
                        return new ActionSchedulerFilterResult(false, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，当前35号路径处于禁用状态，当前预任务可以下发");
                }
                else
                    return new ActionSchedulerFilterResult(false, "");
            }
            //63	输送线2	2020,2028 入C001垛机
            //64  输送线2    2028,2020 出C001垛机
            //还需增加路径选择器
            else if (kv2.Key == preTask.StartLocation.DeviceCode && kv2.Value == preTask.EndLocation.DeviceName)
            {
                var deviceName = "B区一轨双车";
                var device = DeviceConverter.ToDevice<SingleLocationDoubleVehicleSubSystem>(deviceName);
                if (device.RailGuidedVehicleDeviceNames.Count > 1 && device.RailGuidedVehicleDeviceNames.Count - (device.Config.Vehicles.RepairList.Length) > 1)
                {
                    var routeId = 63;
                    if (RouteHelper.DisableRouteIds.Contains(routeId))
                        return new ActionSchedulerFilterResult(true, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，但是当前35号路径处于禁用状态，当前预任务无法下发");
                    else
                        return new ActionSchedulerFilterResult(false, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，当前35号路径处于禁用状态，当前预任务可以下发");
                }
                else
                    return new ActionSchedulerFilterResult(false, "");
            }
            //35	输送线1	1025,1033 入C004垛机
            //36  输送线1    1033,1025 出C004垛机
            //还需增加相应的路径选择器
            else if (kv3.Key == preTask.StartLocation.DeviceName && kv3.Value == preTask.EndLocation.DeviceCode)
            {
                var deviceName = "A区一轨双车";
                var device = DeviceConverter.ToDevice<SingleLocationDoubleVehicleSubSystem>(deviceName);
                if (device.RailGuidedVehicleDeviceNames.Count > 1 && device.RailGuidedVehicleDeviceNames.Count - (device.Config.Vehicles.RepairList.Length) > 1)
                {
                    var routeId = 36;
                    if (RouteHelper.DisableRouteIds.Contains(routeId))
                        return new ActionSchedulerFilterResult(true, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，但是当前35号路径处于禁用状态，当前预任务无法下发");
                    else
                        return new ActionSchedulerFilterResult(false, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，当前35号路径处于禁用状态，当前预任务可以下发");
                }
                else
                    return new ActionSchedulerFilterResult(false, "");
            }
            //63	输送线2	2020,2028 入C001垛机
            //64  输送线2    2028,2020 出C001垛机
            //还需增加路径选择器
            else if (kv4.Key == preTask.StartLocation.DeviceName && kv4.Value == preTask.EndLocation.DeviceCode)
            {
                var deviceName = "B区一轨双车";
                var device = DeviceConverter.ToDevice<SingleLocationDoubleVehicleSubSystem>(deviceName);
                if (device.RailGuidedVehicleDeviceNames.Count > 1 && device.RailGuidedVehicleDeviceNames.Count - (device.Config.Vehicles.RepairList.Length) > 1)
                {
                    var routeId = 64;
                    if (RouteHelper.DisableRouteIds.Contains(routeId))
                        return new ActionSchedulerFilterResult(true, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，但是当前35号路径处于禁用状态，当前预任务无法下发");
                    else
                        return new ActionSchedulerFilterResult(false, $"从{kv1.Key}入库至{kv1.Value}必须从{routeId}号路径，当前35号路径处于禁用状态，当前预任务可以下发");
                }
                else
                    return new ActionSchedulerFilterResult(false, "");
            }
            //方案图左侧
            //23   输送线1   1019,1027 入C001垛机
            //24   输送线1   1027,1019 出C001垛机
            //25   输送线1   1020,1028 入C001垛机
            //26   输送线1   1028,1020 出C001垛机
            //35   输送线1   1025,1033 入C004垛机
            //36   输送线1   1033,1025 出C004垛机
            //37   输送线1   1026,1034 入C004垛机
            //38   输送线1   1034,1026 出C004垛机
            //方案图右侧
            //61	输送线2	2019,2027 入C001垛机 
            //62	输送线2	2027,2019 出C001垛机
            //63	输送线2	2020,2028 入C001垛机
            //64	输送线2	2028,2020 出C001垛机
            //73	输送线2	2025,2033 入C004垛机
            //74	输送线2	2033,2025 出C004垛机
            //75	输送线2	2026,2034 入C004垛机
            //76	输送线2	2034,2026 出C004垛机
            //还需增加相应的路径选择器
            else if (kv5.Key == preTask.StartLocation.DeviceName && kv5.Value == preTask.EndLocation.DeviceName)
            {

            }
            //还需增加相应的路径选择器
            else if (kv5.Key == preTask.EndLocation.DeviceName && kv5.Value == preTask.StartLocation.DeviceName)
            {

            }
            else
                return new ActionSchedulerFilterResult(false, "");
        }
    }
}
