
using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Wcs.Framework;
using Newtonsoft.Json;


namespace Wcs.DefaultImplementCollection.Robot
{
    public partial class RobotDeviceUserInterface : Form, IDeviceUserInterface
    {
        RobotDevice _device;
        Logger _logger = LogManager.GetCurrentClassLogger();

        public RobotDeviceUserInterface(RobotDevice device)
        {
            _device = device;
            InitializeComponent();
            lblDeviceName.Text = _device.Name;
        }

        public void Show(Device device)
        {
            _device = (RobotDevice)device;
            this.Text = string.Format("查看 {0} 的状态", _device.Name);

            this.ShowDialog();
        }

        //BindingList<BoxInfo_Received_Show> boxInfos = new BindingList<BoxInfo_Received_Show>();

        //BindingList<RobotStationStateShow> stations = new BindingList<RobotStationStateShow>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                groupBox4.Text = $"实时信息{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}";
                if (_device.Locker.IsEmpty)
                {
                    btnLock.Enabled = true;
                    btnUnlock.Enabled = false;
                    groupBox5.Enabled = false;
                    cbx_loadTask.Enabled = false;
                }
                else
                {
                    btnLock.Enabled = false;
                    btnUnlock.Enabled = true;
                    groupBox5.Enabled = true;
                    cbx_loadTask.Enabled = true;
                }
                lbl_ConnState.Text = _device.IsConnected ? "已连接" : "未连接";
                tbxWarnings.Text = string.Join("/", _device.Warnings);



