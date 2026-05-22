using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager.Form
{
    public partial class frmStationTaskFilterSetting : System.Windows.Forms.Form
    {
        SingleLocationDoubleVehicleSubSystem _device;
        List<SingleLocationDoubleVehicleSubSystemLocation> locations;
        int columMax = 2;
        public frmStationTaskFilterSetting(SingleLocationDoubleVehicleSubSystem device)
        {
            _device = device;
            InitializeComponent();

            locations = _device.Locations.Select(x => (SingleLocationDoubleVehicleSubSystemLocation)x).Where(x => x.PickingChainAction != RailGuidedVehicle.ChainAction.None && x.PuttingChainAction != RailGuidedVehicle.ChainAction.None).ToList();
            var line = locations.Count() / columMax;
            frmTaskFilterSetting frmTaskFilterSetting = new frmTaskFilterSetting();
            if (locations.Count() % columMax > 0)
                ++line;
            this.Size = new Size(frmTaskFilterSetting.Size.Width * columMax + ((columMax - 1) * 10), (frmTaskFilterSetting.Size.Height + 10) * line + button1.Size.Height);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        List<frmTaskFilterSetting> frmTaskFilterSettings = new List<frmTaskFilterSetting>();
        private void frmStationTaskFilterSetting_Load(object sender, EventArgs e)
        {
            var freedomList = _device.Config.GetFreedomStationList();
            int i = 0, j = 0;
            foreach (var item in locations)
            {
                frmTaskFilterSetting frmTaskFilterSetting = new frmTaskFilterSetting(_device, item.UserCode, freedomList.Contains(item.UserCode));
                if (j >= columMax)
                {
                    ++i;
                    j = 0;
                }
                frmTaskFilterSetting.Location = new Point(j++ * (frmTaskFilterSetting.Size.Width + 10), i * (frmTaskFilterSetting.Height + 10));
                this.Controls.Add(frmTaskFilterSetting);
                frmTaskFilterSettings.Add(frmTaskFilterSetting);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (var item in frmTaskFilterSettings)
                {
                    var kv = item.GetSetting();
                    if (kv.Value)
                        list.Add(kv.Key);
                }
                _device.Config.FreedomStationList = string.Join(",", list);
                _device.Config.Save();
                MessageBox.Show("保存成功");
                this.Dispose();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存时发生异常，请重试！异常消息：{ex}");
            }
        }
    }
}
