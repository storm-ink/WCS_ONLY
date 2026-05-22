using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHibernate.Linq;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.AGV
{
    public partial class SinevaAGVUserInterfaceFrm : Form, IDeviceUserInterface
    {
        SinevaAGVDevice _device;
        public SinevaAGVUserInterfaceFrm()
        {

            InitializeComponent();
            button1.Hide();


        }

        public void Show(Device device)
        {
            _device = (SinevaAGVDevice)device;
            this.Text = String.Format("查看 {0} 的状态", _device.Name);
            //DataTable dt = SinevaAGVDatabaseHand.GetAllTaskToZJB(_device.Name);
            //dgvGrid.DataSource = dt;
            this.ShowDialog();
            dgvGrid.AutoGenerateColumns = false;
        }

        private void CreateTask_Click(object sender, EventArgs e)
        {
            try
            {
                string strBeginLoc = beginloc.Text.Trim();
                string strEndLoc = endloc.Text.Trim();
                string strSalver = string.Empty;
                string beginLevel = this.beginLevel.Text.Trim();
                string endLevel = this.endLevel.Text.Trim();
                if (string.IsNullOrEmpty(beginLevel))
                {
                    beginLevel = "1";
                }
                if (string.IsNullOrEmpty(endLevel))
                {
                    endLevel = "1";
                }
                if (string.IsNullOrEmpty(strBeginLoc) || string.IsNullOrEmpty(strEndLoc) || cmbTaskType.SelectedIndex < 0)
                {
                    MessageBox.Show("任务条件非法,请检查后处理.");
                    return;
                }
                else
                {
                    AGV_T_interface model = new AGV_T_interface();
                    model.begionLoc = strBeginLoc;
                    model.begionLevel = int.Parse(beginLevel);
                    model.endLoc = strEndLoc;
                    model.endLevel = int.Parse(endLevel);
                    model.createDatetime = DateTime.Now;
                    model.grade = 10;
                    model.interCode = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + new Random().Next(10000, 99999).ToString();
                    model.interType = (cmbTaskType.SelectedIndex + 1).ToString();
                    model.state = "1";
                    model.category = "2";
                    model.sort = "0";
                    model.agvId = 0;
                    model.SalverType = "1";
                    model.TrayCode = new Random().Next(50, 100).ToString();
                    if (!SinevaAGVDatabaseHand.InterTaskToAGV(model, _device.Name))
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("创建任务异常，异常信息为：" + exception.Message);
            }

        }

        private void btn_Rush_Click(object sender, EventArgs e)
        {
            DataTable dt = SinevaAGVDatabaseHand.GetAllTaskToZJB(_device.Name);
            dgvGrid.DataSource = dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = SinevaAGVDatabaseHand.HisGetAllTaskToZJB(_device.Name);
            dgvGrid.DataSource = dt;
        }

        private void 强制完成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "";
                if (dgvGrid.CurrentRow != null)
                {
                    str = dgvGrid.CurrentRow.Cells[1].Value.ToString();
                    string sql = $"update[ZJB].[dbo].[t_interface] set state = 3 where intercode = '{str}'";
                    SinevaAGVSubSystemAction _SinevaAGVSubSystemAction;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            _SinevaAGVSubSystemAction = unitOfWork.session.Query<SinevaAGVSubSystemAction>().Where(x => x.SendAGVTaskId == str).FirstOrDefault();


                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {

                            unitOfWork.Commit();
                            throw ex;
                        }
                    }
                    if (_SinevaAGVSubSystemAction != null)
                    {
                        if (MessageBox.Show($"请确认选择需要完成的任务号:{_SinevaAGVSubSystemAction?.Movement.Task.TaskCode},子任务Id:{str}", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    //str = dgvGrid.CurrentRow.Cells[1].Value.ToString();
                    //string sql = $"update[ZJB].[dbo].[t_interface] set state = 3 where intercode = '{str}'";
                    int rows = SinevaAGVDatabaseHand.delete(sql);
                    MessageBox.Show("强制完成成功");
                    DataTable dt = SinevaAGVDatabaseHand.GetAllTaskToZJB(_device.Name);
                    dgvGrid.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (dgvGrid.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                MessageBox.Show("请选择需要操作的任务");
                return;
            }




        }

  

        private void 任务重发ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "";
                if (dgvGrid.CurrentRow != null)
                {
                    str = dgvGrid.CurrentRow.Cells[1].Value.ToString();
                    string sql = $"update[ZJB].[dbo].[t_interface] set state = 1,intercode='{str + "强制重发"}' where intercode = '{str}'";
                    SinevaAGVSubSystemAction _SinevaAGVSubSystemAction;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            _SinevaAGVSubSystemAction = unitOfWork.session.Query<SinevaAGVSubSystemAction>().Where(x => x.SendAGVTaskId == str).FirstOrDefault();


                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {

                            unitOfWork.Commit();
                            throw ex;
                        }
                    }
                    if (_SinevaAGVSubSystemAction != null)
                    {
                        if (MessageBox.Show($"请确认选择需要重发的任务号:{_SinevaAGVSubSystemAction?.Movement.Task.TaskCode},子任务Id:{str}", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    //str = dgvGrid.CurrentRow.Cells[1].Value.ToString();
                    //string sql = $"update[ZJB].[dbo].[t_interface] set state = 3 where intercode = '{str}'";
                    int rows = SinevaAGVDatabaseHand.delete(sql);
                    MessageBox.Show("强制重发成功");
                    DataTable dt = SinevaAGVDatabaseHand.GetAllTaskToZJB(_device.Name);
                    dgvGrid.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }
    }
}
