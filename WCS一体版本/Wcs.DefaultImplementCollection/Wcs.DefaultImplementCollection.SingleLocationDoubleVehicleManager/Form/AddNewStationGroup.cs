using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    public partial class AddNewStationGroup : System.Windows.Forms.Form
    {
        public AddNewStationGroup()
        {
            InitializeComponent();
        }

        string addNewStationName = "";
        public string GetNewGroupName()
        {
            this.ShowDialog();
            return addNewStationName;
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            addNewStationName = tbx_groupName.Text;
            this.Close();
        }
    }
}
