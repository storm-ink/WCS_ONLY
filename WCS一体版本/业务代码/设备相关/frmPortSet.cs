using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;

namespace ZHQXC.设备相关
{
    public partial class frmPortSet : Form
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        List<string> devices = new List<string>() { "专机CV" };
        public frmPortSet()
        {
            InitializeComponent();
        }

        List<SourceCollection> sources = new List<SourceCollection>();
        private void frmPortSet_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                //读取设备状态 
                bool update = false;
                foreach (var item in devices)
                {
                    var device = DeviceConverter.ToDevice<ConveyorDevice>(item);
                    var blocks = device.ReadStatus<DefaultLocationBlock>();
                    foreach (var block in blocks)
                    {
                        if (block.PosNo == 0)
                            continue;
                        string state = "未获取";
                        if (block.HomePos == 0)
                            state = "未设置";
                        else if (block.HomePos == 1)
                            state = "入库";
                        else if (block.HomePos == 1)
                            state = "出库";
                        var source = sources.FirstOrDefault(x => x.DeviceCode == block.PosNo.ToString());
                        if (source == null)
                        {
                            source = new SourceCollection() { Device = item, DeviceCode = block.PosNo.ToString(), State = state };
                            sources.Add(source);
                            update = true;
                        }
                        else if (source.State != state)
                        {
                            source.State = state;
                            update = true;
                        }
                        else
                            continue;
                    }
                }

                if (update)
                {
                    dg.DataSource = new List<SourceCollection>();
                    dg.DataSource = sources;
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show($"加载数据失败，异常消息:{ex}");
            }
        }

        private void dg_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查点击的是否是按钮列
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var row = dg.Rows[e.RowIndex];
                var deviceCode = row.Cells["Column1"].Value.ToString();
                var device = row.Cells["Column5"].Value.ToString();
                var state = row.Cells["Column2"].Value.ToString();
                if (MessageBox.Show($"是否直接切换 {deviceCode} 的状态？\r\n直接切换按钮是用于测试或者纠正现场数据使用，正常业务使用中请使用 请求切换 按钮", "操作提示：第一次确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    if (MessageBox.Show($"是否直接切换 {deviceCode} 的状态？\r\n直接切换按钮是用于测试或者纠正现场数据使用，正常业务使用中请使用 请求切换 按钮", "操作提示：第二次确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (MessageBox.Show($"是否直接切换 {deviceCode} 的状态？\r\n直接切换按钮是用于测试或者纠正现场数据使用，正常业务使用中请使用 请求切换 按钮", "操作提示：第三次确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            //await Task.Run(() => HandOff());
                            System.Threading.Tasks.Task.Run(() => HandOff_plc(deviceCode, device, state));
                        else
                            MessageBox.Show($"用户取消操作");
                    else
                        MessageBox.Show($"用户取消操作");
                else
                    MessageBox.Show($"用户取消操作");
            }
            // 检查点击的是否是按钮列
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                var row = dg.Rows[e.RowIndex];
                var deviceCode = row.Cells["Column1"].Value.ToString();
                var device = row.Cells["Column5"].Value.ToString();
                var state = row.Cells["Column2"].Value.ToString();
                if (MessageBox.Show($"是否请求切换 {deviceCode} 的状态？", "操作提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    ///操作
                    System.Threading.Tasks.Task.Run(() => HandOff_wms(deviceCode, device, state));
                else
                    MessageBox.Show($"用户取消操作");
            }
        }

        private void HandOff_plc(string deviceCode,string device, string state)
        {
            try
            {
                var _device = DeviceConverter.ToDevice<ConveyorDevice>(device);
                if (!_device.IsConnected)
                {
                    MessageBox.Show("设备未连接");
                    return;
                }
                var cmd = new SetDefaultLocationCommand();
                cmd.PosNo = ushort.Parse(deviceCode);
                string _state;
                if (state != "入库")
                {
                    cmd.HomePos = 1;
                    _state = "入库";
                }
                else
                {
                    cmd.HomePos = 2;
                    _state = "出库";
                }
                _device.Write<SetDefaultLocationCommand>(cmd, cmd.SendSuccess);

                
                //this.Invoke(new Action(() =>
                //{
                //    dg.DataSource = new List<SourceCollection>();
                //    dg.DataSource = sources;
                //}));

                var msg = $"直接切换 {deviceCode} 状态 由 {state} 切换为 {_state}操作完成";
                _logger.Info1(msg, this);
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show($"直接切换 {deviceCode} 状态操作时发生异常，异常消息：{ex}");
            }
        }

        private void HandOff_wms(string deviceCode,string device, string state)
        {
            try
            {
                bool request = false;
                //请求WMS

                if (request)
                {
                    var _device = DeviceConverter.ToDevice<ConveyorDevice>(device);
                    if (!_device.IsConnected)
                    {
                        MessageBox.Show("设备未连接");
                        return;
                    }
                    var cmd = new SetDefaultLocationCommand();
                    cmd.PosNo = ushort.Parse(deviceCode);
                    string _state;
                    if (state != "入库")
                    {
                        cmd.HomePos = 1;
                        _state = "入库";
                    }
                    else
                    {
                        cmd.HomePos = 2;
                        _state = "出库";
                    }
                    _device.Write<SetDefaultLocationCommand>(cmd, cmd.SendSuccess);

                    var msg = $"请求切换 {deviceCode} 状态 由 {state} 切换为 {_state}操作完成";
                    _logger.Info1(msg, this);
                    MessageBox.Show(msg);
                }
                else
                {
                    MessageBox.Show($"请求WMS切换失败，请查看WMS提示");
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show($"请求切换 {deviceCode} 状态操作时发生异常，异常消息：{ex}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadData();
        }
    }

    public class SourceCollection
    {
        public string DeviceCode { get; set; }

        public string Device { get; set; }

        public string State { get; set; }

        public string Hand0 { get; set; } = "直接切换";

        public string Hand1 { get; set; } = "请求切换";
    }
}
