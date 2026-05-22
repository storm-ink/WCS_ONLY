using System;
using System.Data;
using System.Windows.Forms;
using Wcs.Framework;

namespace BOE.设备相关.AGV.GSAGV
{
    public partial class GSAGVUserInterfaceFrm : Form, IDeviceUserInterface
    {
        GSAGVDevice _device;
        public GSAGVUserInterfaceFrm()
        {
            InitializeComponent();
        }

        public void Show(Device device)
        {
            _device = (GSAGVDevice)device;
            this.Text = String.Format("查看 {0} 的状态", _device.Name);
            DataTable dt = GSAGVDatabaseHand.GetAllTaskToZJB();
            dgvGrid.DataSource = dt;
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
                    model.beginLoc = strBeginLoc;
                    model.beginLevel = int.Parse(beginLevel);
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
                    if (!GSAGVDatabaseHand.InterTaskToAGV(model))
                    {
                        throw new Exception();
                    }
                    Btn_Rush_Click(null, null);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("创建任务异常，异常信息为：" + exception.Message);
            }

        }

        private void Btn_Rush_Click(object sender, EventArgs e)
        {
            DataTable dt = GSAGVDatabaseHand.GetAllTaskToZJB();
            dgvGrid.DataSource = dt;
        }
    }
}
