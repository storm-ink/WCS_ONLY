using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Wcs.Framework;
using NLog;
//using Wcs.DefaultImpls.CraneSubSystem.CraneService;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public partial class CraneDeviceUserInterface : Form, IDeviceUserInterface
    {
        CraneDevice _device;

        Logger _logger;
        public CraneDeviceUserInterface()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            InitializeComponent();
        }

        private void CraneDeviceUserInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            //checkBox2.CheckedChanged -= checkBox2_CheckedChanged;
            timer1.Stop();
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

        public void Show(Device device)
        {

            _device = (CraneDevice)device;

            this.Text = string.Format("查看 {0} 的状态", _device.Name);

            this.ShowDialog();
        }

        private void btnStartCHTS_Click(object sender, EventArgs e)
        {
            try
            {
                string chtsApplication = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("手持地操应用程序路径", null);
                if (string.IsNullOrWhiteSpace(chtsApplication) || !System.IO.File.Exists(chtsApplication))
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.FileName = "CHTS";
                        ofd.DefaultExt = "exe";
                        ofd.Filter = "Windows应用程序 (*.exe)|*.exe";
                        ofd.InitialDirectory = Application.StartupPath;
                        ofd.Multiselect = false;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            chtsApplication = ofd.FileName;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(chtsApplication))
                    {
                        Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<string>("手持地操应用程序路径", chtsApplication);
                    }
                }

                if (!string.IsNullOrWhiteSpace(chtsApplication) && System.IO.File.Exists(chtsApplication))
                {
                    System.Diagnostics.Process.Start(chtsApplication);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
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
                    gpb_halfAuto.Enabled = false;
                }
                else
                {
                    btnLock.Enabled = false;
                    btnUnlock.Enabled = true;
                    gpb_halfAuto.Enabled = true;
                    lblLockUser.Text = _device.Locker.UserName;
                }

                if (_device.LastStatus == null)
                {
                    lblStatus.Text =
                    lblEvent.Text =
                    lblLocation.Text =
                    lblErrorCode.Text = "-";
                    lblForkHorizontalPosition.Text = "";
                    lblForkVerticalPosition.Text = "";
                    lblTaskId.Text = "";
                    linkLabel1.Text = "远程急停";
                }
                else
                {
                    lblStatus.Text = _device.LastStatus.State.GetDescription() + $"({(int)_device.LastStatus.State})";
                    lblEvent.Text = _device.LastStatus.Event.GetDescription() + $"({(int)_device.LastStatus.Event})";
                    var lostaions = _device.Locations.Select(x => (RackLocation)x).Where(x => !(x is RackLocationWildcard) && x.Column == _device.LastStatus.Column && x.Level == _device.LastStatus.Level).Select(x => x.UserCode);
                    lblLocation.Text = $"{_device.LastStatus.Column.ToString("000")}列 {_device.LastStatus.Level.ToString("000")}层 ({string.Join(" | ", lostaions)})(列编码值：{_device.LastStatus.ColumnCodeValue} 层编码值：{_device.LastStatus.LevelCodeValue})";
                    if (string.IsNullOrWhiteSpace(_device.LastStatus.ErrorCode))
                    {
                        lblErrorCode.Text = $"{_device.LastStatus.ErrorCode}";
                        linkLabel1.Text = "远程急停";
                    }
                    else
                    {
                        var alarm = DeviceErrorHelper.DeviceErrorTypes.FirstOrDefault(x => x.DeviceType == "堆垛机" && x.DeviceErrorCode == _device.LastStatus.ErrorCode);
                        if (alarm == null)
                            lblErrorCode.Text = $"{_device.LastStatus.ErrorCode}_未知错误(此故障码系统未记录)";
                        else
                            lblErrorCode.Text = $"{_device.LastStatus.ErrorCode}_{alarm.ErrorName}";

                        if (_device.LastStatus.State == CraneStatus.AlarmAndShutdown && lblErrorCode.Text.Contains("远程急停"))
                            linkLabel1.Text = "取消急停";
                        else
                            linkLabel1.Text = "远程急停";
                    }

                    lblForkHorizontalPosition.Text = _device.LastStatus.ForkHorizontalPosition.GetDescription() + $"(Z1:{_device.LastStatus.ForkCodeValue_Z1},Z2:{_device.LastStatus.ForkCodeValue_Z2})";
                    lblForkVerticalPosition.Text = _device.LastStatus.ForkVerticalPosition.GetDescription();
                    lblTaskId.Text = _device.LastStatus.TaskId;
                }

                lbl_ConnState.Text = _device.IsConnected ? "已连接" : "未连接";
                lab_IPAddress.Text = _device.IPEndPoint.Address.ToString() + ":" + _device.IPEndPoint.Port.ToString();

                if (_device.Warnings == null || _device.Warnings.Length == 0)
                {
                    tbxWarnings.Text = "";
                }
                else
                {
                    tbxWarnings.Text = String.Join("\r\n", _device.Warnings);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CraneDeviceUserInterface_Load(object sender, EventArgs e)
        {
            //String _堆垛机任务等待超时提升优先级_当前堆垛机 = _堆垛机任务等待超时提升优先级 + "_" + _device.Name;
            //UInt32 overTime;
            //if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey(_堆垛机任务等待超时提升优先级_当前堆垛机))
            //    overTime = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt32>(_堆垛机任务等待超时提升优先级_当前堆垛机, 0);
            //else
            //    overTime = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt32>(_堆垛机任务等待超时提升优先级, 0);
            //textBox1.Text = overTime.ToString();

            //String _堆垛机特殊出库策略_当前堆垛机 = "堆垛机特殊出库策略_急速出库_" + _device.Name;
            //checkBox2.Checked = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>(_堆垛机特殊出库策略_当前堆垛机, false);
            //checkBox2.CheckedChanged += checkBox2_CheckedChanged;

            //String _出库提前待命_当前堆垛机 = "出库提前待命_" + _device.Name;
            //checkBox3.Checked = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>(_出库提前待命_当前堆垛机, false);
            //checkBox3.CheckedChanged += checkBox3_CheckedChanged;

            var locations = WcsConfiguration
            .Instance
            .LocationCollection
            .Locations
            .Where(x => x.Device.Name == _device.Name && !(x is Wcs.Framework.ILocationWildcard));
            AutoCompleteStringCollection locationsAutoCompleteStringCollection = locationsConvertToAutoCompleteStringCollection(locations);
            tbx_start.AutoCompleteCustomSource = locationsAutoCompleteStringCollection;
            tbx_end.AutoCompleteCustomSource = locationsAutoCompleteStringCollection;
            var version = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "");

            if (string.IsNullOrWhiteSpace(version))
                throw new Exception("未配置堆垛机协议版本类型");

            if (version == "V0")
            {
                tbx_barcode.Enabled = false;
                chbx_needCheck.Enabled = false;
            }
            else
            {
                tbx_barcode.Enabled = true;
                chbx_needCheck.Enabled = true;
            }


            timer1_Tick(null, null);

            timer1.Start();
        }

        AutoCompleteStringCollection locationsConvertToAutoCompleteStringCollection(IEnumerable<Location> locations)
        {
            AutoCompleteStringCollection result = new AutoCompleteStringCollection();

            result.AddRange(locations.Where(x => x != null && x.UserCode != null).Select(x => x.UserCode).ToArray());

            return result;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
        }

        //private void checkBox1_CheckedChanged(object sender, EventArgs e)
        //{
        //    CheckBox _cbx = (CheckBox)sender;
        //    if (_cbx.Checked)
        //        textBox1.Enabled = button1.Enabled = button2.Enabled = true;
        //    else
        //        textBox1.Enabled = button1.Enabled = button2.Enabled = false;
        //}

        //const String _堆垛机任务等待超时提升优先级 = "堆垛机任务等待超时提升优先级";
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    UInt32 overTime;
        //    if (UInt32.TryParse(textBox1.Text, out overTime))
        //    {
        //        String _堆垛机任务等待超时提升优先级_当前堆垛机 = _堆垛机任务等待超时提升优先级 + "_" + _device.Name;
        //        Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<UInt32>(_堆垛机任务等待超时提升优先级_当前堆垛机, overTime);
        //        checkBox1.Checked = false;
        //    }
        //    else
        //        MessageBox.Show("请输入不小于0的正确分钟数");
        //}

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    UInt32 overTime;
        //    if (UInt32.TryParse(textBox1.Text, out overTime))
        //    {
        //        Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<UInt32>(_堆垛机任务等待超时提升优先级, overTime);
        //        checkBox1.Checked = false;
        //    }
        //    else
        //        MessageBox.Show("请输入不小于0的正确分钟数");
        //}

        //private void checkBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    String _堆垛机特殊出库策略_当前堆垛机 = "堆垛机特殊出库策略_急速出库_" + _device.Name;
        //    CheckBox _cbx = (CheckBox)sender;
        //    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<Boolean>(_堆垛机特殊出库策略_当前堆垛机, _cbx.Checked);
        //}

        //private void checkBox3_CheckedChanged(object sender, EventArgs e)
        //{
        //    String _出库提前待命_当前堆垛机 = "出库提前待命_" + _device.Name;
        //    CheckBox _cbx = (CheckBox)sender;
        //    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<Boolean>(_出库提前待命_当前堆垛机, _cbx.Checked);
        //}

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (linkLabel1.Text == "远程急停")
                {
                    _device.EmergencyStop();
                    MessageBox.Show($"{lblDeviceName.Text} 远程急停命令发送成功");
                }
                else if (linkLabel1.Text == "取消急停")
                {
                    _device.CancelEmergencyStop(_device.Locker);
                    MessageBox.Show($"{lblDeviceName.Text} 取消急停命令发送成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} {linkLabel1.Text} 命令发送失败，异常消息{ex}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show($"即将给设备 {_device} 发送召回命令，请确认！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    MessageBox.Show($"用户已取消发送召回命令");
                    return;
                }

                _device.BackToTheOrigin(_device.Locker);
                MessageBox.Show($"{lblDeviceName.Text} 召回命令发送成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} 召回命令发送失败，异常消息{ex}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                _device.CancleTask();
                MessageBox.Show($"{lblDeviceName.Text} 取消命令发送成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} 取消命令发送失败，异常消息{ex}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                _device.ResetWarn(_device.Locker);
                MessageBox.Show($"{lblDeviceName.Text} 复位命令发送成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} 复位命令发送失败，异常消息{ex}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //BY - 搬运任务 - 全自动命令
                //YD - 移动命令 - 半自动命令
                //QH - 取货命令 - 半自动命令
                //FH - 放货命令 - 半自动命令
                var barcode = tbx_barcode.Text.Trim();
                var needCheck = chbx_needCheck.Checked;

                if (comboBox1.Text.StartsWith("BY"))
                {
                    string taskId = "99";
                    if (_device.LastStatus.TaskId.StartsWith("99"))
                        taskId += (int.Parse(_device.LastStatus.TaskId.Substring(4)) + 1).ToString("000000");
                    else
                        taskId += "000001";

                    var start = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_start.Text);
                    if (start == null)
                        throw new Exception("起始位置选择/输入错误");
                    var end = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_end.Text);
                    if (end == null)
                        throw new Exception("终点位置选择/输入错误");

                    var result = MessageBox.Show($"即将给设备 {_device} 发送全自动取放货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        MessageBox.Show($"用户已取消发送全自动命令");
                        return;
                    }
                    _logger.Info1($"即将给设备 {_device} 发送全自动取放货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", this);
                    AddTaskCommand cmd = new AddTaskCommand(taskId, AddTaskCommandType.全自动, (RackLocation)start, (RackLocation)end, barcode, needCheck);
                    _device.Write(cmd, cmd.SendSuccess);
                }
                if (comboBox1.Text.StartsWith("YD"))
                {
                    string taskId = "88";
                    if (_device.LastStatus.TaskId.StartsWith("88"))
                        taskId += (int.Parse(_device.LastStatus.TaskId.Substring(4)) + 1).ToString("000000");
                    else
                        taskId += "000001";

                    var end = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_end.Text);
                    if (end == null)
                        throw new Exception("终点位置选择/输入错误");

                    var start = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == _device.LastStatus.Column && x.Level == _device.LastStatus.Level);
                    if (start == null)
                    {
                        if (MessageBox.Show("堆垛机所支持的位置中未找到 当前设备上报所在位置 即将启用有效终点位置代替请确认是否发送", "TIPS:", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                        {
                            MessageBox.Show("用户取消发送");
                            return;
                        }
                        start = (RackLocation)end;
                    }

                    var result = MessageBox.Show($"即将给设备 {_device} 发送全半动行走命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        MessageBox.Show($"用户已取消发送全自动命令");
                        return;
                    }
                    _logger.Info1($"即将给设备 {_device} 发送全半动行走命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", this);
                    AddTaskCommand cmd = new AddTaskCommand(taskId, AddTaskCommandType.半自动行走, (RackLocation)start, (RackLocation)end, barcode, needCheck);
                    _device.Write(cmd, cmd.SendSuccess);
                }
                if (comboBox1.Text.StartsWith("QH"))
                {
                    string taskId = "88";
                    if (_device.LastStatus.TaskId.StartsWith("88"))
                        taskId += (int.Parse(_device.LastStatus.TaskId.Substring(4)) + 1).ToString("000000");
                    else
                        taskId += "000001";

                    var start = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_start.Text);
                    if (start == null)
                        throw new Exception("取货位置选择/输入错误");

                    MessageBox.Show("此处放货位置仅用于堆垛机针对货物高度进行安全检测，请输入正确放货位置,否则手动操作发生碰撞由操作员负责");

                    var end = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_end.Text);
                    if (start == null)
                        throw new Exception("放货位置选择/输入错误，此处放货位置仅用于堆垛机针对货物高度进行安全检测");

                    var result = MessageBox.Show($"即将给设备 {_device} 发送半自动取货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        MessageBox.Show($"用户已取消发送全自动命令");
                        return;
                    }
                    _logger.Info1($"即将给设备 {_device} 发送半自动取货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", this);

                    AddTaskCommand cmd = new AddTaskCommand(taskId, AddTaskCommandType.半自动取货, (RackLocation)start, (RackLocation)end, barcode, needCheck);
                    _device.Write(cmd, cmd.SendSuccess);
                }
                if (comboBox1.Text.StartsWith("FH"))
                {
                    string taskId = "88";
                    if (_device.LastStatus.TaskId.StartsWith("88"))
                        taskId += (int.Parse(_device.LastStatus.TaskId.Substring(4)) + 1).ToString("000000");
                    else
                        taskId += "000001";

                    var end = _device.Locations.FirstOrDefault(x => x.UserCode == tbx_end.Text);
                    if (end == null)
                        throw new Exception("终点位置选择/输入错误");
                    var start = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == _device.LastStatus.Column && x.Level == _device.LastStatus.Level);
                    if (start == null)
                    {
                        if (MessageBox.Show("堆垛机所支持的位置中未找到 当前设备上报所在位置 即将启用有效终点位置代替请确认是否发送", "TIPS:", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                        {
                            MessageBox.Show("用户取消发送");
                            return;
                        }
                        start = (RackLocation)end;
                    }

                    var result = MessageBox.Show($"即将给设备 {_device} 发送半自动取放货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        MessageBox.Show($"用户已取消发送半自动命令");
                        return;
                    }
                    _logger.Info1($"即将给设备 {_device} 发送半自动取放货命令\r\n起点位置:{start.UserCode} --> 终点位置：{end.UserCode}，请确认！", this);
                    AddTaskCommand cmd = new AddTaskCommand(taskId, AddTaskCommandType.半自动放货, (RackLocation)start, (RackLocation)end, barcode, needCheck);
                    _device.Write(cmd, cmd.SendSuccess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} {comboBox1.Text} 命令发送失败，异常消息{ex}");
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.StartsWith("BY"))
                tbx_start.Enabled = tbx_end.Enabled = true;
            else if (comboBox1.Text.StartsWith("YD"))
            {
                tbx_start.Enabled = false;
                tbx_end.Enabled = true;
            }
            else if (comboBox1.Text.StartsWith("QH"))
            {
                tbx_start.Enabled = true;
                tbx_end.Enabled = true;
            }
            else if (comboBox1.Text.StartsWith("FH"))
            {
                tbx_start.Enabled = false;
                tbx_end.Enabled = true;
            }

            tbx_start.Text = "<选择/输入堆垛机位置>";
            tbx_end.Text = "<选择/输入堆垛机位置>";
        }

        private void btnCUp_Click(object sender, EventArgs e)
        {
            try
            {
                string taskId = "S";
                if (_device.LastStatus.TaskId.StartsWith("S"))
                    taskId += (int.Parse(_device.LastStatus.TaskId.Substring(4)) + 1).ToString("0000000");
                else
                    taskId += "0000001";

                var start = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == _device.LastStatus.Column && x.Level == _device.LastStatus.Level);
                if (start == null)
                    throw new Exception("起始位置选择/输入错误");
                var end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == _device.LastStatus.Column && x.Level == _device.LastStatus.Level);
                if (end == null)
                    throw new Exception("堆垛机所支持的位置中未找到 当前设备上报所在位置");

                AddTaskCommand cmd = new AddTaskCommand(taskId, AddTaskCommandType.半自动行走, (RackLocation)start, (RackLocation)end, "", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{lblDeviceName.Text} 左一取货 命令发送失败，异常消息{ex}");
            }
        }
    }
}
