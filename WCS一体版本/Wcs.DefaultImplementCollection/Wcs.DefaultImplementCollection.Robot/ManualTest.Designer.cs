
namespace Wcs.DefaultImplementCollection.Robot
{
    partial class ManualTest
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
            this.tbx_received_bytes = new System.Windows.Forms.TextBox();
            this.tbx_send = new System.Windows.Forms.TextBox();
            this.tbx_send_bytes = new System.Windows.Forms.TextBox();
            this.tbx_received = new System.Windows.Forms.TextBox();
            this.btn_send = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbx_received_bytes
            // 
            this.tbx_received_bytes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbx_received_bytes.Location = new System.Drawing.Point(549, 605);
            this.tbx_received_bytes.Multiline = true;
            this.tbx_received_bytes.Name = "tbx_received_bytes";
            this.tbx_received_bytes.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbx_received_bytes.Size = new System.Drawing.Size(564, 190);
            this.tbx_received_bytes.TabIndex = 11;
            // 
            // tbx_send
            // 
            this.tbx_send.Location = new System.Drawing.Point(4, 37);
            this.tbx_send.Multiline = true;
            this.tbx_send.Name = "tbx_send";
            this.tbx_send.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbx_send.Size = new System.Drawing.Size(539, 562);
            this.tbx_send.TabIndex = 10;
            // 
            // tbx_send_bytes
            // 
            this.tbx_send_bytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tbx_send_bytes.Location = new System.Drawing.Point(4, 605);
            this.tbx_send_bytes.Multiline = true;
            this.tbx_send_bytes.Name = "tbx_send_bytes";
            this.tbx_send_bytes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbx_send_bytes.Size = new System.Drawing.Size(539, 190);
            this.tbx_send_bytes.TabIndex = 9;
            // 
            // tbx_received
            // 
            this.tbx_received.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbx_received.Location = new System.Drawing.Point(549, 37);
            this.tbx_received.Multiline = true;
            this.tbx_received.Name = "tbx_received";
            this.tbx_received.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbx_received.Size = new System.Drawing.Size(564, 562);
            this.tbx_received.TabIndex = 8;
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(182, 8);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(75, 23);
            this.btn_send.TabIndex = 12;
            this.btn_send.Text = "Send";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(263, 12);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(96, 16);
            this.checkBox1.TabIndex = 13;
            this.checkBox1.Text = "显示十六进制";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 14;
            this.label1.Text = "发送区：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(547, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "接收区：";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(71, 8);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(105, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "初始化发送消息";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(983, 15);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(108, 16);
            this.checkBox2.TabIndex = 17;
            this.checkBox2.Text = "接收区停止刷新";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // ManualTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 800);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.tbx_received_bytes);
            this.Controls.Add(this.tbx_send);
            this.Controls.Add(this.tbx_send_bytes);
            this.Controls.Add(this.tbx_received);
            this.Name = "ManualTest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ManualTest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbx_received_bytes;
        private System.Windows.Forms.TextBox tbx_send;
        private System.Windows.Forms.TextBox tbx_send_bytes;
        private System.Windows.Forms.TextBox tbx_received;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}