                if (_device.LastState != null && _device.IsConnected)
                {
                    lblTaskId.Text = _device.LastState.RobotTask.TaskId.ToString();
                    lblStatus.Text = _device.LastState.RobotBasicMessage.Mode.GetDescription();
                    lblIsAlarm.Text = Convert.ToBoolean(_device.LastState.RobotBasicMessage.Alarm).ToString().ToUpper();
                    var robotBasicMessageShow = new RobotBasicMessageShow(_device.LastState.RobotBasicMessage);
                 
                    var alarmList = _device.LastState.AlarmList.Where(x => x != 0).ToList();
                    if (alarmList.Count() > 0)
                        robotBasicMessageShow.Alarms = string.Join("/", alarmList);
                    else
                        robotBasicMessageShow.Alarms = "0";

                    tbx_state.Text = $"{robotBasicMessageShow.XmlSerialize<RobotBasicMessageShow>()}";
              


                    var robotTaskTransferObjectShow = new RobotTaskTransferObjectShow(_device.LastState.RobotTask);
                    tbx_task.Text = $"{robotTaskTransferObjectShow.XmlSerialize<RobotTaskTransferObjectShow>()}";


                }
                else
                {
                    lblTaskId.Text = "";
                    lblStatus.Text = RobotModes.UnKnown.GetDescription();
                    lblIsAlarm.Text = "FALSE";
                    tbx_state.Text = $"未收到数据";
                    tbx_task.Text = $"未收到数据";
                  
                    _device.LastState = null;

                }

            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            try
            {
                _device.Lock(new LockerInfo(System.Environment.MachineName, LockerInfo.GetIpAddress()));
                btnLock.Enabled = false;
                btnUnlock.Enabled = true;
                groupBox3.Enabled = true;

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
                btnLock.Enabled = true;
                btnUnlock.Enabled = false;
                groupBox3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_manual_Click(object sender, EventArgs e)
        {
            ManualTest manualTest = new ManualTest(this._device);
            manualTest.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //try
            //{
            //    RobotRemoteCommand cmd = new RobotRemoteCommand(RemoteCommands.Emergency, 0, 0);
            //    _device.Write<RobotRemoteCommand>(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error1(ex, "手动发送急停命令失败", this);
            //    MessageBox.Show($"远程命令发送失败，异常消息：{ex.Message}");
            //}
        }

        private void RobotDeviceUserInterface_Load(object sender, EventArgs e)
        {
            lab_IPAddress.Text = _device.IPEndPoint.Address.ToString() + ":" + _device.IPEndPoint.Port.ToString();

            if (_device.Locker != null && !_device.Locker.IsEmpty)
            {
                btnLock.Enabled = false;
                btnUnlock.Enabled = true;
            }
            else
            {
                btnLock.Enabled = true;
                btnUnlock.Enabled = false;
            }


       
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {


                #region 箱任务完成更新
                //var brs = _device.LastState.RobotTask.BoxInfo.Where(x => x.State != 0);
                //if (brs.Count() > 0)
                //{
                //    Task task = null;
                //    if (_device.EquipmentActionToAddCommandPlugin != null)
                //        _device.EquipmentActionToAddCommandPlugin.EquipmentActionUpdatePlugin(ref task, brs);
                //}
                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                RobotTaskClearCommand cmd = new RobotTaskClearCommand();
                _device.Write<RobotTaskClearCommand>(cmd, cmd.SendSuccess);
                MessageBox.Show("清除任务发送成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送失败，异常消息:{ex.Message}");
            }
        }





        private void 初始化ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void wCS完成ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 异常ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            //try
            //{
            //    RobotRemoteCommand cmd = new RobotRemoteCommand(RemoteCommands.CancleEmergency, 0, 0);
            //    _device.Write<RobotRemoteCommand>(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error1(ex, "手动发送急停命令失败", this);
            //    MessageBox.Show($"远程命令发送失败，异常消息：{ex.Message}");
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    RobotRemoteCommand cmd = new RobotRemoteCommand(RemoteCommands.ReqLock, ushort.Parse(textBox1.Text), ushort.Parse(textBox2.Text));
            //    _device.Write<RobotRemoteCommand>(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error1(ex, "手动发送急停命令失败", this);
            //    MessageBox.Show($"远程命令发送失败，异常消息：{ex.Message}");
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    RobotRemoteCommand cmd = new RobotRemoteCommand(RemoteCommands.ReqUnLock, ushort.Parse(textBox1.Text), ushort.Parse(textBox2.Text));
            //    _device.Write<RobotRemoteCommand>(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error1(ex, "手动发送急停命令失败", this);
            //    MessageBox.Show($"远程命令发送失败，异常消息：{ex.Message}");
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    RobotRemoteCommand cmd = new RobotRemoteCommand(RemoteCommands.CanclePause, 0, 0);
            //    _device.Write<RobotRemoteCommand>(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error1(ex, "手动发送急停命令失败", this);
            //    MessageBox.Show($"远程命令发送失败，异常消息：{ex.Message}");
            //}
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    //public class RobotStationStateShow
    //{
    //    public RobotStationStateShow(RobotStationState robotStationState)
    //    {
    //        //this.StationNo = robotStationState.StationNo;
    //        //if (robotStationState.StationType == 0)
    //        //    this.StationType = $"未知{robotStationState.StationType}";
    //        //else if (robotStationState.StationType == 1)
    //        //    this.StationType = $"码垛{robotStationState.StationType}";
    //        //else if (robotStationState.StationType == 2)
    //        //    this.StationType = $"拆垛{robotStationState.StationType}";
    //        //else
    //        //    this.StationType = $"({robotStationState.StationType})";
    //        //this.Total = robotStationState.Total;
    //        //this.CVToRobotSingle = string.Join("", robotStationState.CVToRobotSingle);
    //        //this.RobotToCVSingle = string.Join("", robotStationState.RobotToCVSingle);
    //    }
    //    /// <summary>
    //    /// 站点编号
    //    /// </summary>
    //    public UInt16 StationNo { get; set; }
    //    /// <summary>
    //    /// 站点类型（0未知，1码垛，2拆垛）
    //    /// </summary>
    //    public string StationType { get; set; }
    //    /// <summary>
    //    /// Box数量
    //    /// </summary>
    //    public UInt16 Total { get; set; }
    //    /// <summary>
    //    /// CV给Robot的干接点信息
    //    /// </summary>
    //    public string CVToRobotSingle { get; set; }
    //    /// <summary>
    //    /// Robot给CV的干接点信息
    //    /// </summary>
    //    public string RobotToCVSingle { get; set; }
    //}

    public class RobotBasicMessageShow
    {
        public RobotBasicMessageShow()
        { }
        public RobotBasicMessageShow(RobotBasicMessage msg)
        {
            this.编号 = msg.DeviceNO;
            this.工作模式 = msg.Mode.GetDescription();
            this.是否报警 = msg.Alarm == 1 ? "是" : "否";
            this.原点 = msg.Origin == 1 ? "是" : "否";
            //this.CV是否可以动作 = msg.CVMove == 1 ? "是" : "否";
            this.是否抓取 = msg.Catch == 1 ? "是" : "否";
            this.是否抓取 = msg.Catch == 1 ? "是" : "否";
            //this.抓取编号 = msg.SeriNo;
            //this.速度 = msg.Speed;
            this.已抓次数 = msg.CountTotal;

            //this.A轴位置 = msg.RobotPosition.A_POSITION;
            //this.B轴位置 = msg.RobotPosition.B_POSITION;
            //this.C轴位置 = msg.RobotPosition.C_POSITION;
            this.Alarms = "";
        }
        /// <summary>
        /// 机械手编号
        /// </summary>
        public UInt16 编号 { get; set; }
        /// <summary>
        /// 工作模式上报
        /// </summary>
        public string 工作模式 { get; set; }
        /// <summary>
        /// 是否故障（1故障，0正常）
        /// </summary>
        public string 是否报警 { get; set; }
        /// <summary>
        /// 是否在原点
        /// </summary>
        public string 原点 { get; set; }
        /// <summary>
        /// CV是否可以动作
        /// </summary>
       // public string CV是否可以动作 { get; set; }
        /// <summary>
        /// 机械手是否抓取箱子
        /// </summary>
        public string 是否抓取 { get; set; }


        public UInt16 已抓次数 { get; set; }

        /// <summary>
        /// 本次抓取箱子编号
        /// </summary>
        //public Byte 抓取编号 { get; set; }
        /// <summary>
        /// 速度
        /// </summary>
        //public UInt16 速度 { get; set; }
        /// <summary>
        /// robotX轴位置
        /// </summary>
        //public Int32 X轴位置 { get; set; }
        ///// <summary>
        ///// robotY轴位置
        ///// </summary>
        //public Int32 Y轴位置 { get; set; }
        ///// <summary>
        ///// robotZ轴位置
        ///// </summary>
        //public Int32 Z轴位置 { get; set; }
        /// <summary>
        /// robotA轴位置
        /// </summary>
        //public Int32 A轴位置 { get; set; }
        /// <summary>
        /// robotB轴位置
        /// </summary>
        ///public Int32 B轴位置 { get; set; }
        /// <summary>
        /// robotC轴位置
        /// </summary>
        //public Int32 C轴位置 { get; set; }

        /// <summary>
        /// 报警列表
        /// </summary>
        public string Alarms { get; set; }
    }

    //public class RobotAreaMessageShow
    //{
    //    public RobotAreaMessageShow()
    //    { }
    //    public RobotAreaMessageShow(RobotStationState msg)
    //    {
    //        区域 = msg.Area.ToString();
    //        命令编号 = msg.Area_Cmd_No.ToString();
    //        状态 = msg.Area_IsOK == 1 ? "锁定" : "未锁定";
    //    }

    //    /// <summary>
    //    /// 工作模式上报
    //    /// </summary>
    //    public string 区域 { get; set; }
    //    /// <summary>
    //    /// 是否故障（1故障，0正常）
    //    /// </summary>
    //    public string 命令编号 { get; set; }
    //    /// <summary>
    //    /// 是否在原点
    //    /// </summary>
    //    public string 状态 { get; set; }

    //}


    public class RobotTaskTransferObjectShow
    {
        public RobotTaskTransferObjectShow()
        { }
        public RobotTaskTransferObjectShow(RobotTaskTransferObject robotTaskTransferObject)
        {
            任务号 = robotTaskTransferObject.TaskId.ToString();
            握手 = robotTaskTransferObject.HandShake.GetDescription();
            //是否需要校验 = robotTaskTransferObject.Check == 1 ? "是" : "否";
            取货站台 = robotTaskTransferObject.Pick.ToString();
            放货站台 = robotTaskTransferObject.Put.ToString();
            要抓数量 = robotTaskTransferObject.Count;
            备用 = robotTaskTransferObject.Raw;
            //请求FosbId = $"{robotTaskTransferObject.Size_Length.ToString()}(0-未请求，1-请求)";
            //BOX长度 = robotTaskTransferObject.Size_Length.ToString();
            //BOX宽度 = robotTaskTransferObject.Size_Width.ToString();
            //BOX高度 = robotTaskTransferObject.Size_Height.ToString();
            //垛型 = robotTaskTransferObject.Style.ToString();
            //初始层数 = robotTaskTransferObject.Level.ToString();
            //拆垛位BOX初始数量 = robotTaskTransferObject.PickTotal.ToString();
            //码垛位BOX初始数量 = robotTaskTransferObject.PutTotal.ToString();
            //本次拆垛数量 = robotTaskTransferObject.Count.ToString();
            //拆垛栈板号 = robotTaskTransferObject.PickPalletId;
            //码垛栈板号 = robotTaskTransferObject.PutPalletId;
        }
        /// <summary>
        /// 任务号，wcs和机械手交互唯一标志
        /// </summary>
        public string 任务号 { get; set; }
        /// <summary>
        /// 握手
        /// </summary>
        public string 握手 { get; set; }
        /// <summary>
        /// 是否需要扫码校验（取值范围 true/false 1/0）
        /// </summary>
        /// <summary>
        /// 取货站台
        /// </summary>
        public string 取货站台 { get; set; }
        /// <summary>
        /// 放货站台
        /// </summary>
        public string 放货站台 { get; set; }

        public UInt16 要抓数量 { get; set; }

        public String 备用 { get; set; }

        /// <summary>
        /// Box尺寸-长度
        /// </summary>
        //public string BOX长度 { get; set; }
        /// <summary>
        /// Box尺寸-宽度
        /// </summary>
        //public string BOX宽度 { get; set; }
        /// <summary>
        /// Box尺寸-高度
        /// </summary>
        //public string BOX高度 { get; set; }
        /// <summary>
        /// 垛型（1-n）
        /// </summary>
        //public string 垛型 { get; set; }
        /// <summary>
        /// 层数（初始层数）
        /// </summary>
        //public string 初始层数 { get; set; }
        /// <summary>
        /// 取货站box总数
        /// </summary>
        //public string 拆垛位BOX初始数量 { get; set; }
        /// <summary>
        /// 放货站box总数
        /// </summary>
        //public string 码垛位BOX初始数量 { get; set; }
        /// <summary>
        /// 本次拆垛数量（润西取值范围，0 - 36）
        /// </summary>
        //public string 本次拆垛数量 { get; set; }
        /// <summary>
        /// 20字节 拆垛子托盘条码号（Ascii码值，兼容字母）
        /// </summary>
        //public string 拆垛栈板号 { get; set; }
        /// <summary>
        /// 20字节 码垛子托盘条码号（Ascii码值，兼容字母）
        /// </summary>
        //public string 码垛栈板号 { get; set; }

        /// <summary>
        /// FOSBID传递信号
        /// </summary>
        public string 请求FosbId { get; set; }
    }


    /// <summary>
    /// Robot上报的BoxInfo
    /// </summary>
    //public class BoxInfo_Received_Show
    //{
    //    public BoxInfo_Received_Show(BoxInfo_Received boxInfo_Received)
    //    {
    //        BoxId = boxInfo_Received.BoxId;
    //        Level = boxInfo_Received.Level;
    //        Position = boxInfo_Received.Position;
    //        SerialNo = boxInfo_Received.SerialNo;

    //        if (Enum.TryParse<BoxStatus>(boxInfo_Received.State.ToString(), out BoxStatus state))
    //            State = state.ToString();
    //        else
    //            State = boxInfo_Received.State.ToString();
    //    }

    //    /// <summary>
    //    /// 条码号（Ascii码值，兼容字母）
    //    /// </summary>
    //    public String BoxId { get; set; }
    //    /// <summary>
    //    /// 当前Box所在层
    //    /// </summary>
    //    public UInt16 Level { get; set; }
    //    /// <summary>
    //    /// 在托盘上的编号
    //    /// </summary>
    //    public Byte Position { get; set; }
    //    /// <summary>
    //    /// 编号（boxId在本次拆垛任务中的编号）从1开始 最大值Count字段
    //    /// </summary>
    //    public Byte SerialNo { get; set; }
    //    /// <summary>
    //    /// Box状态（0-初始化，1-未完成，2-自动完成，3-手动完成，4-异常，5-执行中，6-WCS完成）
    //    /// </summary>
    //    public string State { get; set; }
    //}
}
