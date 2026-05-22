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
using Wcs.Framework;
using Wcs.DefaultImpls;
using NLog;
using System.Reflection;

namespace BOE
{
    public partial class ElevatorDeviceUI : Form,IDeviceUserInterface
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        ElevatorDevice _elevatorDevice;
        public ElevatorDeviceUI()
        {
            InitializeComponent();
        }

        public void Show(Device device)
        {
            _elevatorDevice = (ElevatorDevice)device;

            this.Text = string.Format("查看 {0} 的状态", this._elevatorDevice.Name);

            displayInfo();

            this.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                displayInfo();
            }
            catch (Exception ex)
            {

                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            timer1.Start();
        }

        void displayInfo()
        {
            groupBox2.Text = string.Format("状态信息（{0}）", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            groupBox1.Enabled = true;
            

          
            lblWarn.Text = string.Join("\r\n", _elevatorDevice.Warnings);
            var type = typeof(HoistNetTransferObject);


       
               
            lblPosNo.Text = _elevatorDevice.PosNo;
           

            lblDeviceName.Text = _elevatorDevice.Name;
            lblDeviceType.Text = "提升机";
            lblStatus.Text = _elevatorDevice.IsConnected ? "已连接" : "未连接";
            lbl_OwnerConveyor.Text = _elevatorDevice.OwnerConveyorDevice.Name;
            lbl_index.Text = _elevatorDevice.No.ToString();

            //提升机设备附加在输送线上
            var status = _elevatorDevice.OwnerConveyorDevice.ReadStatus<HoistNetTransferObject>()
                .Where(x => x.AtPacketIndex == _elevatorDevice.No)
                .FirstOrDefault();
              
            foreach (var pi in type.GetProperties().Where(x => x.GetIndexParameters().Length == 0))
            {
                Label contentLabel = (Label)flowLayoutPanel1.Controls.Cast<Control>()
                    .Where(x => x is Label)
                    .FirstOrDefault(x => x.Name == pi.Name);

                if (contentLabel == null)
                {
                    int width = 100;
                    Label titleLabel = new Label();
                    titleLabel.Font = new System.Drawing.Font(titleLabel.Font, FontStyle.Bold);
                    titleLabel.AutoSize = false;
                    titleLabel.Width = width;
                    titleLabel.Text = pi.GetDisplayName();


                    titleLabel.BorderStyle = BorderStyle.Fixed3D;
                

                    flowLayoutPanel1.Controls.Add(titleLabel);

                    contentLabel = new Label();
                    contentLabel.AutoSize = false;
                    contentLabel.Width = width;
                    contentLabel.Name = pi.Name;

                    flowLayoutPanel1.Controls.Add(contentLabel);
                }

                if (status == null)
                {
                    contentLabel.Text = "-";
                }
                else
                {
                    var v = pi.GetValue(status, null);

                    if (pi.PropertyType.IsEnum)
                    {
                        contentLabel.Text = getEnumDisplayName(pi, v);
                    }
                    else
                    {

                        contentLabel.Text = Convert.ToString(v);
                    }
                }
            }
        }

        String getEnumDisplayName(PropertyInfo pi, Object v)
        {
            var type = typeof(Wcs.EnumExtentions);

            var mi = type.GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public);
         
            mi = mi.MakeGenericMethod(new Type[] { pi.PropertyType });

            var r = mi.Invoke(null, new object[] { v });

            return Convert.ToString(r);
        }

        private void ElevatorDeviceUI_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
        }

      
    }
}
