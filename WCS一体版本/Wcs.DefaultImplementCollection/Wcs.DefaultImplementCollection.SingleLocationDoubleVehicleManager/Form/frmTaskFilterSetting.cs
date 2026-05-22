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
    public partial class frmTaskFilterSetting : UserControl
    {
        SingleLocationDoubleVehicleSubSystem _device;
        string _userCode;

        public frmTaskFilterSetting()
        {
            InitializeComponent();
        }
        public frmTaskFilterSetting(SingleLocationDoubleVehicleSubSystem device, string userCode, bool isChecked)
        {
            _device = device;
            _userCode = userCode;

            InitializeComponent();

            label1.Text = userCode;
            checkBox1.Checked = isChecked;
        }

        public KeyValuePair<string, bool> GetSetting()
        {
            return new KeyValuePair<string, bool>(label1.Text, this.checkBox1.Checked);
        }
    }
}
