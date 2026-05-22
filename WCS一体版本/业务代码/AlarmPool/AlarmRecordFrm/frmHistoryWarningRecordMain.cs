using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs.Framework;
using NHibernate.Linq;
using Wcs.Framework.Events;
using Wcs.Framework.Cfg;
using Wcs.Framework.EventBus;
using Wcs;
using System.Collections;
using NHibernate.Transform;
using NLog;
using System.Data.SqlClient;

namespace ZHQXC.AlarmPool
{
    public partial class frmHistoryWarningRecordMain : Form
    {
        const string NullTaskType = "【空字符串】";
        //WcsContext WcsContext;
        BindingSource _bindingSource;
        Logger _logger = LogManager.GetCurrentClassLogger();
        public frmHistoryWarningRecordMain()
        {
            InitializeComponent();
            dgvGrid.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = new List<Hashtable>();
            dgvGrid.DataSource = _bindingSource;
        }

        public frmHistoryWarningRecordMain(WcsContext context)
            : this()
        {

            //WcsContext = context;
            dgvGrid.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = new List<Hashtable>();
            dgvGrid.DataSource = _bindingSource;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            //{
            //    loadTaskTypes();
            //}, null);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }


        void load()
        {
            Dictionary<string, object> pars = new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();
            SqlCommand cmd = new SqlCommand();
            sb.AppendLine(@"SELECT top " + Convert.ToInt32(numericUpDown1.Value) + @" * FROM [AlarmRecords] where 1=1");
            if (dtpStartDate.Checked)
            {
                sb.AppendLine(" and BeginingAt>=" + "'" + dtpStartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            }

            if (dtpEndDate.Checked)
            {
                sb.AppendLine(" and BeginingAt<=" + "'" + dtpEndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            }
            if (!string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                var v = comboBox1.Text.Trim();
                if (v == NullTaskType)
                {
                    //sb.AppendLine(" and (DeviceType = '' or DeviceType is null)");
                }
                else
                {
                    sb.AppendLine(" and DeviceType like" + "'" + comboBox1.Text.Trim() + "'");
                }
            }

            sb.AppendLine(" order by id desc");
            DataTable dtb = new DataTable();
            using (SqlConnection conn = new SqlConnection(WcsConfigurationConnectionStrings.WcsBakConnectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    cmd.Connection = conn;
                    cmd.Transaction = trans;
                    cmd.CommandText = sb.ToString();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dtb);
                    }

                    trans.Commit();
                }
            }

            BindingSource bindingSource = new BindingSource(dtb, "");
            dgvGrid.DataSource = bindingSource;
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        groupBox1.Enabled =
                        dgvGrid.Enabled = false;
                        load();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        MessageBox.Show(string.Format("数据加载失败。{0}", ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //WcsContext.Application.Logger.Error1(ex, this);
                    }
                    finally
                    {
                        groupBox1.Enabled =
                       dgvGrid.Enabled = true;
                    }
                }));

            }, null);
        }

        private void dgvList_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, new SolidBrush(Color.Black), e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                //MessageBox.Show("添加行号时发生错误，错误信息：" + ex.Message, "操作失败");
            }
        }

        private void dgvGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        private void tsmiView_Click(object sender, EventArgs e)
        {

        }

        private void dgvGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //tsmiView_Click(null, null);
        }

        private void tbxTaskCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch.PerformClick();
            }
        }

        private void btn_Excel_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName;
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.CheckFileExists = false;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.DefaultExt = "xlsx";
                    saveFileDialog.FileName = "报警记录" + DateTime.Now.ToString("yyyyMMdd");
                    saveFileDialog.Filter = "XML 电子表格 2003|*.xlsx";
                    saveFileDialog.ValidateNames = true;
                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                    {
                        return;
                    }

                    fileName = saveFileDialog.FileName;
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                dgvGrid.ExportAsExcel(fileName);

                if (MessageBox.Show("导出成功，是否立即打开？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //void loadTaskTypes()
        //{
        //    try
        //    {
        //        using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
        //        {
        //            var taskTypes = unitOfWork.session.Query<Task>()
        //                .GroupBy(x => x.TaskType)
        //                .Select(x => x.Key)
        //                .ToArray();

        //            unitOfWork.Commit();

        //            this.Invoke(new MethodInvoker(() =>
        //            {
        //                try
        //                {
        //                    comboBox1.Items.Clear();
        //                    comboBox1.Items.Add("");
        //                    foreach (var item in taskTypes)
        //                    {
        //                        if (String.IsNullOrWhiteSpace(item))
        //                        {
        //                            var key = "【空字符串】";
        //                            comboBox1.Items.Add(key);
        //                        }
        //                        else
        //                        {
        //                            comboBox1.Items.Add(item);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.Error1(ex, this);
        //                    //WcsContext.Application.Logger.Error1(ex, this);
        //                }
        //            }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error1(ex, this);
        //        //WcsContext.Application.Logger.Error1(ex, this);
        //    }
        //}
    }
}
