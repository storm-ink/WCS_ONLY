namespace Wcs.DefaultImplementCollection.Crane
{
    partial class frmResumeCraneTask
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rbF1wh = new System.Windows.Forms.RadioButton();
            this.rbF1yh = new System.Windows.Forms.RadioButton();
            this.lblForkBarcode = new System.Windows.Forms.Label();
            this.tbxForkBarcode = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tbxLocationBarcode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblLF1Barcode = new System.Windows.Forms.Label();
            this.lblLocation = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 183);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(377, 38);
            this.panel1.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label6.Location = new System.Drawing.Point(10, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(137, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "请认真核对，严防窜货！";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(275, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消(&ESC)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(182, 6);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "确认(&OK)";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(377, 1);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "货叉：";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbF1wh);
            this.panel2.Controls.Add(this.rbF1yh);
            this.panel2.Location = new System.Drawing.Point(60, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(110, 23);
            this.panel2.TabIndex = 2;
            // 
            // rbF1wh
            // 
            this.rbF1wh.AutoSize = true;
            this.rbF1wh.Location = new System.Drawing.Point(56, 3);
            this.rbF1wh.Name = "rbF1wh";
            this.rbF1wh.Size = new System.Drawing.Size(47, 16);
            this.rbF1wh.TabIndex = 0;
            this.rbF1wh.TabStop = true;
            this.rbF1wh.Text = "无货";
            this.rbF1wh.UseVisualStyleBackColor = true;
            this.rbF1wh.CheckedChanged += new System.EventHandler(this.rbF1yh_CheckedChanged);
            // 
            // rbF1yh
            // 
            this.rbF1yh.AutoSize = true;
            this.rbF1yh.Location = new System.Drawing.Point(3, 3);
            this.rbF1yh.Name = "rbF1yh";
            this.rbF1yh.Size = new System.Drawing.Size(47, 16);
            this.rbF1yh.TabIndex = 0;
            this.rbF1yh.TabStop = true;
            this.rbF1yh.Text = "有货";
            this.rbF1yh.UseVisualStyleBackColor = true;
            this.rbF1yh.CheckedChanged += new System.EventHandler(this.rbF1yh_CheckedChanged);
            // 
            // lblForkBarcode
            // 
            this.lblForkBarcode.AutoSize = true;
            this.lblForkBarcode.Enabled = false;
            this.lblForkBarcode.Location = new System.Drawing.Point(182, 32);
            this.lblForkBarcode.Name = "lblForkBarcode";
            this.lblForkBarcode.Size = new System.Drawing.Size(41, 12);
            this.lblForkBarcode.TabIndex = 3;
            this.lblForkBarcode.Text = "条码：";
            // 
            // tbxForkBarcode
            // 
            this.tbxForkBarcode.Enabled = false;
            this.tbxForkBarcode.Location = new System.Drawing.Point(217, 28);
            this.tbxForkBarcode.Name = "tbxForkBarcode";
            this.tbxForkBarcode.Size = new System.Drawing.Size(121, 21);
            this.tbxForkBarcode.TabIndex = 4;
            this.toolTip1.SetToolTip(this.tbxForkBarcode, "当前货叉1上面货物的容器编码（条码），多个时只需要输入一个。");
            this.tbxForkBarcode.TextChanged += new System.EventHandler(this.tbxF1Barcode_TextChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // tbxLocationBarcode
            // 
            this.tbxLocationBarcode.Location = new System.Drawing.Point(217, 28);
            this.tbxLocationBarcode.Name = "tbxLocationBarcode";
            this.tbxLocationBarcode.Size = new System.Drawing.Size(121, 21);
            this.tbxLocationBarcode.TabIndex = 4;
            this.toolTip1.SetToolTip(this.tbxLocationBarcode, "货物的容器编码（条码），多个时只需要输入一个。");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "位置：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbxForkBarcode);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.lblForkBarcode);
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(352, 68);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "载货台情况";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbxLocationBarcode);
            this.groupBox2.Controls.Add(this.lblLF1Barcode);
            this.groupBox2.Controls.Add(this.lblLocation);
            this.groupBox2.Location = new System.Drawing.Point(12, 96);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(352, 66);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "取货位情况";
            // 
            // lblLF1Barcode
            // 
            this.lblLF1Barcode.AutoSize = true;
            this.lblLF1Barcode.Location = new System.Drawing.Point(182, 32);
            this.lblLF1Barcode.Name = "lblLF1Barcode";
            this.lblLF1Barcode.Size = new System.Drawing.Size(41, 12);
            this.lblLF1Barcode.TabIndex = 3;
            this.lblLF1Barcode.Text = "条码：";
            // 
            // lblLocation
            // 
            this.lblLocation.AutoSize = true;
            this.lblLocation.Location = new System.Drawing.Point(59, 32);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(11, 12);
            this.lblLocation.TabIndex = 3;
            this.lblLocation.Text = "-";
            // 
            // frmResumeCraneTask
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(377, 221);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmResumeCraneTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "请确认C001相关信息";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rbF1wh;
        private System.Windows.Forms.RadioButton rbF1yh;
        private System.Windows.Forms.Label lblForkBarcode;
        private System.Windows.Forms.TextBox tbxForkBarcode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbxLocationBarcode;
        private System.Windows.Forms.Label lblLF1Barcode;
        private System.Windows.Forms.Label lblLocation;
    }
}