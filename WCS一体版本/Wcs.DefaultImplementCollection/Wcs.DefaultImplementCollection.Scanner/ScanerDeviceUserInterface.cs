//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace Wcs.DefaultImplementCollection.Scanner
//{
//    public partial class ScanerDeviceUserInterface : Form
//    {
//        public ScanerDeviceUserInterface()
//        {
//            InitializeComponent();
//        }
//    }
//}



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;
namespace Wcs.DefaultImplementCollection.Scanner
{
    public partial class ScanerDeviceUserInterface : Form, IDeviceUserInterface
    {
        Logger _logger;
        ScanerDevice _device;
        public ScanerDeviceUserInterface(ScanerDevice device)
        {
            _device = device;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        public void ScanerDeviceUserInterface_Load(object sender, EventArgs e)
        {
            timer1_Tick(null, null);
            timer1.Start();

            _device.BarcodeReceived += _device_BarcodeReceived;

            var showScannerManualStr = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("showScannerManual", "");
            if (!showScannerManualStr.Contains(_device.Name))
            {
                btn_ManualScanner.Visible = false;
                lblBarcode.Visible = true;
            }
            else
            {
                btn_ManualScanner.Visible = true;
                lblBarcode.Visible = false;
            }
        }

        void _device_BarcodeReceived(object sender, BarcodeReceivedArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Action<object, BarcodeReceivedArgs> act = (xx, yy) =>
                    {
                        _device_BarcodeReceived(xx, yy);
                    };

                    this.Invoke(act, sender, e);
                }
                else
                {
                    this.lblBarcode.Text = e.Barcode;
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        public void Show(Device device)
        {
            _device = (ScanerDevice)device;
            this.Text = String.Format("查看 {0} 的状态", _device.Name);
            lblLocation.Text = _device.BindingLocation;
            this.ShowDialog();
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            _device.BarcodeReceived -= _device_BarcodeReceived;
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lblDeviceType.Text = _device.GetType().GetDisplayName();
                lblDeviceName.Text = _device.Name;
                lblLockIP.Text = _device.IPEndPoint.ToString();
                lblStatus.Text = _device.IsConnected ? "已连接" : "未连接";
                tbxScanerInfo.Text = String.Join("\r\n", _device.Barcodes.Select(x => x.Value + " " + x.Key).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        DateTime sendAt;
        object objLock = new object();
        private void btn_ManualScanner_Click(object sender, EventArgs e)
        {
            btn_ManualScanner.Enabled = false;
            lock (objLock)
            {
                Action act = () =>
                {
                    try
                    {
                        KEYENCECommand cmd = new KEYENCECommand(_device);
                        sendAt = DateTime.Now;
                        _device.Write(cmd.Bytes, SendSuccess);
                        MessageBox.Show($"{BitConverter.ToString(cmd.Bytes)} 命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    btn_ManualScanner.Enabled = true;
                };

                this.BeginInvoke(act);
            }
        }

        public Boolean SendSuccess()
        {
            //return _device.Barcodes.Any(x => x.Key > sendAt);
            return true;
        }
    }
}

