using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs.Framework;
using NLog;
using Wcs;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public partial class RailGuidedVehicleDeviceUserInterface : Form, IDeviceUserInterface

    {
        RailGuidedVehicleDevice _device;
        Logger _logger;
        string _空闲等待站点;
        string _启用空闲时发送到等待站点;

        public RailGuidedVehicleDeviceUserInterface()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        public void Show(Device device)
        {
            _device = (RailGuidedVehicleDevice)device;
            this.Text = string.Format("查看 {0} 的状态", _device.Name);
            cbxStation.DisplayMember = cbxStation_pick.DisplayMember = cbxStation_put.DisplayMember = "UserCode";
            var array0 = _device.Locations
                .Select(x => (RailGuidedVehicleStation)x)
                .Where(x => !(x is ILocationWildcard))
                .ToArray();
            cbxStation.DataSource = array0;
            var array1 = _device.Locations
                 .Select(x => (RailGuidedVehicleStation)x)
                 .Where(x => !(x is ILocationWildcard))
                 .ToArray();
            cbxStation_pick.DataSource = array1;
            var array2 = _device.Locations
                 .Select(x => (RailGuidedVehicleStation)x)
                 .Where(x => !(x is ILocationWildcard))
                 .ToArray();
            cbxStation_put.DataSource = array2;
            cbxFreeStation.DataSource = _device.Locations
                .Select(x => (RailGuidedVehicleStation)x)
                .Where(x => !(x is ILocationWildcard))
                .ToArray();

            this.ShowDialog();
        }

        private void RailGuidedVehicleDeviceUserInterface_Load(object sender, EventArgs e)
        {
            lbl_IPAddress.Text = _device.IPEndPoint.Address.ToString() + ":" + _device.IPEndPoint.Port.ToString();

            var showFreeVehicleSettings = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("showFreeVehicleSettings", "").Split(',').ToArray();
            if (showFreeVehicleSettings.Contains(_device.Name))
            {
                groupBox4.Visible = true;

                _空闲等待站点 = String.Format("{0}空闲待命站点", _device.Name);
                _启用空闲时发送到等待站点 = string.Format("{0}启用空闲时发送到等待站点", _device.Name);

                var _freeWaitStation = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<String>(_空闲等待站点, "");
                if (_device.Locations.Any(x => x.ToConvertibleCode() == _freeWaitStation))
                    cbxFreeStation.SelectedItem = _device.Locations.First(x=>x.ToConvertibleCode() == _freeWaitStation);
                checkBox1.Checked = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>(_启用空闲时发送到等待站点, false);

                cbxFreeStation.SelectedIndexChanged += cbxFreeStation_SelectedIndexChanged;
                checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            }

            timer1_Tick(null, null);

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                var showVehicleClearCacheCommand = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("showVehicleClearCacheCommand", false);
                if (showVehicleClearCacheCommand)
                    linkClearCache.Visible = true;
                else
                    linkClearCache.Visible = false;

                lblDeviceType.Text = _device.GetType().GetDisplayName();
                lblDeviceName.Text = _device.Name;
                lblLockIP.Text = _device.Locker.IPAddress;
                if (_device.Locker.IsEmpty)
                {
                    lblLockUser.Text = "";
                    btnLock.Enabled = true;
                    btnUnlock.Enabled = false;
                    lklSendHP.Enabled = false;
                    linkLabel2.Enabled = false;
                    linkClearCache.Enabled = false;
                    groupBox4.Enabled = false;
                }
                else
                {
                    btnLock.Enabled = false;
                    btnUnlock.Enabled = true;
                    lblLockUser.Text = _device.Locker.UserName;
                    lklSendHP.Enabled = true;
                    linkLabel2.Enabled = true;
                    linkClearCache.Enabled = true;
                    groupBox4.Enabled = true;
                }

                if (_device.LastStatus == null)
                {
                    lblStatus.Text =
                    lblEvent.Text =
                    lblLocation.Text =
                    lbl_position.Text =
                    lbl_atStation.Text = "-";
                    lblTaskId.Text = "";
                }
                else
                {
                    lblStatus.Text = _device.LastStatus.State.GetDescription() + $"({(int)_device.LastStatus.State:X})";
                    lblEvent.Text = _device.LastStatus.Event.GetDescription() + $"({(int)_device.LastStatus.Event:X})";
                    RailGuidedVehicleStation station = _device.Locations.Select(x => (RailGuidedVehicleStation)x).FirstOrDefault(x => x.StationNo == _device.LastStatus.CurrentStation);
                    if (station != null && station.Synonymous.Count() > 0)
                        lblLocation.Text = $"{_device.LastStatus.CurrentStation}({station.Synonymous.First().DeviceCode})";
                    else
                        lblLocation.Text = string.Format("{0}", _device.LastStatus.CurrentStation);

                    lblTaskId.Text = _device.LastStatus.TaskId;
                    lbl_atStation.Text = _device.LastStatus.AtStation.ToString().ToUpper();
                    lbl_position.Text = _device.LastStatus.Position.ToString();
                }
                if (_device.Locker != null && _device.Locker.IsEmpty == false)
                    gpb_manual.Enabled = true;
                else
                    gpb_manual.Enabled = false;

                if (_device.Warnings == null || _device.Warnings.Length == 0)
                {
                    tbxWarnings.Text = "";
                }
                else
                {
                    tbxWarnings.Text = String.Join("\r\n", _device.Warnings);
                }

                setWorkerStyles();
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void setWorkerStyles()
        {
            var act = (RailGuidedVehicleStepByStepAction)_device.EquipmentActionScheduler.Actions.FirstOrDefault(x =>
                 x.Status == EquipmentActionStatus.Executing
                 || x.Status == EquipmentActionStatus.Error
                 || x.Status == EquipmentActionStatus.Suspend);

            if (act == null || act.StateManager == null)
            {
                lblFromStation.Text =
                    lblEndStation.Text = "-";
                gbxWorkerStatus.Enabled = false;
            }
            else
            {
                gbxWorkerStatus.Enabled = true;
                lblFromStation.Text = act.LoadLocation.UserCode;
                lblEndStation.Text = act.UnloadLocation.UserCode;

                lblStep1.Enabled
                    = lblStep2.Enabled
                    = lblStep3.Enabled
                    = lblStep4.Enabled
                    = lblFromStation.Enabled
                        = lblEndStation.Enabled = false;

                if (act.StateManager.CurrentState is NotArriveToPickingLocationState)
                {
                    lblStep1.Enabled =
                        lblFromStation.Enabled = true;

                }
                else if (act.StateManager.CurrentState is NotPickingState)
                {
                    lblStep2.Enabled = true;
                }
                else if (act.StateManager.CurrentState is NotArriveToUnloadingLocationState)
                {
                    lblStep3.Enabled =
                        lblEndStation.Enabled = true;
                }
                else if (act.StateManager.CurrentState is NotUnloadingState)
                {
                    lblStep4.Enabled = true;
                }
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

        private void lklSendHP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_device.LastStatus == null)
            {
                MessageBox.Show("小车状态未同步，无法执行此操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("是否需要强制清除小车任务？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            try
            {
                _logger.Trace1(string.Format("尝试强制清除 {0} 的任务 {1}...", _device, _device.LastStatus.TaskId), this);
                var cmd = new RailGuidedVehicle.ClearTaskCommand();

                this._device.Write(cmd, cmd.SendSuccess);
                //this._device.Write(cmd.GetBytes());
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLeftPicking_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var station = _device.GetCurrentStation();
                if (station == null)
                {
                    MessageBox.Show(string.Format("{0} 当前不在站点位置，无法发送此命令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始下发手工取货指令...", _device), this);
                var cmd = new RailGuidedVehicle.PickingCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                    0,
                    station.StationNo,
                    ChainAction.LeftPicking);

                _device.Write(cmd, cmd.SendSuccess);
                _logger.Trace1("手工取货指令下发成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLeftPutting_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var station = _device.GetCurrentStation();
                if (station == null)
                {
                    MessageBox.Show(string.Format("{0} 当前不在站点位置，无法发送此命令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始下发手工放货指令...", _device), this);
                var cmd = new RailGuidedVehicle.PuttingCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                    0,
                    station.StationNo,
                    ChainAction.LeftUnloading);

                _device.Write(cmd, cmd.SendSuccess);
                _logger.Trace1("手工放货指令下发成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRightPutting_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var station = _device.GetCurrentStation();
                if (station == null)
                {
                    MessageBox.Show(string.Format("{0} 当前不在站点位置，无法发送此命令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始下发手工放货指令...", _device), this);
                var cmd = new RailGuidedVehicle.PuttingCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                    0,
                    station.StationNo,
                    ChainAction.RightUnloading);

                _device.Write(cmd, cmd.SendSuccess);
                _logger.Trace1("手工放货指令下发成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRightPicking_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var station = _device.GetCurrentStation();
                if (station == null)
                {
                    MessageBox.Show(string.Format("{0} 当前不在站点位置，无法发送此命令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始下发手工取货指令...", _device), this);
                var cmd = new RailGuidedVehicle.PickingCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                    0,
                    station.StationNo,
                    ChainAction.RightPicking);

                _device.Write(cmd, cmd.SendSuccess);
                _logger.Trace1("手工取货指令下发成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWalk_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var station = (RailGuidedVehicle.RailGuidedVehicleStation)cbxStation.SelectedItem;
                if (_device.LastStatus.State == RailGuidedVehicleStatus.无货待命)
                {
                    _logger.Trace1(string.Format("{0} 开始下发手工行走指令...", _device), this);
                    var cmd = new RailGuidedVehicle.NormalWalkCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                        0,
                        station.StationNo);

                    _device.Write(cmd, cmd.SendSuccess);
                    _logger.Trace1($"手工无货行走到{station.StationNo}号站口指令下发成功", this);
                }
                else if (_device.LastStatus.State == RailGuidedVehicleStatus.有货待命)
                {
                    _logger.Trace1(string.Format("{0} 开始下发手工行走指令...", _device), this);
                    var cmd = new RailGuidedVehicle.WalkWithGoodsCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                        0,
                        station.StationNo);

                    _device.Write(cmd, cmd.SendSuccess);
                    _logger.Trace1("手工有货行走到{station.StationNo}号站口指令下发成功", this);
                }
                else
                    MessageBox.Show($"{_device.Name}不是无货待命或者有货待命状态，无法发送手工行走指令");
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<bool>(_启用空闲时发送到等待站点, checkBox1.Checked);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_device.LastStatus == null)
            {
                MessageBox.Show("小车状态未同步，无法执行此操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _logger.Trace1(string.Format("尝试给 {0} 发送远程急停命令...", _device), this);
                var cmd = new RailGuidedVehicle.EmergencyStopCommand();

                this._device.Write(cmd, cmd.SendSuccess);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_device.LastStatus == null)
            {
                MessageBox.Show("小车状态未同步，无法执行此操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("是否需要发送 取消远程急停 命令？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            try
            {
                _logger.Trace1(string.Format("尝试给 {0} 发送取消远程急停命令...", _device), this);
                var cmd = new RailGuidedVehicle.ClearEmergencyStopCommand();

                //this._device.Write(cmd, cmd.SendSuccess);
                this._device.Write(cmd.GetBytes());
                MessageBox.Show("取消急停命令发送成功");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_hb_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_device.CanManual)
                {
                    MessageBox.Show(string.Format("{0} 当前处于繁忙状态，无法下发手工指令", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var start = (RailGuidedVehicle.RailGuidedVehicleStation)cbxStation_pick.SelectedItem;
                //if (start.PickingChainAction == ChainAction.None)
                //{
                //    MessageBox.Show(string.Format("站点 {0} 无法执行取货动作", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                var end = (RailGuidedVehicle.RailGuidedVehicleStation)cbxStation_put.SelectedItem;
                //if (end.PuttingChainAction == ChainAction.None)
                //{
                //    MessageBox.Show(string.Format("站点 {0} 无法执行放货动作", _device), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                _logger.Trace1(string.Format("{0} 开始下发手工全自动指令...", _device), this);
                var cmd = new RailGuidedVehicle.AutoCommand(Wcs.Framework.SerialNumberFactory.GenerateRandomValue(80000000, 99999999).ToString(),
                    0,
                    start.StationNo,
                    start.PickingChainAction,
                    end.StationNo,
                    end.PuttingChainAction);
                _device.Write(cmd, cmd.SendSuccess);
                _logger.Trace1("手工全自动指令指令下发成功", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkClearCache_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_device.LastStatus == null)
            {
                MessageBox.Show("小车状态未同步，无法执行此操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("是否需要强制清空小车接收块？注：此命令仅调试人员使用，正式使用时请屏蔽该功能！！！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            try
            {
                _logger.Trace1(string.Format("尝试强制清空小车{0}接收块...", _device), this);
                var cmd = new RailGuidedVehicle.ClearClearCacheCommand();

                this._device.Write(cmd.GetBytes());
                MessageBox.Show(string.Format("尝试强制清空小车{0}接收块命令发送成功...", _device));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void cbxFreeStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var _freeStation = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<String>(_空闲等待站点, "");
                var station = (Location)cbxFreeStation.SelectedItem;
                if (_freeStation != station.ToConvertibleCode())
                    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<String>(_空闲等待站点, station.ToConvertibleCode());
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }
    }
}
