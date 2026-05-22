using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public partial class SingleLocationDoubleVehicleLocationSettiing : System.Windows.Forms.Form
    {
        SingleLocationDoubleVehicleSubSystem _device;
        public SingleLocationDoubleVehicleLocationSettiing(SingleLocationDoubleVehicleSubSystem device, string text)
        {
            _device = device;
            InitializeComponent();
            label3.Text = text;
            if (text == "车辆列表")
            {
                btn_deleteGroup.Visible = false;
                btn_addGroup.Visible = false;
                bindingGroups = new Dictionary<string, List<BindingGroup>>();
            }
            else
            {
                stationGroups = new Dictionary<string, string>();
                deleteStationGroupList = new List<string>();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void SingleLocationDoubleVehicleLocationSettiing_Load(object sender, EventArgs e)
        {
            if (label3.Text == "车辆列表")
                lv_group.Items.AddRange(_device.RailGuidedVehicleDeviceNames.Select(x => new ListViewItem(x)).ToArray());
            else
                lv_group.Items.AddRange(_device.Config.Stations.Groups.Select(x => new ListViewItem(x.Name)).ToArray());
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_stations.Items != null && lv_stations.Items.Count > 0)
                {
                    var item = lv_stations.Items[0];
                    lv_stations.Items.Remove(item);
                    lv_select.Items.Add(item);
                }
                SetConfig();
            }
            catch (Exception ex)
            {
            }
        }

        Dictionary<string, List<BindingGroup>> bindingGroups = null;
        Dictionary<string, string> stationGroups = null;
        List<string> deleteStationGroupList = null;
        private void lv_group_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listView = (ListView)sender;
            if (listView.Name == "lv_group")
            {
                if (lv_group.SelectedItems == null)
                {
                    lv_select.Items.Clear();
                    return;
                }
                lv_select.Items.Clear();
                lv_stations.Items.Clear();
                var item = lv_group.SelectedItems[0];
                IEnumerable<string> selects = new List<string>();
                if (label3.Text == "车辆列表")
                    selects = _device.Config.Bindings.Groups.Where(x => x.Vehicle == item.Text).Select(x => x.Station);
                else
                {
                    var group = _device.Config.Stations.Groups.FirstOrDefault(x => x.Name == item.Text);
                    if (group != null)
                        selects = group.List.Split(',').Where(x => !string.IsNullOrWhiteSpace(x));
                }
                if (selects != null && selects.Count() > 0)
                    lv_select.Items.AddRange(selects.Select(x => new ListViewItem(x)).ToArray());

                var stations = _device.Locations.Where(x => ((SingleLocationDoubleVehicleSubSystemLocation)x).StationNo > 0 && !selects.Contains(x.UserCode)).Select(x => x.UserCode);
                lv_stations.Items.AddRange(stations.Select(x => new ListViewItem(x)).ToArray());
            }
        }

        private void SetConfig()
        {
            var group = lv_group.SelectedItems[0].Text;
            if (label3.Text == "车辆列表")
            {
                List<BindingGroup> list = new List<BindingGroup>();
                foreach (ListViewItem item in lv_select.Items)
                {
                    list.Add(new BindingGroup() { Station = item.Text, Vehicle = group });
                }
                if (!bindingGroups.ContainsKey(lv_group.SelectedItems[0].Text))
                    bindingGroups.Add(group, list);
                else
                    bindingGroups[group] = list;
            }
            else
            {
                List<string> list = new List<string>();
                foreach (ListViewItem item in lv_select.Items)
                {
                    list.Add(item.Text);
                }
                if (!stationGroups.ContainsKey(lv_group.SelectedItems[0].Text))
                    stationGroups.Add(group, string.Join(",", list));
                else
                    stationGroups[group] = string.Join(",", list);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_stations.SelectedItems != null && lv_stations.SelectedItems.Count != 0)
                {
                    var item = lv_stations.SelectedItems[0];
                    lv_stations.Items.Remove(item);
                    lv_select.Items.Add(item);
                }
                SetConfig();
            }
            catch (Exception ex)
            {

            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_select.SelectedItems != null && lv_select.SelectedItems.Count != 0)
                {
                    var item = lv_select.SelectedItems[0];
                    lv_select.Items.Remove(item);
                    lv_stations.Items.Add(item);
                }
                SetConfig();
            }
            catch (Exception)
            {

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_select.Items != null && lv_select.Items.Count > 0)
                {
                    var item = lv_select.Items[0];
                    lv_select.Items.Remove(item);
                    lv_stations.Items.Add(item);
                }
                SetConfig();
            }
            catch (Exception ex)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (label3.Text == "车辆列表")
                {
                    if (bindingGroups.Count() == 0)
                        MessageBox.Show($"本次未修改站点绑定车辆设置");
                    else
                    {
                        foreach (var item in bindingGroups)
                        {
                            _device.Config.Bindings.Groups.RemoveAll(x => x.Vehicle == item.Key);
                            if (item.Value.Count() > 0)
                                _device.Config.Bindings.Groups.AddRange(item.Value);
                        }
                    }
                }
                else
                {
                    if (stationGroups.Count() == 0 && deleteStationGroupList.Count() == 0)
                        MessageBox.Show($"本次未修改站点分组设置");
                    else
                    {
                        foreach (var item in stationGroups)
                        {
                            if (_device.Config.Stations.Groups.Any(x => x.Name == item.Key))
                            {
                                _device.Config.Stations.Groups.RemoveAll(x => x.Name == item.Key);
                                _device.Config.Stations.Groups.Add(new StationGroup() { Name = item.Key, List = string.Join(",", item.Value) });
                            }
                            else
                                _device.Config.Stations.Groups.Add(new StationGroup() { Name = item.Key, List = string.Join(",", item.Value) });
                        }
                        if (deleteStationGroupList.Count() > 0)
                        {
                            _device.Config.Stations.Groups.RemoveAll(x => deleteStationGroupList.Contains(x.Name));
                            var list = _device.Config.Stations.Priority.Groups.Split(',').ToList();
                            list = list.Where(x => !deleteStationGroupList.Contains(x)).ToList();
                            _device.Config.Stations.Priority.Groups = string.Join(",", list);
                        }
                    }
                }
                _device.Config.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置失败，异常消息:{ex}");
            }
            this.Dispose();
            this.Close();
        }

        private void btn_addGroup_Click(object sender, EventArgs e)
        {
            try
            {
                AddNewStationGroup frm = new AddNewStationGroup();
                var newStationGroupName = frm.GetNewGroupName();

                if (string.IsNullOrWhiteSpace(newStationGroupName))
                    MessageBox.Show("新增站点分组名称不可为空，请尝试重新增加！");
                else
                {
                    foreach (ListViewItem item in lv_group.Items)
                    {
                        if (item.Text == newStationGroupName)
                        {
                            MessageBox.Show("新增站点分组名称已存在不可重复添加！");
                            return;
                        }
                    }
                    lv_group.Items.Add(new ListViewItem(newStationGroupName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"新增站点分组时发生异常，异常信息：{ex}");
            }
        }

        private void btn_deleteGroup_Click(object sender, EventArgs e)
        {
            try
            {
                if (lv_group.SelectedItems == null || lv_group.SelectedItems.Count == 0)
                {
                    MessageBox.Show("未选中需要删除的站点分组，请选择后尝试删除");
                    return;
                }
                var group = lv_group.SelectedItems[0].Text;
                if (!deleteStationGroupList.Contains(group))
                    deleteStationGroupList.Add(group);
                if (stationGroups.ContainsKey(group))
                    stationGroups.Remove(group);

                lv_group.Items.Remove(lv_group.SelectedItems[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除站点分组时发生异常，异常信息：{ex}");
            }
        }
    }
}
