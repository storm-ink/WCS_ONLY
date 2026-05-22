
namespace WCS.APP
{
    partial class frmCloseReasonDialog
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
            this.lblWaitings = new System.Windows.Forms.Label();
            this.tbxOperators = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbxDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxReason = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblWaitings
            // 
            this.lblWaitings.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
            this.lblWaitings.ForeColor = System.Drawing.Color.Red;
            this.lblWaitings.Location = new System.Drawing.Point(12, 326);
            this.lblWaitings.Name = "lblWaitings";
            this.lblWaitings.Size = new System.Drawing.Size(203, 19);
            this.lblWaitings.TabIndex = 19;
            this.lblWaitings.Text = "正在等待 1 个工作线程退出...";
            this.lblWaitings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblWaitings.Visible = false;
            // 
            // tbxOperators
            // 
            this.tbxOperators.Location = new System.Drawing.Point(10, 56);
            this.tbxOperators.Name = "tbxOperators";
            this.tbxOperators.Size = new System.Drawing.Size(380, 21);
            this.tbxOperators.TabIndex = 18;
            this.tbxOperators.TextChanged += new System.EventHandler(this.tbxOperators_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(185, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "操作人员（本次操作的相关人员）";
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Location = new System.Drawing.Point(242, 326);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(64, 20);
            this.btnAccept.TabIndex = 15;
            this.btnAccept.Text = "确定(&OK)";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(327, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 20);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "取消(&ESC)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbxDescription
            // 
            this.tbxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxDescription.Location = new System.Drawing.Point(10, 149);
            this.tbxDescription.Multiline = true;
            this.tbxDescription.Name = "tbxDescription";
            this.tbxDescription.Size = new System.Drawing.Size(382, 170);
            this.tbxDescription.TabIndex = 14;
            this.tbxDescription.TextChanged += new System.EventHandler(this.tbxDescription_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "注释(至少10个字符)";
            // 
            // cbxReason
            // 
            this.cbxReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxReason.FormattingEnabled = true;
            this.cbxReason.IntegralHeight = false;
            this.cbxReason.ItemHeight = 12;
            this.cbxReason.Items.AddRange(new object[] {
            "",
            "配置更新后需要重新启动（维护）",
            "计算机需要关闭（维护）",
            "其它原因（维护）",
            "应用程序无响应（故障）"});
            this.cbxReason.Location = new System.Drawing.Point(10, 100);
            this.cbxReason.Name = "cbxReason";
            this.cbxReason.Size = new System.Drawing.Size(382, 20);
            this.cbxReason.TabIndex = 12;
            this.cbxReason.SelectedIndexChanged += new System.EventHandler(this.cbxReason_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(239, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "请选择最准确的您关闭Wcs系统的原因的选项";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "您正在关闭仓库设备控制系统(v2.1.4.0312)...";
            // 
            // frmCloseReasonDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 359);
            this.Controls.Add(this.lblWaitings);
            this.Controls.Add(this.tbxOperators);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tbxDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbxReason);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "frmCloseReasonDialog";
            this.Text = "关闭事件跟踪程序";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWaitings;
        private System.Windows.Forms.TextBox tbxOperators;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbxDescription;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxReason;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}