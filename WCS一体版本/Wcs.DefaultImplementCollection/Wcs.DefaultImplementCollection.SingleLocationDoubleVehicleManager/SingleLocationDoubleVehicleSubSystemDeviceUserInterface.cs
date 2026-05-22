using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager;
using Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager.Form;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public partial class SingleLocationDoubleVehicleSubSystemDeviceUserInterface : System.Windows.Forms.Form, IDeviceUserInterface
    {
        Logger _logger = LogManager.CreateNullLogger();
        SingleLocationDoubleVehicleSubSystem _device;
        public SingleLocationDoubleVehicleSubSystemDeviceUserInterface()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public void Show(Device device)
        {
            _device = (SingleLocationDoubleVehicleSubSystem)device;
            this.Text = string.Format("查看 {0} 的状态", _device.Name);
            this.lblDeviceName.Text = _device.Name;
            Point point = new Point(6, 20);
            foreach (var item in _device.RailGuidedVehicleDeviceNames)
            {
                frmVehicleControl frmVehicleControl = new frmVehicleControl(_device, item);
                frmVehicleControl.Location = point;
                groupBox3.Controls.Add(frmVehicleControl);
                point = new Point(point.X, frmVehicleControl.Bottom + 3);
            }

            this.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lblLockIP.Text = _device.Locker.IPAddress;
                if (_device.Locker.IsEmpty)
                {
                    lblLockUser.Text = "";
                    btnLock.Enabled = true;
                    btnUnlock.Enabled = false;
                    groupBox3.Enabled = false;
                    //groupBox5.Enabled = false;
                    groupBox6.Enabled = false;
                }
                else
                {
                    btnLock.Enabled = false;
                    btnUnlock.Enabled = true;
                    lblLockUser.Text = _device.Locker.UserName;
                    groupBox3.Enabled = true;
                    //groupBox5.Enabled = true;
                    groupBox6.Enabled = true;
                }

                if (_device.Warnings == null || _device.Warnings.Length == 0)
                    tbxWarnings.Text = "";
                else
                    tbxWarnings.Text = String.Join("\r\n", _device.Warnings);

                if (_device.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler scheduler)
                {
                    string showMsg = scheduler.GetDispatchShowInfo();
                    if (showMsg != tbx_scheduler.Text)
                        tbx_scheduler.Text = showMsg;

                    if (scheduler.LastCalFreeVehicleResult != null)
                        tbx_freeCalResult.Text = string.Join("\r\n", scheduler.LastCalFreeVehicleResult);
                }
                else if (_device.EquipmentActionScheduler is SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 scheduler2)
                {
                    string showMsg = scheduler2.GetDispatchShowInfo();
                    if (showMsg != tbx_scheduler.Text)
                        tbx_scheduler.Text = showMsg;

                    if (scheduler2.LastCalFreeVehicleResult != null)
                        tbx_freeCalResult.Text = string.Join("\r\n", scheduler2.LastCalFreeVehicleResult);
                }

                long[] poss = new long[2];
                int i = 0;
                foreach (var item in _device.RailGuidedVehicles)
                {
                    if (item.LastStatus == null)
                    {
                        groupBox3.Text = "车辆列表(两车实时距离 0)";
                        return;
                    }

                    if (!_device.Config.Vehicles.ConvertList.Contains(item.Name))
                        poss[i++] = item.LastStatus.Position;
                    else
                        poss[i++] = _device.Config.Vehicles.AverageTotalDistance - item.LastStatus.Position;
                }
                groupBox3.Text = $"车辆列表(两车实时距离 {Math.Abs(poss[0] - poss[1])})";
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            try
            {
                _device.Lock(new LockerInfo(System.Environment.MachineName, LockerInfo.GetIpAddress()));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            string msg = String.Format("是否要强制清除 {0} {1} {2}\n\n设备锁在清除后有可能会立即运行\n请先确认所有人员已离开设备运行区域\n\n是否继续？"
                , _device.GetType().GetDisplayName()
                , _device.Name
                , _device.Locker);
            if (MessageBox.Show(msg, "强制清除设备锁定提示"
                , MessageBoxButtons.YesNo
                , MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            try
            {
                _device.Unlock(LockerInfo.Adminstrator);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        int oldSafeDistance;
        int oldSlowDistance;
        private void SingleLocationDoubleVehicleSubSystemDeviceUserInterface_Load(object sender, EventArgs e)
        {
            try
            {
                lblDeviceType.Text = _device.GetType().GetDisplayName();
                lblDeviceName.Text = _device.Name;
                lblLockIP.Text = _device.Locker.IPAddress;
                if (_device.Locker.IsEmpty)
                {
                    lblLockUser.Text = "";
                    btnLock.Enabled = true;
                    btnUnlock.Enabled = false;
                    groupBox3.Enabled = false;
                    groupBox5.Enabled = false;
                    groupBox6.Enabled = false;
                }
                else
                {
                    btnLock.Enabled = false;
                    btnUnlock.Enabled = true;
                    lblLockUser.Text = _device.Locker.UserName;
                    groupBox3.Enabled = true;
                    groupBox5.Enabled = true;
                    groupBox6.Enabled = true;
                }

                oldSafeDistance = _device.Config.SafeDistance;
                oldSlowDistance = _device.Config.SlowDistance;
                tbx_safeDistance.Text = _device.Config.SafeDistance.ToString();
                tbx_slowDistance.Text = _device.Config.SlowDistance.ToString();
            }
            catch
            {
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                SingleLocationDoubleVehicleLocationSettiing frm = new SingleLocationDoubleVehicleLocationSettiing(_device, "分组列表");
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show($"站点设置 - 优先级站点分组 设置失败，异常消息{ex}");
            }
        }

        private void btn_safeDistance_Click(object sender, EventArgs e)
        {
            try
            {
                int safeDistance;
                if (int.TryParse(tbx_safeDistance.Text, out safeDistance))
                {
                    var result = MessageBox.Show($"安全距离（单位：mm）即将由 {oldSafeDistance} 调整为 {safeDistance}，请确认！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        string msg;
                        if (oldSlowDistance < safeDistance)
                        {
                            msg = $"本次设置的安全距离（单位：mm） {safeDistance} 大于设置的原始的减速距离 {oldSlowDistance}，这种情况下减速距离在车辆让道计算中将默认无效，并可能存在撞车风险";
                            if (MessageBox.Show($"{msg}，请确认！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                return;
                            else
                                _logger.Info1($"{msg}，操作员已确认此操作。", this);
                        }
                        _device.Config.SafeDistance = safeDistance;
                        _device.Config.Save();
                        _logger.Info1($"安全距离被人工由 {oldSafeDistance} 调整为 {safeDistance}", this);
                        oldSafeDistance = safeDistance;
                    }
                }
                else
                {
                    MessageBox.Show("请输入正确的安全距离数字（单位：mm）");
                    tbx_safeDistance.Text = oldSafeDistance.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改安全距离（单位：mm）时发生异常，异常信息{ex}");
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                SingleLocationDoubleVehicleLocationSettiing frm = new SingleLocationDoubleVehicleLocationSettiing(_device, "车辆列表");
                frm.ShowDialog();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                SingleLocationDoubleVehicleLocationPrioritySettiing frm = new SingleLocationDoubleVehicleLocationPrioritySettiing(_device, true);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                SingleLocationDoubleVehicleLocationPrioritySettiing frm = new SingleLocationDoubleVehicleLocationPrioritySettiing(_device, false);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                frmStationTaskFilterSetting frm = new frmStationTaskFilterSetting(_device);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.btn_slowDistance, "设置两车减速距离，属于提升车辆运行效率优化项");//p.SetToolTip(控件名称, "提示内容");
        }

        private void btn_safeDistance_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.btn_safeDistance, "设置两车安全距离，属于保证车辆运行安全项");//p.SetToolTip(控件名称, "提示内容");
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.tbx_slowDistance, "设置两车减速距离，属于提升车辆运行效率优化项，请询问车辆设计人员后设置");//p.SetToolTip(控件名称, "提示内容");
        }

        private void tbx_safeDistance_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.tbx_safeDistance, "设置两车安全距离，属于保证车辆运行安全项，请询问车辆设计人员后设置");//p.SetToolTip(控件名称, "提示内容");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                int slowDistance;
                if (int.TryParse(tbx_slowDistance.Text, out slowDistance))
                {
                    var result = MessageBox.Show($"减速距离（单位：mm）即将由 {oldSlowDistance} 调整为 {slowDistance}，请确认！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！\r\n请在专业人员指导下进行修改，否则发生撞车事件则由操作员负责！！！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        string msg;
                        if (slowDistance < oldSafeDistance)
                        {
                            msg = $"本次设置的减速距离（单位：mm） {slowDistance} 小于设置的安全距离 {oldSafeDistance}，这种情况下减速距离在车辆让道计算中将默认无效，否则可能存在撞车风险";
                            if (MessageBox.Show($"{msg}，请确认！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                return;
                            else
                                _logger.Info1($"{msg}，操作员已确认此操作。", this);
                        }
                        _device.Config.SlowDistance = slowDistance;
                        _device.Config.Save();
                        _logger.Info1($"减速距离被人工由 {oldSlowDistance} 调整为 {slowDistance}", this);
                        oldSlowDistance = slowDistance;
                    }
                }
                else
                {
                    MessageBox.Show("请输入正确的减速距离数字（单位：mm）");
                    tbx_safeDistance.Text = oldSlowDistance.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改减速距离（单位：mm）时发生异常，异常信息{ex}");
            }
        }
    }
}
