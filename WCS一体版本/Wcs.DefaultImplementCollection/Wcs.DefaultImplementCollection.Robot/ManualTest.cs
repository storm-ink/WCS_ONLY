using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Wcs.DefaultImplementCollection.Robot
{
    public partial class ManualTest : Form
    {
        RobotDevice _device;
        public ManualTest(RobotDevice device)
        {
            InitializeComponent();
            _device = device;
        }

        byte[] sendedBytes = null;
        RobotTaskCommand cmd;
        string sendedTime;
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                cmd = new RobotTaskCommand();
                if (string.IsNullOrWhiteSpace(tbx_send.Text))
                {
                    cmd.TaskId = 9999;
                    cmd.HandShake = HandShake.New;
                    cmd.Pick = 1;
                    cmd.Put = 2;
                
                    cmd.Count = 11;
                  

                    tbx_send.Text = XmlFileExtentions.XmlSerialize<RobotTaskCommand>(cmd);
                }
                else
                    cmd = XmlFileExtentions.DESerializer<RobotTaskCommand>(new XmlDocument(), tbx_send.Text);

                sendedBytes = cmd.ToTelex();
                sendedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ff.ssss");
                if (checkBox1.Checked)
                    tbx_send_bytes.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes);
                else
                    tbx_send_bytes.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes.Select(x => x.ToString("x2")));

                //_device.Write<RobotTaskCommand>(cmd, cmd.SendSuccess);
                MessageBox.Show("初始化成功，请根据所提供的结构修改合适数据后发送");

                btn_send.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化失败：" + ex.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (sendedBytes != null)
            {
                if (checkBox1.Checked)
                    tbx_send_bytes.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes);
                else
                    tbx_send_bytes.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes.Select(x => x.ToString("x2")));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btn_send.Enabled = false;
            try
            {
                sendedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ff.ssss");
                //if (checkBox1.Checked)
                //    textBox2.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes);
                //else
                //    textBox2.Text = $"{sendedTime}\r\n总计{sendedBytes.Length}字节：\r\n" + string.Join(" ", sendedBytes.Select(x => x.ToString("x2")));
                _device.Write<RobotTaskCommand>(cmd, cmd.SendSuccess);
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ff.ssss");
            try
            {
                if (checkBox2.Checked)
                    return;

                if (_device.LastState != null)
                {
                    var _datetime = _device.LastStateAt.ToString("yyyy-MM-dd HH:mm:ff.ssss");
                    if (checkBox1.Checked)
                        tbx_received_bytes.Text = $"{datetime}({datetime})\r\n总计{_device.LastState.bytes.Length}字节：\r\n" + string.Join(" ", _device.LastState.bytes);
                    else
                        tbx_received_bytes.Text = $"{datetime}({datetime})\r\n总计{_device.LastState.bytes.Length}字节：\r\n" + string.Join(" ", _device.LastState.bytes.Select(x => x.ToString("x2")));
                    tbx_received.Text = $"{datetime}\r\n" + XmlFileExtentions.XmlSerialize<RobotStatusTelexTransferObject>(_device.LastState);
                }
                else
                {
                    tbx_send.Text = $"{datetime}\r\n未收到数据";
                    tbx_received_bytes.Text = $"{datetime}\r\n未收到数据";
                }
            }
            catch (Exception ex)
            {
                tbx_received_bytes.Text = $"{datetime}\r\n{ex.Message}";
            }
        }
    }
}

