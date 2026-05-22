using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CraneTaskTaskTypeEquipmentActionSchedulerFilter : Wcs.Framework.EquipmentActionSchedulerFilter
    {
        /// <summary>
        /// 允许的任务类型
        /// </summary>
        public static Dictionary<String, String[]> AllowTaskTypes { get; private set; }

        public static void SetTypes(String craneDeviceName, String[] types)
        {
            AllowTaskTypes[craneDeviceName] = types;

            if (types != null)
            {
                Wcs.Framework.Cfg
                        .WcsConfiguration
                        .Instance
                        .SettingCollection
                        .SetSetting<String>("/堆垛机任务类型过滤器/" + craneDeviceName, String.Join(",", types));
            }
            else
            {
                Wcs.Framework.Cfg
                        .WcsConfiguration
                        .Instance
                        .SettingCollection
                        .SetSetting<String>("/堆垛机任务类型过滤器/" + craneDeviceName, "");
            }
        }

        public static String[] GetTypes(String craneDeviceName)
        {
            if (!AllowTaskTypes.ContainsKey(craneDeviceName))
            {
                return new string[0];
            }

            return AllowTaskTypes[craneDeviceName];
        }

        static CraneTaskTaskTypeEquipmentActionSchedulerFilter()
        {
            AllowTaskTypes = new Dictionary<string, string[]>();
            foreach (var craneDevice in Wcs.Framework
                .Cfg
                .WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Where(x => x.Device is CraneDevice)
                .Select(x => x.Device as CraneDevice)
                .OrderBy(x => x.No)
                )
            {
                var currentSetting = Wcs.Framework.Cfg
                    .WcsConfiguration
                    .Instance
                    .SettingCollection
                    .GetSetting<String>("/堆垛机任务类型过滤器/" + craneDevice.Name, "")
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                SetTypes(craneDevice.Name, currentSetting);
            }

        }

        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (action.Movement == null || action.Movement.Task == null)
            {
                return new ActionSchedulerFilterResult(false, "");
            }

            var taskTypes = GetTypes(action.DeviceName);
            if (taskTypes == null || taskTypes.Length == 0)
            {
                return new ActionSchedulerFilterResult(false, "");
            }


            if (taskTypes.Any(x => String.Equals(action.Movement.Task.TaskType, x, StringComparison.CurrentCultureIgnoreCase)))
            {
                return new ActionSchedulerFilterResult(false, "");
            }

            var actions = equipmentActionScheduler.Actions.Where(x => x.Movement != null && x.Movement.Task != null);
            if (actions
                .Any(act =>
                    taskTypes.Any(type =>
                    String.Equals(type, act.Movement.Task.TaskType, StringComparison.CurrentCultureIgnoreCase)
                    )
                  )
             )
            {
                return new ActionSchedulerFilterResult(true, "当前只允许类型为 “" + String.Join(",", AllowTaskTypes) + "” 的作业。");
            }

            return new ActionSchedulerFilterResult(false, "");
        }

    }
}
