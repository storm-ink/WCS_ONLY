using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wcs;

namespace WCS.APP
{
    public partial class frmCloseReasonDialog : Form
    {
        IWcsApplication _application;
        public frmCloseReasonDialog(IWcsApplication application)
        {
            _application = application;
            InitializeComponent();

            label1.Text = string.Format("您正在关闭仓库设备控制系统(v{0})...", Assembly.GetExecutingAssembly().GetName().Version);
            setButtonsEnable();

            tbxOperators.Text = Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name;
        }

        void setButtonsEnable()
        {
            btnAccept.Enabled = !(string.IsNullOrWhiteSpace(tbxDescription.Text)
                || string.IsNullOrWhiteSpace(cbxReason.Text)
                || tbxDescription.Text.Trim().Length < 10);
        }

        private void tbxOperators_TextChanged(object sender, EventArgs e)
        {
            setButtonsEnable();
        }

        private void cbxReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            setButtonsEnable();
        }

        private void tbxDescription_TextChanged(object sender, EventArgs e)
        {
            setButtonsEnable();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                Wcs.Framework.ExecutiveState.ApplicationExiting = true;

                string msg = string.Format(@"您正在关闭仓库设备控制系统(v{0})...
本次操作人员：{1}
关闭的原因选项：{2}
注释：{3}", Assembly.GetExecutingAssembly().GetName().Version
           , tbxOperators.Text
           , cbxReason.Text
           , tbxDescription.Text);

                _application.Logger.Info1(msg, this);

                System.Threading.Thread.Sleep(200);

                this.Enabled = false;

                lblWaitings.Text = String.Format("正在等待 {0} 个工作线程退出...", Wcs.Framework.ExecutiveState.ActiveCounters.Count(x => x.Value != 0));
                while (Wcs.Framework.ExecutiveState.ActiveCounters.Any(x => x.Value != 0))
                {
                    lblWaitings.Visible = true;

                    lblWaitings.Text = String.Format("正在等待 {0} 个工作线程退出...", Wcs.Framework.ExecutiveState.ActiveCounters.Count(x => x.Value != 0));
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                lblWaitings.Visible = false;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void frmCloseReasonDialog_Load(object sender, EventArgs e)
        {
            // btnAccept.Click += btnAccept_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Wcs.Framework.ExecutiveState.ApplicationExiting = true;

            string msg = string.Format(@"您正在关闭仓库设备控制系统(v{0})...
本次操作人员：{1}
关闭的原因选项：{2}
注释：{3}", Assembly.GetExecutingAssembly().GetName().Version
       , tbxOperators.Text
       , cbxReason.Text
       , tbxDescription.Text);

            _application.Logger.Info1(msg, this);

            System.Threading.Thread.Sleep(200);

            this.Enabled = false;
            lblWaitings.Visible = true;
            lblWaitings.Text = String.Format("正在等待 {0} 个工作线程退出...", Wcs.Framework.ExecutiveState.ActiveCounters.Count(x => x.Value != 0));
            while (Wcs.Framework.ExecutiveState.ActiveCounters.Any(x => x.Value != 0))
            {
                lblWaitings.Visible = true;

                lblWaitings.Text = String.Format("正在等待 {0} 个工作线程退出...", Wcs.Framework.ExecutiveState.ActiveCounters.Count(x => x.Value != 0));
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

            lblWaitings.Visible = false;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            this.Close();
        }
    }
}