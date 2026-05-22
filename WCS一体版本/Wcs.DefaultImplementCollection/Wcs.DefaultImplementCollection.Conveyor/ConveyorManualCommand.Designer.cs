
namespace Wcs.DefaultImplementCollection.Conveyor
{
    partial class ConveyorManualCommand
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tbx_send = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_send = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbx_send = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tbx_received = new System.Windows.Forms.TextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chb_restart = new System.Windows.Forms.CheckBox();
            this.cbx_received = new System.Windows.Forms.ComboBox();
            this.tbx_propertyValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbx_property = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.chb_WriteToFile = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel3);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Panel2.Controls.Add(this.panel4);
            this.splitContainer1.Size = new System.Drawing.Size(1090, 636);
            this.splitContainer1.SplitterDistance = 522;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.tbx_send);
            this.panel3.Location = new System.Drawing.Point(0, 52);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(522, 584);
            this.panel3.TabIndex = 1;
            // 
            // tbx_send
            // 
            this.tbx_send.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_send.Location = new System.Drawing.Point(0, 0);
            this.tbx_send.Multiline = true;
            this.tbx_send.Name = "tbx_send";
            this.tbx_send.Size = new System.Drawing.Size(522, 584);
            this.tbx_send.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.btn_send);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cbx_send);
            this.panel1.Location = new System.Drawing.Point(0, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(522, 43);
            this.panel1.TabIndex = 0;
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(406, 11);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(75, 23);
            this.btn_send.TabIndex = 2;
            this.btn_send.Text = "ManualSend";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "发送块：";
            // 
            // cbx_send
            // 
            this.cbx_send.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_send.FormattingEnabled = true;
            this.cbx_send.Location = new System.Drawing.Point(71, 12);
            this.cbx_send.Name = "cbx_send";
            this.cbx_send.Size = new System.Drawing.Size(314, 20);
            this.cbx_send.TabIndex = 0;
            this.cbx_send.TextChanged += new System.EventHandler(this.cbx_send_TextChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.tbx_received);
            this.panel2.Location = new System.Drawing.Point(0, 52);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(563, 584);
            this.panel2.TabIndex = 3;
            // 
            // tbx_received
            // 
            this.tbx_received.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_received.Location = new System.Drawing.Point(0, 0);
            this.tbx_received.Multiline = true;
            this.tbx_received.Name = "tbx_received";
            this.tbx_received.Size = new System.Drawing.Size(563, 584);
            this.tbx_received.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.chb_WriteToFile);
            this.panel4.Controls.Add(this.chb_restart);
            this.panel4.Controls.Add(this.cbx_received);
            this.panel4.Controls.Add(this.tbx_propertyValue);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.cbx_property);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Location = new System.Drawing.Point(1, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(563, 43);
            this.panel4.TabIndex = 2;
            // 
            // chb_restart
            // 
            this.chb_restart.AutoSize = true;
            this.chb_restart.Location = new System.Drawing.Point(400, 16);
            this.chb_restart.Name = "chb_restart";
            this.chb_restart.Size = new System.Drawing.Size(72, 16);
            this.chb_restart.TabIndex = 7;
            this.chb_restart.Text = "启动监控";
            this.chb_restart.UseVisualStyleBackColor = true;
            this.chb_restart.CheckedChanged += new System.EventHandler(this.chb_restart_CheckedChanged);
            // 
            // cbx_received
            // 
            this.cbx_received.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_received.FormattingEnabled = true;
            this.cbx_received.Location = new System.Drawing.Point(60, 12);
            this.cbx_received.Name = "cbx_received";
            this.cbx_received.Size = new System.Drawing.Size(122, 20);
            this.cbx_received.TabIndex = 0;
            this.cbx_received.TextChanged += new System.EventHandler(this.cbx_received_TextChanged);
            // 
            // tbx_propertyValue
            // 
            this.tbx_propertyValue.Location = new System.Drawing.Point(333, 13);
            this.tbx_propertyValue.Name = "tbx_propertyValue";
            this.tbx_propertyValue.Size = new System.Drawing.Size(61, 21);
            this.tbx_propertyValue.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(316, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "=";
            // 
            // cbx_property
            // 
            this.cbx_property.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_property.FormattingEnabled = true;
            this.cbx_property.Location = new System.Drawing.Point(245, 12);
            this.cbx_property.Name = "cbx_property";
            this.cbx_property.Size = new System.Drawing.Size(64, 20);
            this.cbx_property.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(188, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "检索条件：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "接收块：";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // chb_WriteToFile
            // 
            this.chb_WriteToFile.AutoSize = true;
            this.chb_WriteToFile.Location = new System.Drawing.Point(473, 15);
            this.chb_WriteToFile.Name = "chb_WriteToFile";
            this.chb_WriteToFile.Size = new System.Drawing.Size(84, 16);
            this.chb_WriteToFile.TabIndex = 8;
            this.chb_WriteToFile.Text = "输出到文件";
            this.chb_WriteToFile.UseVisualStyleBackColor = true;
            // 
            // ConveyorManualCommand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 636);
            this.Controls.Add(this.splitContainer1);
            this.MaximumSize = new System.Drawing.Size(1106, 675);
            this.MinimumSize = new System.Drawing.Size(1106, 675);
            this.Name = "ConveyorManualCommand";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ConveyorManualCommand";
            this.Load += new System.EventHandler(this.ConveyorManualCommand_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbx_send;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox tbx_propertyValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbx_property;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbx_received;
        private System.Windows.Forms.TextBox tbx_send;
        private System.Windows.Forms.TextBox tbx_received;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.CheckBox chb_restart;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chb_WriteToFile;
    }
}