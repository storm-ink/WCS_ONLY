
namespace WCS.APP
{
    partial class frmStarting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStarting));
            this.marqueeProgressBarControl1 = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.pictureEdit2 = new DevExpress.XtraEditors.PictureEdit();
            this.pictureEdit1 = new DevExpress.XtraEditors.PictureEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.lblExitApplication = new DevExpress.XtraEditors.HyperlinkLabelControl();
            this.tbxInfo = new DevExpress.Tutorials.Controls.RichTextBoxEx();
            this.lblVersion = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // marqueeProgressBarControl1
            // 
            this.marqueeProgressBarControl1.EditValue = 0;
            this.marqueeProgressBarControl1.Location = new System.Drawing.Point(12, 213);
            this.marqueeProgressBarControl1.Name = "marqueeProgressBarControl1";
            this.marqueeProgressBarControl1.Size = new System.Drawing.Size(426, 11);
            this.marqueeProgressBarControl1.TabIndex = 5;
            // 
            // labelControl1
            // 
            this.labelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.labelControl1.Location = new System.Drawing.Point(114, 264);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(131, 14);
            this.labelControl1.TabIndex = 6;
            this.labelControl1.Text = "Copyright © 2022-3022";
            // 
            // pictureEdit2
            // 
            this.pictureEdit2.EditValue = ((object)(resources.GetObject("pictureEdit2.EditValue")));
            this.pictureEdit2.Location = new System.Drawing.Point(12, 53);
            this.pictureEdit2.Name = "pictureEdit2";
            this.pictureEdit2.Properties.AllowFocused = false;
            this.pictureEdit2.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureEdit2.Properties.Appearance.Options.UseBackColor = true;
            this.pictureEdit2.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureEdit2.Properties.ShowMenu = false;
            this.pictureEdit2.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.pictureEdit2.Size = new System.Drawing.Size(426, 123);
            this.pictureEdit2.TabIndex = 9;
            // 
            // pictureEdit1
            // 
            this.pictureEdit1.EditValue = ((object)(resources.GetObject("pictureEdit1.EditValue")));
            this.pictureEdit1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureEdit1.Location = new System.Drawing.Point(322, 246);
            this.pictureEdit1.Name = "pictureEdit1";
            this.pictureEdit1.Properties.AllowFocused = false;
            this.pictureEdit1.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureEdit1.Properties.Appearance.Options.UseBackColor = true;
            this.pictureEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureEdit1.Properties.ShowMenu = false;
            this.pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.pictureEdit1.Size = new System.Drawing.Size(160, 44);
            this.pictureEdit1.TabIndex = 8;
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 21.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl3.Appearance.Options.UseBackColor = true;
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(12, 12);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(283, 35);
            this.labelControl3.TabIndex = 10;
            this.labelControl3.Text = "WCS - 仓储控制系统";
            // 
            // lblExitApplication
            // 
            this.lblExitApplication.Appearance.ForeColor = System.Drawing.Color.Red;
            this.lblExitApplication.Appearance.Options.UseForeColor = true;
            this.lblExitApplication.LineColor = System.Drawing.Color.Red;
            this.lblExitApplication.Location = new System.Drawing.Point(410, 12);
            this.lblExitApplication.Name = "lblExitApplication";
            this.lblExitApplication.Size = new System.Drawing.Size(28, 14);
            this.lblExitApplication.TabIndex = 11;
            this.lblExitApplication.Text = "Close";
            this.lblExitApplication.Click += new System.EventHandler(this.lblExitApplication_Click);
            // 
            // tbxInfo
            // 
            this.tbxInfo.BackColor = System.Drawing.SystemColors.Control;
            this.tbxInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxInfo.Location = new System.Drawing.Point(12, 182);
            this.tbxInfo.Name = "tbxInfo";
            this.tbxInfo.Size = new System.Drawing.Size(426, 25);
            this.tbxInfo.TabIndex = 12;
            this.tbxInfo.Text = "";
            // 
            // lblVersion
            // 
            this.lblVersion.Location = new System.Drawing.Point(12, 264);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(79, 14);
            this.lblVersion.TabIndex = 14;
            this.lblVersion.Text = "版本号:v 1.0.0";
            // 
            // frmStarting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 295);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.tbxInfo);
            this.Controls.Add(this.lblExitApplication);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.pictureEdit2);
            this.Controls.Add(this.pictureEdit1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.marqueeProgressBarControl1);
            this.Name = "frmStarting";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmStarting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.MarqueeProgressBarControl marqueeProgressBarControl1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.PictureEdit pictureEdit1;
        private DevExpress.XtraEditors.PictureEdit pictureEdit2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.HyperlinkLabelControl lblExitApplication;
        private DevExpress.Tutorials.Controls.RichTextBoxEx tbxInfo;
        private DevExpress.XtraEditors.LabelControl lblVersion;
    }
}
