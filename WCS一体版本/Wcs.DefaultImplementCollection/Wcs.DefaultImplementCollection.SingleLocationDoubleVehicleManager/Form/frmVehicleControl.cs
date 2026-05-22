using NLog;
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
    public partial class frmVehicleControl : UserControl
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        SingleLocationDoubleVehicleSubSystem _device;
        string _deviceName;
        public frmVehicleControl()
        {
            this._deviceName = "1号穿梭车";
            InitializeComponent();
        }
        public frmVehicleControl(SingleLocationDoubleVehicleSubSystem device, string deviceName)
        {
            _device = device;
            this._deviceName = deviceName;
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                _device.SetAbleWork(_deviceName);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                _device.SetStopWork(_deviceName);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                _device.SetRepair(_deviceName);
        }

        private void frmVehicleControl_Load(object sender, EventArgs e)
        {
            if (this.DesignMode)
                return;

            lablDeviceName.Text = _deviceName;
            switch (_device.GetVehicleState(_deviceName))
            {
                case RailGuidVehicleSetStatus.Working:
                    radioButton1.Checked = true;
                    break;
                case RailGuidVehicleSetStatus.Stoping:
                    radioButton2.Checked = true;
                    break;
                case RailGuidVehicleSetStatus.Repairing:
                    radioButton3.Checked = true;
                    break;
                case RailGuidVehicleSetStatus.Unknow:
                default:
                    radioButton1.Checked = false;
                    radioButton2.Checked = false;
                    radioButton3.Checked = false;
                    break;
            }

            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            radioButton3.CheckedChanged += radioButton3_CheckedChanged;

            if (radioButton2.Checked || radioButton3.Checked)
                linkLabel1.Enabled = true;
            else
                linkLabel1.Enabled = false;

            if (_device.Config.Vehicles.ConvertList.Contains(_deviceName))
                checkBox1.Checked = true;

            label2.Text = _device.Config.Vehicles.AverageTotalDistance.ToString();

            this.checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                _device.ResetBindingTask(this._deviceName);
                _logger.Info1("手动重置车辆绑定的任务成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show($"手动重置车辆绑定的任务失败，异常消息{ex}");
            }
        }

        private void radioButton2_CheckedChanged_1(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
                linkLabel1.Enabled = true;
        }

        private void radioButton3_CheckedChanged_1(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
                linkLabel1.Enabled = true;
        }

        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                linkLabel1.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _device.SetConvert(_deviceName, checkBox1.Checked);
            if (checkBox1.Checked)
                label2.Text = _device.Config.Vehicles.AverageTotalDistance.ToString();
            else
                label2.Text = "0";
        }
    }
}
