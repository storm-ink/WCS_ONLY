using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane
{
    public partial class frmResumeCraneTask : Form
    {
        CraneAutomaticTransferWithStepByStepAction _act;
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public frmResumeCraneTask(CraneAutomaticTransferWithStepByStepAction act)
        {
            InitializeComponent();

            _act = act;

            this.Text = String.Format("请验证{0}相关信息", act.DeviceName);

            lblLocation.Text = act.LoadLocation.UserCode;
            var loadLocation = LocationConverter.ToLocation(act.LoadLocation);
            if (loadLocation.Device.Name != act.DeviceName)
            {
                lblLocation.Text = loadLocation.UserCode;
            }
            else
            {
                if (loadLocation.Synonymous.Length > 0)
                {
                    lblLocation.Text = loadLocation.Synonymous.First().UserCode;
                }
                else
                {
                    lblLocation.Text = loadLocation.UserCode;
                }
            }

            setStatus();
        }

        private void rbF1yh_CheckedChanged(object sender, EventArgs e)
        {
            setStatus();
        }

        void setStatus()
        {
            groupBox2.Enabled = rbF1wh.Checked;

            lblForkBarcode.Enabled =
                tbxForkBarcode.Enabled = rbF1yh.Checked;


            if ((tbxForkBarcode.Enabled && String.IsNullOrWhiteSpace(tbxForkBarcode.Text))
                ||
                (rbF1wh.Checked == false && rbF1yh.Checked == false && rbF1wh.Checked == false && rbF1yh.Checked == false)
                )
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            var craneDevice = DeviceConverter.ToDevice<CraneDevice>(_act.DeviceName);
            var la = craneDevice.LastStatus;
            if (la == null)
            {
                _logger.Warn1(string.Format("验证失败，堆垛机{0}状态获取失败。", craneDevice), this, _act);
                MessageBox.Show(string.Format("堆垛机{0}状态获取失败,请稍后重试。", craneDevice), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!验证载货台(craneDevice, la, _act, rbF1yh.Checked, tbxForkBarcode.Text.Trim(), tbxLocationBarcode.Text.Trim()))
            {
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        bool 验证载货台(CraneDevice craneDevice, CraneReportInfo la, EquipmentAction act,Boolean 载货台是否有货,String forkBarcode, String locBarcode) 
        {
            if (载货台是否有货)
            {
                if (la.IsLoaded != 1)
                {
                    MessageBox.Show(string.Format("堆垛机{0}货台上当前实际检测到并无货物。\n若信息无误，则表示货物和任务已经不符，请人工将货物调整正确。", craneDevice), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (String.IsNullOrWhiteSpace(forkBarcode))
                {
                    MessageBox.Show("请输入载货台上的货物条码（容器编码）", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (
                    !act.Movement.Task.ContainerCodes.Any(x => string.Equals(x, forkBarcode, StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    _logger.Warn1(string.Format("验证失败，用户输入的货台上的货物条码为{0}，不包含在系统登记的{1}当中。", forkBarcode, String.Join(",", act.Movement.Task.ContainerCodes.ToArray())), this, act);
                    MessageBox.Show("验证失败，货台上上登记的货物和输入的条码值不符。\n若填写的信息无误，则表示货物和任务已经不符，请人工将货物调整正确。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                _logger.Info1(string.Format("载货台验证成功，用户输入的条码为{0},系统登记的条码为{1}。", forkBarcode, String.Join(",", act.Movement.Task.ContainerCodes.ToArray())), this, act);
            }
            else
            {
                if (la.IsLoaded == 1)
                {
                    MessageBox.Show(string.Format("堆垛机{0}货台上当前实际检测到有货。\n若填写的信息无误，则表示货物和任务已经不符，请人工将货物调整正确。", craneDevice), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (String.IsNullOrWhiteSpace(locBarcode))
                {
                    MessageBox.Show("请输入取货位上的货物条码（容器编码）", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (
                    !act.Movement.Task.ContainerCodes.Any(x => string.Equals(x, locBarcode, StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    _logger.Warn1(string.Format("验证失败，用户输入的取货位货物条码为{0}，不包含在系统登记的{1}当中。", locBarcode, String.Join(",", act.Movement.Task.ContainerCodes.ToArray())), this, act);
                    MessageBox.Show("验证失败，取货位上登记的货物和输入的条码值不符，请仔细核对后重试。\n若填写的信息无误，则表示货物和任务已经不符，请人工将货物调整正确。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                _logger.Info1(string.Format("取货位验证成功，用户输入的条码为{0},系统登记的条码为{1}。", locBarcode, String.Join(",", act.Movement.Task.ContainerCodes.ToArray())), this, act);

            }

            return true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void tbxF1Barcode_TextChanged(object sender, EventArgs e)
        {
            setStatus();
        }
    }
}
