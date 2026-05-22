namespace Wcs.DefaultImplementCollection.Robot
{
    partial class RobotDeviceUserInterface
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLock = new System.Windows.Forms.Button();
            this.btnUnlock = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblIsAlarm = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.lbl_ConnState = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lab_IPAddress = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblTaskId = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblDeviceType = new System.Windows.Forms.Label();
            this.lblDeviceName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.初始化ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wCS完成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.异常ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btn_manual = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cbx_loadTask = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbx_state = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tbxWarnings = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.tbx_task = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLock);
            this.groupBox2.Controls.Add(this.btnUnlock);
            this.groupBox2.Location = new System.Drawing.Point(0, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(943, 49);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "锁信息";
            // 
            // btnLock
            // 
            this.btnLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLock.Location = new System.Drawing.Point(774, 20);
            this.btnLock.Name = "btnLock";
            this.btnLock.Size = new System.Drawing.Size(75, 23);
            this.btnLock.TabIndex = 5;
            this.btnLock.Text = "锁定";
            this.btnLock.UseVisualStyleBackColor = true;
            this.btnLock.Click += new System.EventHandler(this.btnLock_Click);
            // 
            // btnUnlock
            // 
            this.btnUnlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnlock.Location = new System.Drawing.Point(862, 20);
            this.btnUnlock.Name = "btnUnlock";
            this.btnUnlock.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock.TabIndex = 4;
            this.btnUnlock.Text = "强制解除";
            this.btnUnlock.UseVisualStyleBackColor = true;
            this.btnUnlock.Click += new System.EventHandler(this.btnUnlock_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblIsAlarm);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.linkLabel1);
            this.groupBox1.Controls.Add(this.lbl_ConnState);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.lab_IPAddress);
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.lblTaskId);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.lblDeviceType);
            this.groupBox1.Controls.Add(this.lblDeviceName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblStatus);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(0, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(943, 77);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "基本信息";
            // 
            // lblIsAlarm
            // 
            this.lblIsAlarm.AutoSize = true;
            this.lblIsAlarm.Location = new System.Drawing.Point(314, 50);
            this.lblIsAlarm.Name = "lblIsAlarm";
            this.lblIsAlarm.Size = new System.Drawing.Size(29, 12);
            this.lblIsAlarm.TabIndex = 25;
            this.lblIsAlarm.Text = "TRUE";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(274, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 24;
            this.label5.Text = "报警：";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel1.Location = new System.Drawing.Point(787, 29);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(147, 33);
            this.linkLabel1.TabIndex = 23;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "远程急停";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // lbl_ConnState
            // 
            this.lbl_ConnState.AutoSize = true;
            this.lbl_ConnState.Location = new System.Drawing.Point(357, 25);
            this.lbl_ConnState.Name = "lbl_ConnState";
            this.lbl_ConnState.Size = new System.Drawing.Size(41, 12);
            this.lbl_ConnState.TabIndex = 22;
            this.lbl_ConnState.Text = "已连接";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(274, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 21;
            this.label11.Text = "连接状态：";
            // 
            // lab_IPAddress
            // 
            this.lab_IPAddress.AutoSize = true;
            this.lab_IPAddress.Location = new System.Drawing.Point(481, 25);
            this.lab_IPAddress.Name = "lab_IPAddress";
            this.lab_IPAddress.Size = new System.Drawing.Size(125, 12);
            this.lab_IPAddress.TabIndex = 18;
            this.lab_IPAddress.Text = "192.192.192.192:9999";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(458, 25);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(29, 12);
            this.label19.TabIndex = 17;
            this.label19.Text = "IP：";
            // 
            // lblTaskId
            // 
            this.lblTaskId.AutoSize = true;
            this.lblTaskId.Location = new System.Drawing.Point(53, 50);
            this.lblTaskId.Name = "lblTaskId";
            this.lblTaskId.Size = new System.Drawing.Size(41, 12);
            this.lblTaskId.TabIndex = 12;
            this.lblTaskId.Text = "000000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 50);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 12);
            this.label10.TabIndex = 11;
            this.label10.Text = "任务：";
            // 
            // lblDeviceType
            // 
            this.lblDeviceType.AutoSize = true;
            this.lblDeviceType.Location = new System.Drawing.Point(47, 25);
            this.lblDeviceType.Name = "lblDeviceType";
            this.lblDeviceType.Size = new System.Drawing.Size(41, 12);
            this.lblDeviceType.TabIndex = 3;
            this.lblDeviceType.Text = "机械手";
            // 
            // lblDeviceName
            // 
            this.lblDeviceName.AutoSize = true;
            this.lblDeviceName.Location = new System.Drawing.Point(182, 25);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(47, 12);
            this.lblDeviceName.TabIndex = 3;
            this.lblDeviceName.Text = "机械手1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "类型：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "名称：";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(187, 50);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 12);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "初始化";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "模式：";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.初始化ToolStripMenuItem,
            this.wCS完成ToolStripMenuItem,
            this.异常ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(128, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // 初始化ToolStripMenuItem
            // 
            this.初始化ToolStripMenuItem.Name = "初始化ToolStripMenuItem";
            this.初始化ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.初始化ToolStripMenuItem.Text = "初始化";
            this.初始化ToolStripMenuItem.Click += new System.EventHandler(this.初始化ToolStripMenuItem_Click);
            // 
            // wCS完成ToolStripMenuItem
            // 
            this.wCS完成ToolStripMenuItem.Name = "wCS完成ToolStripMenuItem";
            this.wCS完成ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.wCS完成ToolStripMenuItem.Text = "WCS完成";
            this.wCS完成ToolStripMenuItem.Click += new System.EventHandler(this.wCS完成ToolStripMenuItem_Click);
            // 
            // 异常ToolStripMenuItem
            // 
            this.异常ToolStripMenuItem.Name = "异常ToolStripMenuItem";
            this.异常ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.异常ToolStripMenuItem.Text = "异常";
            this.异常ToolStripMenuItem.Click += new System.EventHandler(this.异常ToolStripMenuItem_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cbx_loadTask);
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.btn_manual);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox5.Location = new System.Drawing.Point(3, 17);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(940, 49);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "手动操作区";
            // 
            // btn_manual
            // 
            this.btn_manual.Location = new System.Drawing.Point(6, 21);
            this.btn_manual.Name = "btn_manual";
            this.btn_manual.Size = new System.Drawing.Size(75, 23);
            this.btn_manual.TabIndex = 9;
            this.btn_manual.Text = "ManualTask";
            this.btn_manual.UseVisualStyleBackColor = true;
            this.btn_manual.Click += new System.EventHandler(this.btn_manual_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(87, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "ClearTask";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbx_loadTask
            // 
            this.cbx_loadTask.AutoSize = true;
            this.cbx_loadTask.Location = new System.Drawing.Point(790, 25);
            this.cbx_loadTask.Name = "cbx_loadTask";
            this.cbx_loadTask.Size = new System.Drawing.Size(144, 16);
            this.cbx_loadTask.TabIndex = 11;
            this.cbx_loadTask.Text = "从任务中加载箱子信息";
            this.cbx_loadTask.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox4.Controls.Add(this.tbx_task);
            this.groupBox4.Controls.Add(this.panel1);
            this.groupBox4.Controls.Add(this.groupBox6);
            this.groupBox4.Location = new System.Drawing.Point(3, 66);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(940, 507);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "实时信息";
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.tbx_state);
            this.panel1.Location = new System.Drawing.Point(3, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(937, 254);
            this.panel1.TabIndex = 2;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // tbx_state
            // 
            this.tbx_state.Location = new System.Drawing.Point(-3, 0);
            this.tbx_state.Multiline = true;
            this.tbx_state.Name = "tbx_state";
            this.tbx_state.ReadOnly = true;
            this.tbx_state.Size = new System.Drawing.Size(458, 250);
            this.tbx_state.TabIndex = 4;
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox6.Controls.Add(this.textBox4);
            this.groupBox6.Controls.Add(this.tbxWarnings);
            this.groupBox6.Location = new System.Drawing.Point(0, 280);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(940, 277);
            this.groupBox6.TabIndex = 9;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "警告信息";
            // 
            // tbxWarnings
            // 
            this.tbxWarnings.BackColor = System.Drawing.SystemColors.Control;
            this.tbxWarnings.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxWarnings.Location = new System.Drawing.Point(3, 17);
            this.tbxWarnings.Multiline = true;
            this.tbxWarnings.Name = "tbxWarnings";
            this.tbxWarnings.ReadOnly = true;
            this.tbxWarnings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxWarnings.Size = new System.Drawing.Size(934, 257);
            this.tbxWarnings.TabIndex = 0;
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox4.Location = new System.Drawing.Point(3, 17);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox4.Size = new System.Drawing.Size(934, 257);
            this.textBox4.TabIndex = 1;
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // tbx_task
            // 
            this.tbx_task.Location = new System.Drawing.Point(457, 20);
            this.tbx_task.Multiline = true;
            this.tbx_task.Name = "tbx_task";
            this.tbx_task.ReadOnly = true;
            this.tbx_task.Size = new System.Drawing.Size(486, 250);
            this.tbx_task.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Location = new System.Drawing.Point(0, 148);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(946, 629);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Robot上报信息+手动操作区";
            // 
            // RobotDeviceUserInterface
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(946, 766);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.MaximumSize = new System.Drawing.Size(962, 805);
            this.MinimumSize = new System.Drawing.Size(962, 805);
            this.Name = "RobotDeviceUserInterface";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RobotDeviceUserInterface";
            this.Load += new System.EventHandler(this.RobotDeviceUserInterface_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnLock;
        private System.Windows.Forms.Button btnUnlock;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label lbl_ConnState;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lab_IPAddress;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label lblTaskId;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblDeviceType;
        private System.Windows.Forms.Label lblDeviceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblIsAlarm;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 初始化ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wCS完成ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 异常ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox cbx_loadTask;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_manual;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbx_task;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox tbxWarnings;
        private System.Windows.Forms.TextBox tbx_state;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}