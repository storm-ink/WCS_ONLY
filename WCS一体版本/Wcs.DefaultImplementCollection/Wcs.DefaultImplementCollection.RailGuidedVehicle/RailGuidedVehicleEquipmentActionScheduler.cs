using System;
using System.Collections.Generic;
using System.Linq; using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public sealed class RailGuidedVehicleEquipmentActionScheduler:Wcs.Framework.EquipmentActionScheduler
    {
        protected RailGuidedVehicleEquipmentActionScheduler() : base() { }
        public RailGuidedVehicleEquipmentActionScheduler(TaskableDevice device, EquipmentActionSchedulerFilter[] actionFilters)
            : base(device, actionFilters){ }

        const String _任务模式配置名称 = "穿梭车任务模式";
        protected override IOrderedEnumerable<EquipmentAction> GetAvailableActions()
        {
            var actions = base.GetAvailableActions();

            if (actions == null)
            {
                return actions;
            }

            var workMode = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<RailGuidedVehicleWrokMode>(_任务模式配置名称, RailGuidedVehicleWrokMode.Normal);
            var outboundFirst = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("出库优先");
            var currentStation = ((RailGuidedVehicleDevice)this.Device).GetCurrentStation();
            //重置排序
            if (outboundFirst)
            {
                if (workMode == RailGuidedVehicleWrokMode.Nearby && currentStation!=null)
                {
                    actions = actions
                      .OrderByDescending(x => x.Movement.Task.Priority)
                      .ThenBy(x => x.Movement.Task.Direction == TaskDirection.Out ? 0 : 1)
                      .ThenBy(x=>getDistance(currentStation,x))
                      .ThenBy(x => x.CreatedAt)
                      .ThenBy(x => x.SequenceOrdering)
                      .ThenBy(x => x.Id);
                }
                else
                {
                    actions = actions
                      .OrderByDescending(x => x.Movement.Task.Priority)
                      .ThenBy(x => x.Movement.Task.Direction == TaskDirection.Out ? 0 : 1)
                      .ThenBy(x => x.CreatedAt) 
                      .ThenBy(x => x.SequenceOrdering)
                      .ThenBy(x => x.Id);
                }
            }
            else
            {
                if (workMode == RailGuidedVehicleWrokMode.Nearby && currentStation != null)
                {
                    actions = actions
                      .OrderByDescending(x => x.Movement.Task.Priority)
                      .ThenBy(x => getDistance(currentStation, x))
                      .ThenBy(x => x.CreatedAt)
                      .ThenBy(x => x.SequenceOrdering)
                      .ThenBy(x => x.Id);
                }
                else
                {
                    actions = actions
                      .OrderByDescending(x => x.Movement.Task.Priority)
                      .ThenBy(x => x.CreatedAt)
                      .ThenBy(x => x.SequenceOrdering)
                      .ThenBy(x => x.Id);
                }
            }

            return actions;
        }

        Int32 getDistance(RailGuidedVehicleStation currentStation, EquipmentAction action)
        {
            var startLocation = LocationConverter.ToLocation(action.Movement.StartLocation);
            var startStation = (RailGuidedVehicleStation)startLocation.Synonymous.FirstOrDefault(x => x is RailGuidedVehicleStation);

            if (startStation == null)
            {
                _logger.Warn1(string.Format("{0} 未关联同义站点", startLocation), this);

                return 99;
            }

            return currentStation.GetDistance(startStation);
        }
    }

    //public sealed class RailGuidedVehicleDeviceSetttingPlugin : WcsPlugin
    //{
    //    const String _任务模式配置名称 = "穿梭车任务模式";

    //    public override bool Initialization(WcsContext context)
    //    {
    //        var rgvDevices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
    //            .Where(x => x.Device is RailGuidedVehicleDevice)
    //            .Select(x => x.Device as RailGuidedVehicleDevice);

    //        if (rgvDevices.Count() > 0)
    //        {
    //            var currentMode = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<RailGuidedVehicleWrokMode>(_任务模式配置名称, RailGuidedVehicleWrokMode.Normal);
    //            ToolStripMenuItem menu = new ToolStripMenuItem("穿梭车接货模式");
    //            foreach (var item in Wcs.EnumExtentions.ToKeyValueList<RailGuidedVehicleWrokMode>())
    //            {
    //                var v = (RailGuidedVehicleWrokMode)Enum.Parse(typeof(RailGuidedVehicleWrokMode), item.Key);
    //                var mi = new ToolStripMenuItem(v.GetDescription());
    //                if (currentMode == v)
    //                {
    //                    mi.Checked = true;
    //                }
    //                mi.Tag = v;
    //                mi.Click += mi_Click;
    //                menu.DropDownItems.Add(mi);
    //            }

    //            context.Application.GetMenu(WcsApplicationMenuType.Edit).DropDownItems.Add(menu);
    //        }

    //        return base.Initialization(context);

    //    }

    //    void mi_Click(object sender, EventArgs e)
    //    {
    //        var mi = (ToolStripMenuItem)sender;
    //        var newValue = (RailGuidedVehicleWrokMode)mi.Tag;
    //        Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting(_任务模式配置名称, newValue);

    //        var pmi = (ToolStripMenuItem)mi.OwnerItem;
    //        foreach (ToolStripMenuItem item in pmi.DropDownItems)
    //        {
    //            item.Checked = false;
    //        }

    //        mi.Checked = true;
    //    }
    //}
}
