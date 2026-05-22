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
using NHibernate.Linq;
using System.Reflection;
using System.Dynamic;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public partial class ConveyorDeviceUserInterface : Form, IDeviceUserInterface
    {
        ConveyorDevice _device;
        DefaultConveyorTcpProtocolDataReceiver _receiver;
        Logger _logger;
        public ConveyorDeviceUserInterface()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            InitializeComponent();

            if (!Wcs.Security.WcsPermission.IsAdministratorMode)
            {
                tpLastSentBytes.Parent = null;
                _lastSentBytesTimer.Enabled = false;
            }
            else
            {
                _lastSentBytesTimer.Interval = 500;
                _lastSentBytesTimer.Enabled = true;
            }
        }

        private void ConveyorDeviceUserInterface_Load(object sender, EventArgs e)
        {
            DefaultConveyorTcpProtocolDataReceiver receiver = (DefaultConveyorTcpProtocolDataReceiver)_device.DataReceiver;
            var items = receiver.ReceiverEncoding._collectionSettings.Select(x => new _item(x.type)).ToArray();
            cmbEncodingTypes.Items.AddRange(items);

            var remoteStatus = _device.ReadStatus<RemoteCMDBlock>();
            if (remoteStatus != null && remoteStatus.Length > 0)
            {
                var areaNos = remoteStatus.Select(x => x.AreaNo.ToString()).ToArray();
                cbx_areaNo.Items.AddRange(areaNos);
            }

            //panel2.Visible = panel3.Visible = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("showConveyorDeviceTool", false);

            timer1_Tick(null, null);
            timer1.Start();
        }

        private void ConveyorDeviceUserInterface_Closing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            try
            {
                _device.Lock(new LockerInfo(System.Environment.MachineName, LockerInfo.GetIpAddress()));
                btnLock.Enabled = false;
                btnUnlock.Enabled = true;
                btn_Manual.Enabled = true;

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
            btn_Manual.Enabled = false;
            try
            {
                _device.Unlock(LockerInfo.Adminstrator);
                btnLock.Enabled = true;
                btnUnlock.Enabled = false;
                btn_Manual.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Show(Device device)
        {

            _device = (ConveyorDevice)device;
            _receiver = (DefaultConveyorTcpProtocolDataReceiver)_device.DataReceiver;
            this.Text = string.Format("查看 {0} 的状态", _device.Name);

            this.ShowDialog();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                loadInfo();
                loadTasks();
                loadLocationTasks();
                loadBjs();
                loadZW();

                if (cbx_areaNo.Items.Count == 0)
                {
                    var remoteStatus = _device.ReadStatus<RemoteCMDBlock>();
                    if (remoteStatus != null && remoteStatus.Length > 0)
                    {
                        var areaNos = remoteStatus.Select(x => x.AreaNo.ToString()).ToArray();
                        cbx_areaNo.Items.AddRange(areaNos);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void loadInfo()
        {
            lblDeviceType.Text = _device.GetType().GetDisplayName();
            lblDeviceName.Text = _device.Name;

            if (_device.Locker.IsEmpty)
            {
                lblIpAddress.Text = "";
                lblLockUser.Text = "";
                btnLock.Enabled = true;
                btnUnlock.Enabled = false;
                btn_Manual.Enabled = false;
            }
            else
            {
                btnLock.Enabled = false;
                btnUnlock.Enabled = true;
                lblIpAddress.Text = _device.Locker.IPAddress;
                lblLockUser.Text = _device.Locker.UserName;
                btn_Manual.Enabled = true;
            }
        }

        void loadTasks()
        {
            lvTasks.Items.Clear();
            //运行中的任务
            IEnumerable<TaskBlock> runningTasks = new List<TaskBlock>();
            if (_device.TaskBlocks != null)
            {
                runningTasks = _device.TaskBlocks
                    .Where(x => x.TaskNo != 0 && x.HandShake != TaskHandShakes.Empty
                        && x.TaskState != TaskBlockTaskStatus.Empty
                        && x.TaskState != TaskBlockTaskStatus.Finished
                        && x.TaskState != TaskBlockTaskStatus.Error);
            }


            //残余的任务
            IEnumerable<TaskBlock> cyTasks = new List<TaskBlock>();
            if (_device.TaskBlocks != null)
            {
                cyTasks = _device.TaskBlocks
                    .Where(x => x.TaskNo != 0 && x.HandShake != TaskHandShakes.Empty
                        &&
                        (x.TaskState == TaskBlockTaskStatus.Finished
                        || x.TaskState == TaskBlockTaskStatus.Error)
                        );
            }

            linkLabel3.Text = runningTasks.Count().ToString();
            linkLabel5.Text = cyTasks.Count().ToString();

            foreach (var item in runningTasks)
            {
                ListViewItem lvi = new ListViewItem(new string[]
                {
                    item.TaskNo.ToString(),
                    item.HandShake.GetDescription(),
                    item.TaskState.GetDescription(),
                    item.From.ToString(),
                    item.To.ToString(),
                    "",
                    "",
                    item.AtPacketIndex.ToString()
                }, lvTasks.Groups["lvgRunningTasks"]);
                lvi.Tag = item;
                lvTasks.Items.Add(lvi);
            }

            foreach (var item in cyTasks)
            {
                ListViewItem lvi = new ListViewItem(new string[]
                {
                    item.TaskNo.ToString(),
                    item.HandShake.GetDescription(),
                    item.TaskState.GetDescription(),
                    item.From.ToString(),
                    item.To.ToString(),
                    "",
                    "",
                    item.AtPacketIndex.ToString()
                }, lvTasks.Groups["lvgCYTasks"]);
                lvi.Tag = item;
                lvTasks.Items.Add(lvi);
            }
        }

        void loadLocationTasks()
        {
            lvLocationCurrentTasks.Items.Clear();
            IEnumerable<LocationInfoBlock> locationTasks = new List<LocationInfoBlock>();
            if (_device.LocationInfoBlocks != null)
            {
                locationTasks = _device.LocationInfoBlocks.Where(x => x.TaskNo != 0);
            }

            linkLabel4.Text = locationTasks.Count().ToString();
            foreach (var item in locationTasks)
            {
                ListViewItem lvi;
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("ConveyorLocationTask", "") != "V1")
                {
                    lvi = new ListViewItem(new string[]
                    {
                    item.PosNo.ToString(),
                    item.TaskNo.ToString(),
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ""
                    });
                }
                else
                {
                    lvi = new ListViewItem(new string[]
                    {
                    item.PosNo.ToString(),
                    item.TaskNo.ToString(),
                    "",
                    ""
                    });
                }

                lvi.Tag = item;

                lvLocationCurrentTasks.Items.Add(lvi);
            }
        }

        void loadBjs()
        {
            lvBJHW.Items.Clear();
            //if (_device.MachineAlarms == null)
            //{
            //    linkLabel2.Text = "0";
            //    return;
            //}

            //if (_device.MachineAlarms.Length>0)
            //{
            //    var alarms = _device.MachineAlarms.Select(x => new
            //    {
            //        PosNo = x.PosNo,
            //        Alarms = x.ToAlarms()
            //    }).Where(x => x.Alarms != null && x.Alarms.Length > 0);

            //    linkLabel2.Text = alarms.Count().ToString();
            //    foreach (var item in alarms)
            //    {
            //        ListViewItem lvi = new ListViewItem(
            //            new string[]
            //        {
            //        item.PosNo.ToString(),
            //        string.Join(",",item.Alarms)
            //        });
            //        lvBJHW.Items.Add(lvi);
            //    }

            //}
            //else 
            //if (_device.ReadStatus<GeneralAlarmNetTransferObject>().Length > 0)
            //{
            //    var alarms = _device.ReadStatus<GeneralAlarmNetTransferObject>().Select(x => new
            //    {
            //        PosNo = x.PosNo,
            //        Alarms = x.GetAlarm()
            //    }).Where(x => x.Alarms != null && x.Alarms.Length > 0);

            //    linkLabel2.Text = alarms.Count().ToString();
            //    foreach (var item in alarms)
            //    {
            //        ListViewItem lvi = new ListViewItem(
            //            new string[]
            //        {
            //        item.PosNo.ToString(),
            //        string.Join(",",item.Alarms)
            //        });
            //        lvBJHW.Items.Add(lvi);
            //    }
            //}
        }

        void loadZW()
        {
            lvZW.Items.Clear();
            IEnumerable<RequestBlock> requests = new List<RequestBlock>();
            if (_device.RequestBlocks != null)
            {
                requests = _device.RequestBlocks.Where(x => x.PosNo != 0 && x.HandShake == RequestHandShakes.New);
            }

            linkLabel1.Text = requests.Count().ToString();

            foreach (var item in requests)
            {
                ListViewItem lvi = new ListViewItem(
                   new string[]
                {
                    item.PosNo.ToString(),
                    item.HandShake.GetDescription(),
                    item.IOData.ToString(),
                    item.RequestID.ToString(),
                    item.AtPacketIndex.ToString()
                });
                lvi.Tag = item;
                lvZW.Items.Add(lvi);
            }
        }



        TaskBlock currentSelectedTask
        {
            get
            {
                if (lvTasks.SelectedItems.Count != 1)
                {
                    return null;
                }

                return lvTasks.SelectedItems[0].Tag as TaskBlock;
            }
        }


        private void cmsTasks_Opening(object sender, CancelEventArgs e)
        {
            timer1.Stop();
            删除选中设备任务ToolStripMenuItem.Enabled = currentSelectedTask != null && currentSelectedTask.TaskState != TaskBlockTaskStatus.Finished;
            tsmiClearTask.Enabled = currentSelectedTask != null && currentSelectedTask.TaskState == TaskBlockTaskStatus.Finished;
        }

        static Random _rnd = new Random();
        private void 删除选中设备任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var task = currentSelectedTask;
            if (task == null)
            {
                return;
            }

            EquipmentAction action;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                action = unitOfWork.session.Query<EquipmentAction>()
                    .SingleOrDefault(x => x.EquipmentTaskId == task.TaskNo);

                unitOfWork.Commit();
            }

            if (action != null
                && (action.Status != EquipmentActionStatus.Completed
                    && action.Status != EquipmentActionStatus.Cancelled
                    && action.Status != EquipmentActionStatus.Error
                    && action.Status != EquipmentActionStatus.Suspend)
                )
            {
                MessageBox.Show(String.Format("系统下发的 {0} 未完成，无法手动删除该设备任务", action), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(String.Format("是确定要手动删除 {0}", task), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                TaskCommand cmd = new TaskCommand(
                     TaskHandShakes.ApplyForDelete,
                    Convert.ToUInt32(task.TaskNo),
                    "",
                    new UInt16[10],
                    Convert.ToUInt16(task.RotingNo),
                    Convert.ToUInt16(task.From),
                    Convert.ToUInt16(task.To),
                    Convert.ToUInt16(task.AtPacketIndex));

                _device.Write(cmd, (device, _cmd) =>
                {
                    ConveyorDevice conveyorDevice = (ConveyorDevice)device;

                    return conveyorDevice.TaskBlocks != null &&
                        conveyorDevice.TaskBlocks[_cmd.DBIndex - 1].TaskNo != _cmd.TaskNo;
                });
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsmiClearTask_Click(object sender, EventArgs e)
        {
            var task = currentSelectedTask;
            if (task == null)
            {
                return;
            }

            ConveyorTransferAction action;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                action = unitOfWork.session.Query<ConveyorTransferAction>()
                    .SingleOrDefault(x => x.EquipmentTaskId == task.TaskNo);

                unitOfWork.Commit();

            }

            if (action != null
                   && (action.Status != EquipmentActionStatus.Completed
                       && action.Status != EquipmentActionStatus.Cancelled
                       && action.Status != EquipmentActionStatus.Error
                       && action.Status != EquipmentActionStatus.Suspend)
                   )
            {
                MessageBox.Show(String.Format("系统下发的 {0} 未完成，无法手动清除该设备任务", action), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(String.Format("是确定要手动清除 {0}", task), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                TaskCommand cmd = new TaskCommand(
                     TaskHandShakes.ApplyForDelete,
                    Convert.ToUInt32(task.TaskNo),
                    "",
                    new UInt16[20],
                    Convert.ToUInt16(task.RotingNo),
                    Convert.ToUInt16(task.From),
                    Convert.ToUInt16(task.To),
                    Convert.ToUInt16(task.AtPacketIndex));

                _device.Write(cmd, (device, _cmd) =>
                {
                    ConveyorDevice conveyorDevice = (ConveyorDevice)device;

                    return conveyorDevice.TaskBlocks != null
                        && conveyorDevice.TaskBlocks.Length > 0
                        && conveyorDevice.TaskBlocks[_cmd.DBIndex - 1].HandShake == TaskHandShakes.Empty;
                });
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        LocationInfoBlock currentSelectedLocationCurrentTask
        {
            get
            {
                if (lvLocationCurrentTasks.SelectedItems.Count != 1)
                {
                    return null;
                }

                return lvLocationCurrentTasks.SelectedItems[0].Tag as LocationInfoBlock;
            }
        }

        private void cmsLocationCurrentTasks_Opening(object sender, CancelEventArgs e)
        {
            timer1.Stop();

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("ConveyorLocationTask", "") == "V1")
            {
                设置选中货位任务_分路号_承载ToolStripMenuItem.Enabled = currentSelectedLocationCurrentTask != null;
                删除选中货位任务ToolStripMenuItem.Visible = false;
            }
            else
            {
                删除选中货位任务ToolStripMenuItem.Enabled = currentSelectedLocationCurrentTask != null;
                设置选中货位任务_分路号_承载ToolStripMenuItem.Visible = false;
            }
        }


        private void 删除选中货位任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var task = currentSelectedLocationCurrentTask;
            if (task == null)
            {
                return;
            }

            if (_device.TaskBlocks != null && _device.TaskBlocks.Any(x => x.HandShake != TaskHandShakes.Empty && x.TaskNo == task.TaskNo))
            {
                MessageBox.Show(String.Format("任务缓冲区存在未重置的任务号为 {0} 的任务，无法手动删除该货位任务", task.TaskNo), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (MessageBox.Show(String.Format("是确定要手动删除 {0}", task), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ClearLocationTaskCommand cmd = new ClearLocationTaskCommand(task.PosNo, task.TaskNo);

                _device.Write(cmd, (device, _cmd) =>
                {
                    ConveyorDevice conveyorDevice = (ConveyorDevice)device;
                    LocationInfoBlock locationInfoBlock = null;
                    if (conveyorDevice.LocationInfoBlocks != null)
                        locationInfoBlock = conveyorDevice.LocationInfoBlocks.FirstOrDefault(x => x.PosNo == cmd.PosNo);
                    return locationInfoBlock != null && locationInfoBlock.TaskNo != _cmd.TaskNo;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("手动删除 {0} 失败,{1}", task, ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void 设置选中货位任务_分路号_承载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("待开发，请稍后！");
            //var task = currentSelectedLocationCurrentTask;
            //if (task == null)
            //{
            //    return;
            //}

            ////if (_device.TaskBlocks != null && _device.TaskBlocks.Any(x => x.HandShake != TaskNetTransferObjectHandShake.Empty && x.TaskNo == task.TaskNo))
            ////{
            ////    MessageBox.Show(String.Format("任务缓冲区存在未重置的任务号为 {0} 的任务，无法手动设置该货位任务", task.TaskNo), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ////    return;
            ////}


            //if (MessageBox.Show(String.Format("是确定要手动设置 {0}", task), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            //{
            //    return;
            //}

            //try
            //{
            //    ClearLocationTaskCommand cmd = new ClearLocationTaskCommand(task.PosNo, task.TaskNo, task.TUID, Convert.ToUInt16(task.AtPacketIndex), Convert.ToUInt16(_rnd.Next(1, Int16.MaxValue)));

            //    _device.Write(cmd, (device, _cmd) =>
            //    {
            //        ConveyorDevice conveyorDevice = (ConveyorDevice)device;
            //        return conveyorDevice.LocationCurrentTasks != null
            //            && conveyorDevice.LocationCurrentTasks.Length > 0
            //            && conveyorDevice.LocationCurrentTasks[_cmd.DB1002_Index - 1].TaskNo != _cmd.TaskNo;
            //    });
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(String.Format("手动设置 {0} 失败,{1}", task, ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        class _item
        {
            public _item(Type type)
            {
                this.type = type;
            }
            public Type type { get; set; }
            public override string ToString()
            {
                return type.GetDisplayName();
            }
        }

        private void cmbEncodingTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbEncodingTypes.SelectedItem == null)
                {
                    return;
                }

                Type type = ((_item)cmbEncodingTypes.SelectedItem).type;

                MethodInfo readStatusMethod = typeof(ConveyorDevice).GetMethod("ReadStatus");
                readStatusMethod = readStatusMethod.MakeGenericMethod(type);

                var datas = (IEnumerable<object>)readStatusMethod.Invoke(_device, null);
                if (!string.IsNullOrWhiteSpace(cbx_property.Text) && !string.IsNullOrWhiteSpace(tbx_propertyValue.Text))
                {
                    #region datas as object[]转换成可以使用lameda表达式的数组，再使用DataTable动态创建显示类型
                    var name = cbx_property.Text.Trim();
                    var value = tbx_propertyValue.Text.Trim();
                    var _datas = datas as object[];
                    DataTable dataTable = new DataTable();
                    if (_datas.Length > 0)
                    {
                        var item = _datas[0];
                        MethodInfo getDataGridViewShowMethod = item.GetType().GetMethod("GetDataGridViewShow");
                        var _data = getDataGridViewShowMethod.Invoke(item, null);
                        foreach (PropertyInfo _item in _data.GetType().GetProperties())
                        {
                            try
                            {
                                //var obj = _data.GetType().GetProperty(_item.Name).GetValue(_data, null);
                                dataTable.Columns.Add(new DataColumn(_item.Name, typeof(Object)));
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                        return;

                    List<Object> list = new List<object>();
                    foreach (var item in _datas)
                    {
                        MethodInfo getDataGridViewShowMethod = item.GetType().GetMethod("GetDataGridViewShow");
                        var _data = getDataGridViewShowMethod.Invoke(item, null);
                        list.Add(_data);
                    }

                    var _list = list.Where(x => x.GetType().GetProperties().Any(y => y.Name == name && y.GetValue(x, null).ToString() == value)).ToArray();

                    DataRow dr;
                    foreach (var item in _list)
                    {
                        dr = dataTable.NewRow();
                        foreach (PropertyInfo _item in item.GetType().GetProperties())
                        {
                            try
                            {
                                var obj = item.GetType().GetProperty(_item.Name).GetValue(item, null);
                                dr[_item.Name] = obj;
                            }
                            catch
                            {

                            }
                        }
                        dataTable.Rows.Add(dr);
                    }

                    dgvGrid.DataSource = dataTable;
                    #endregion
                }
                else
                {
                    if (datas.Count() > 0)
                    {
                        List<Object> list = new List<object>();
                        foreach (var item in datas)
                        {
                            MethodInfo getDataGridViewShowMethod = item.GetType().GetMethod("GetDataGridViewShow");
                            var _data = getDataGridViewShowMethod.Invoke(item, null);
                            list.Add(_data);
                        }
                        dgvGrid.DataSource = list;
                    }
                }
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cmbEncodingTypes_SelectedIndexChanged(null, null);
        }

        private void cmsTasks_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            timer1.Start();
        }

        private void cmsLocationCurrentTasks_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            timer1.Start();
        }

        private void _lastSentBytesTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var conveyor = (ConveyorDevice)_device;
                var bytes = conveyor.GetLastSentBytes();
                if (bytes == null)
                {
                    tbxLastSentBytesLenght.Text = "<Null>";
                    tbxLastSentBytes.Text = "";
                }
                else
                {
                    tbxLastSentBytesLenght.Text = bytes.Length.ToString();

                    var content = "";
                    for (int i = 0; i < bytes.Length; i += 16)
                    {
                        content += String.Join(" ", bytes.Skip(i).Take(16).Select(x => x.ToString("x2"))) + "\r\n";
                    }

                    tbxLastSentBytes.Text = content;
                }
            }
            catch (Exception ex)
            {
                tbxLastSentBytes.Text = ex.ToString();
            }
        }

        RequestBlock currentSelectedHoldSingal
        {
            get
            {
                if (lvZW.SelectedItems.Count != 1)
                {
                    return null;
                }

                return lvZW.SelectedItems[0].Tag as RequestBlock;
            }
        }
        private void cmsZW_Opening(object sender, CancelEventArgs e)
        {
            timer1.Stop();
            tsmiDeleteZW.Enabled = currentSelectedHoldSingal != null;
        }

        private void cmsZW_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            timer1.Start();
        }

        private void tsmiDeleteZW_Click(object sender, EventArgs e)
        {
            var hs = currentSelectedHoldSingal;
            if (hs == null)
            {
                return;
            }

            if (MessageBox.Show(String.Format("是确定要手动删除 {0} 上的占位信号吗？\n警告：\n如果手动删除了一些与业务绑定的占位，可能引起设备错误的动作。", hs.PosNo), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ClearRequestCommand cmd = new ClearRequestCommand(hs.PosNo);

                _device.Write(cmd, (device, _cmd) =>
                {
                    ConveyorDevice conveyorDevice = (ConveyorDevice)device;
                    RequestBlock requestBlock = null;
                    if (conveyorDevice.RequestBlocks != null)
                        requestBlock = conveyorDevice.RequestBlocks.FirstOrDefault(x => x.PosNo == hs.PosNo);
                    return requestBlock != null && requestBlock.HandShake != RequestHandShakes.New;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("手动删除 {0} 上的占位失败,{1}", hs.PosNo, ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void llab_start_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(cbx_areaNo.Text))
                        {
                            MessageBox.Show("请选择即将操作的区域");
                            return;
                        }
                        var result = MessageBox.Show($"即将给PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程启动 命令，请确认是否继续？", "TIPS:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result != DialogResult.Yes)
                        {
                            MessageBox.Show("已取消发送");
                            return;
                        }
                        RemoteCMDCommand cmd = new RemoteCMDCommand()
                        {
                            AreaNo = UInt16.Parse(cbx_areaNo.Text),
                            Start = true,
                            Reset = false,
                            Stop = false
                        };
                        _device.Write(cmd, cmd.SendSuccess);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程启动 命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程启动 命令发送失败，异常消息：{ex}");
                    }
                }));

            }, null);
        }

        private void llab_reset_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(cbx_areaNo.Text))
                        {
                            MessageBox.Show("请选择即将操作的区域");
                            return;
                        }
                        var result = MessageBox.Show($"即将给PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程复位 命令，请确认是否继续？", "TIPS:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result != DialogResult.Yes)
                        {
                            MessageBox.Show("已取消发送");
                            return;
                        }
                        RemoteCMDCommand cmd = new RemoteCMDCommand()
                        {
                            AreaNo = UInt16.Parse(cbx_areaNo.Text),
                            Start = false,
                            Reset = true,
                            Stop = false
                        };
                        _device.Write(cmd, cmd.SendSuccess);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程复位 命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程复位 命令发送失败，异常消息：{ex}");
                    }
                }));

            }, null);
        }

        private void llab_stop_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(cbx_areaNo.Text))
                        {
                            MessageBox.Show("请选择即将操作的区域");
                            return;
                        }
                        var result = MessageBox.Show($"即将给PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程停止 命令，请确认是否继续？", "TIPS:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result != DialogResult.Yes)
                        {
                            MessageBox.Show("已取消发送");
                            return;
                        }
                        RemoteCMDCommand cmd = new RemoteCMDCommand()
                        {
                            AreaNo = UInt16.Parse(cbx_areaNo.Text),
                            Start = false,
                            Reset = false,
                            Stop = true
                        };
                        _device.Write(cmd, cmd.SendSuccess);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程停止 命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        MessageBox.Show($"PLC {_device.Name} 区域 {cbx_areaNo.Text} 发送 远程停止 命令发送失败，异常消息：{ex}");
                    }
                }));

            }, null);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show($"此功能为直接手动发送PLC报文命令，供专业人员在测试、调试、手动调整PLC数据时使用，请确认！", "一次确认:", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            if (MessageBox.Show($"此功能为直接手动发送PLC报文命令，供专业人员在测试、调试、手动调整PLC数据时使用，请确认！", "二次确认:", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            if (MessageBox.Show($"此功能为直接手动发送PLC报文命令，供专业人员在测试、调试、手动调整PLC数据时使用，请确认！", "三次确认:", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            _logger.Info1($"即将启用设备 {_device.Name} 的手动命令发送功能。", this);

            ConveyorManualCommand frm = new ConveyorManualCommand(_device);
            frm.ShowDialog();
        }

        private void cbx_property_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbEncodingTypes.SelectedItem == null)
                return;
            if (string.IsNullOrWhiteSpace(cbx_property.Text))
                return;
            if (string.IsNullOrWhiteSpace(tbx_propertyValue.Text))
                return;

            cmbEncodingTypes_SelectedIndexChanged(null, null);
        }

        private void cmbEncodingTypes_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbEncodingTypes.Text))
                return;
            Type type = ((_item)cmbEncodingTypes.SelectedItem).type;
            object obj = Activator.CreateInstance(type, true);//根据类型创建实例
            MethodInfo methodInfo = obj.GetType().GetMethod("GetDataGridViewShow");
            var showObj = methodInfo.Invoke(obj, null);
            cbx_property.Items.Clear();
            cbx_property.Items.AddRange(showObj.GetType().GetProperties().Select(x => x.Name).ToArray());
        }

        private void tbx_propertyValue_TextChanged(object sender, EventArgs e)
        {
            if (cmbEncodingTypes.SelectedItem == null)
                return;
            if (string.IsNullOrWhiteSpace(cbx_property.Text))
                return;
            if (string.IsNullOrWhiteSpace(tbx_propertyValue.Text))
                return;

            cmbEncodingTypes_SelectedIndexChanged(null, null);
        }
    }
}
