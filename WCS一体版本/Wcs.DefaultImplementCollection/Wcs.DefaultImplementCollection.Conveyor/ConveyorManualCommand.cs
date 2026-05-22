using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public partial class ConveyorManualCommand : Form
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        ConveyorDevice _device;
        DefaultConveyorTcpProtocolDataReceiver _receiver;
        public ConveyorManualCommand()
        {
            InitializeComponent();
        }

        public ConveyorManualCommand(ConveyorDevice conveyor)
            : this()
        {
            _device = conveyor;
            _receiver = (DefaultConveyorTcpProtocolDataReceiver)_device.DataReceiver;
        }

        private void ConveyorManualCommand_Load(object sender, EventArgs e)
        {
            var items = _receiver.SenderEncoding._collectionSettings.Select(x => new _item(x.type)).ToArray();
            cbx_send.Items.AddRange(items);
            items = _receiver.ReceiverEncoding._collectionSettings.Select(x => new _item(x.type)).ToArray();
            cbx_received.Items.AddRange(items);
        }

        private void cbx_send_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var type = ((_item)cbx_send.SelectedItem).type;
                var xmlNode = _receiver.SenderEncoding._collectionSettings.Single(x => x.type.Equals(type)).xmlNode;
                tbx_send.Text = xmlNode.FormatXml();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"手动发送命令失败，异常信息:{ex.Message}");
            }
        }

        private void cbx_received_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbx_received.SelectedItem == null)
                {
                    cbx_property.Items.Clear();
                    return;
                }
                var type = ((_item)cbx_received.SelectedItem).type;
                var cs = _receiver.ReceiverEncoding._collectionSettings.Single(x => x.type.Equals(type));
                tbx_received.Text = cs.xmlNode.FormatXml();
                cbx_property.Items.Clear();
                cbx_property.Items.AddRange(cs.propertys);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"手动发送命令失败，异常信息:{ex.Message}");
            }
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbx_send.SelectedItem == null)
                    return;

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(tbx_send.Text);

                Type type = ((_item)cbx_send.SelectedItem).type;

                var cmd = ReflectionHelper.CreateInstance<DeviceCommand>(type);
                cmd = cmd.Create(cmd, xml);
                var _cmd = (IDeviceCommandAdjudicator)cmd;

                _logger.Info1($"即将给 {_device.Name} 输送线，发送手动命令：{cmd} ", this);
                if (MessageBox.Show($"即将给 {_device.Name} 输送线，发送手动命令：{cmd}！请确认！\r\n该功能请在专业人士指导下使用！！！", "Tips:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _device.Write(cmd, _cmd.SendSuccess);
                    MessageBox.Show($"{_device.Name} 输送线，手动命令：{cmd} 发送成功！");
                    _logger.Info1($"{_device.Name} 输送线，手动命令：{cmd} 发送成功！", this);
                }
                else
                {
                    MessageBox.Show($"{_device.Name} 输送线，手动命令：{cmd} 取消发送！");
                    _logger.Info1($"{_device.Name} 输送线，手动命令：{cmd} 取消发送！", this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"手动发送命令失败，异常消息：{ex.Message}");
                _logger.Error1(ex, this);
            }
        }

        private void chb_restart_CheckedChanged(object sender, EventArgs e)
        {
            if (chb_restart.Checked)
            {
                timer1.Start();
                cbx_received.Enabled = false;
                cbx_property.Enabled = false;
                tbx_propertyValue.Enabled = false;
            }
            else
            {
                timer1.Stop();
                cbx_received.Enabled = true;
                cbx_property.Enabled = true;
                tbx_propertyValue.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cbx_received.Text) || string.IsNullOrWhiteSpace(cbx_property.Text) || string.IsNullOrWhiteSpace(tbx_propertyValue.Text))
                {
                    chb_restart.Checked = false;
                    return;
                }

                Type type = ((_item)cbx_received.SelectedItem).type;
                MethodInfo readStatusMethodInfo = typeof(ConveyorDevice).GetMethod("ReadStatus");
                readStatusMethodInfo = readStatusMethodInfo.MakeGenericMethod(type);
                var datas = readStatusMethodInfo.Invoke(_device, null);
                var _datas = datas as object[];
                if (_datas.Length == 0)
                {
                    tbx_received.Text = $"<{DateTime.Now.ToString("yyyyy-MM-dd HH:mm:ss.ffff")}\r\n未读取到源数据>";
                    if (chb_WriteToFile.Checked)
                        LogWriteToFileHelper.log(tbx_received.Text, $"{_device.Name}\\手动测试记录");
                    return;
                }
                var name = cbx_property.Text.Trim();
                var value = tbx_propertyValue.Text.Trim();
                _datas = _datas.Where(x => x.GetType().GetProperties().Any(y => y.Name == name && y.GetValue(x, null).ToString() == value)).ToArray();
                tbx_received.Text = $"{DateTime.Now.ToString("yyyyy-MM-dd HH:mm:ss.ffff")}\r\n";
                if (_datas.Length > 0)
                {
                    var cs = _receiver.ReceiverEncoding._collectionSettings.Single(x => x.type.Equals(type));
                    foreach (var item in _datas)
                    {
                        XmlDocument xd = new XmlDocument();
                        xd.LoadXml(cs.xmlNode.FormatXml());
                        var properInfos = item.GetType().GetProperties();
                        foreach (XmlNode childNode in xd.ChildNodes)
                        {
                            foreach (XmlNode _item in childNode.ChildNodes)
                            {
                                var porperInfo = properInfos.FirstOrDefault(x => x.Name == _item.Attributes["name"].Value);
                                if (porperInfo != null)
                                    _item.InnerText = porperInfo.GetValue(item, null).ToString();
                            }
                        }
                        tbx_received.Text += $"{xd.FormatXml()}\r\n";
                    }
                }
                else
                    tbx_received.Text += "未检索到值";

                if (chb_WriteToFile.Checked)
                    LogWriteToFileHelper.log(tbx_received.Text, $"手动测试记录\\{_device.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                tbx_received.Text = $"<{DateTime.Now.ToString("yyyyy-MM-dd HH:mm:ss.ffff")}获取数据发生异常，异常消息：\r\n{ex.Message}>";
                chb_restart.Checked = false;
                timer1.Stop();
                if (chb_WriteToFile.Checked)
                    LogWriteToFileHelper.log(tbx_received.Text, $"{_device.Name}\\手动测试记录");
            }
        }
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
}
