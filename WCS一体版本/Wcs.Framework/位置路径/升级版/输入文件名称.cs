using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wcs.Framework
{
    public partial class 输入文件名称 : Form
    {
        public 输入文件名称()
        {
            InitializeComponent();
        }

        static DialogResult _result;

        public static string Run(String textBox)
        {
            输入文件名称 dlg = new 输入文件名称();
            dlg.textBox1.Text = textBox;
            _result = dlg.ShowDialog();
            if (_result == DialogResult.Yes)
            {
                var _newFileName = dlg.textBox1.Text.Trim();
                dlg.Dispose();
                dlg.Close();
                return _newFileName;
            }
            else
            {
                dlg.Dispose();
                dlg.Close();
                return "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }
    }
}
