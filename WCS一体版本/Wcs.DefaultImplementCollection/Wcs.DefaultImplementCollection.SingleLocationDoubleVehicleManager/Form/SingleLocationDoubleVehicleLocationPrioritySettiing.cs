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
    public partial class SingleLocationDoubleVehicleLocationPrioritySettiing : System.Windows.Forms.Form
    {
        SingleLocationDoubleVehicleSubSystem _device;
        bool _singleStaionSetting = true;
        public SingleLocationDoubleVehicleLocationPrioritySettiing(SingleLocationDoubleVehicleSubSystem device, bool singleStaionSetting)
        {
            _device = device;
            InitializeComponent();
            _singleStaionSetting = singleStaionSetting;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void SingleLocationDoubleVehicleLocationSettiing_Load(object sender, EventArgs e)
        {
            if (_singleStaionSetting)
            {
                var _stations = _device.Config.Stations.Priority.Single.Split(',').Where(x => !(string.IsNullOrWhiteSpace(x)));
                var stations = _device.Locations.Where(x => ((SingleLocationDoubleVehicleSubSystemLocation)x).StationNo > 0 && !_stations.Contains(x.UserCode) && !_stations.Contains(x.UserCode)).Select(x => x.UserCode);

                if (_stations.Count() > 0)
                    lv_highPriority.Items.AddRange(_stations.Select(x => new ListViewItem(x)).ToArray());
                if (stations.Count() > 0)
                    lv_normal.Items.AddRange(stations.Select(x => new ListViewItem(x)).ToArray());
            }
            else
            {
                var _groups = _device.Config.Stations.Priority.Groups.Split(',').Where(x => !(string.IsNullOrWhiteSpace(x)));
                var groups = _device.Config.Stations.Groups.Select(x => x.Name).Where(x => !(string.IsNullOrWhiteSpace(x)) && !_groups.Contains(x));

                if (_groups.Count() > 0)
                    lv_highPriority.Items.AddRange(_groups.Select(x => new ListViewItem(x)).ToArray());
                if (groups.Count() > 0)
                    lv_normal.Items.AddRange(groups.Select(x => new ListViewItem(x)).ToArray());
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_normal.Items != null && lv_normal.Items.Count > 0)
                {
                    var item = lv_normal.Items[0];
                    lv_normal.Items.Remove(item);
                    lv_highPriority.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_normal.SelectedItems != null && lv_normal.SelectedItems.Count != 0)
                {
                    var item = lv_normal.SelectedItems[0];
                    lv_normal.Items.Remove(item);
                    lv_highPriority.Items.Add(item);
                }
            }
            catch (Exception)
            {

            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_highPriority.SelectedItems != null && lv_highPriority.SelectedItems.Count != 0)
                {
                    var item = lv_highPriority.SelectedItems[0];
                    lv_highPriority.Items.Remove(item);
                    lv_normal.Items.Add(item);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                while (lv_highPriority.Items != null && lv_highPriority.Items.Count > 0)
                {
                    var item = lv_highPriority.Items[0];
                    lv_highPriority.Items.Remove(item);
                    lv_normal.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> _groups = new List<string>();
                List<string> _stations = new List<string>();
                if (lv_highPriority.Items != null && lv_highPriority.Items.Count != 0)
                {
                    foreach (ListViewItem item in lv_highPriority.Items)
                    {
                        if (_singleStaionSetting)
                            _stations.Add(item.Text);
                        else
                            _groups.Add(item.Text);
                    }
                }

                if (_singleStaionSetting)
                    _device.Config.Stations.Priority.Single = string.Join(",", _stations);
                else
                    _device.Config.Stations.Priority.Groups = string.Join(",", _groups);

                _device.Config.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"单个站点站点/设置失败，异常消息:{ex}");
            }
            this.Dispose();
            this.Close();
        }
    }
}
