
namespace Wcs.DefaultImplementCollection.AGV
{
    partial class SinevaAGVUserInterfaceFrm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbTaskType = new System.Windows.Forms.ComboBox();
            this.createTask = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.endLevel = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.beginLevel = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.endloc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.beginloc = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvGrid = new System.Windows.Forms.DataGridView();
            this.a_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_interCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_begionLoc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_endLoc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_interType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_state = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_createDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_useDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_finishDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_remark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_category = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_agvId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_sort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.a_SalverType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_Rush = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrid)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbTaskType);
            this.groupBox1.Controls.Add(this.createTask);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.endLevel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.beginLevel);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.endloc);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.beginloc);
            this.groupBox1.Location = new System.Drawing.Point(12, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(987, 69);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "手动任务";
            // 
            // cmbTaskType
            // 
            this.cmbTaskType.FormattingEnabled = true;
            this.cmbTaskType.Items.AddRange(new object[] {
            "空托接料",
            "满托回位",
            "取货放货",
            "空托回库",
            "配料出库",
            "配料回库",
            "上料出库",
            "上料回库",
            "空托入库",
            "小车行走"});
            this.cmbTaskType.Location = new System.Drawing.Point(737, 30);
            this.cmbTaskType.Name = "cmbTaskType";
            this.cmbTaskType.Size = new System.Drawing.Size(72, 20);
            this.cmbTaskType.TabIndex = 9;
            // 
            // createTask
            // 
            this.createTask.Location = new System.Drawing.Point(853, 30);
            this.createTask.Name = "createTask";
            this.createTask.Size = new System.Drawing.Size(75, 21);
            this.createTask.TabIndex = 12;
            this.createTask.Text = "创建任务";
            this.createTask.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(650, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "任务类型：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(465, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "终点层级：";
            // 
            // endLevel
            // 
            this.endLevel.Location = new System.Drawing.Point(550, 30);
            this.endLevel.Name = "endLevel";
            this.endLevel.Size = new System.Drawing.Size(75, 21);
            this.endLevel.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "起点层级：";
            // 
            // beginLevel
            // 
            this.beginLevel.Location = new System.Drawing.Point(238, 30);
            this.beginLevel.Name = "beginLevel";
            this.beginLevel.Size = new System.Drawing.Size(62, 21);
            this.beginLevel.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(304, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "终点：";
            // 
            // endloc
            // 
            this.endloc.Location = new System.Drawing.Point(377, 30);
            this.endloc.Name = "endloc";
            this.endloc.Size = new System.Drawing.Size(75, 21);
            this.endloc.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "起点：";
            // 
            // beginloc
            // 
            this.beginloc.Location = new System.Drawing.Point(81, 30);
            this.beginloc.Name = "beginloc";
            this.beginloc.Size = new System.Drawing.Size(69, 21);
            this.beginloc.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dgvGrid);
            this.groupBox2.Location = new System.Drawing.Point(12, 199);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(987, 449);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "交互记录";
            // 
            // dgvGrid
            // 
            this.dgvGrid.AllowUserToAddRows = false;
            this.dgvGrid.AllowUserToDeleteRows = false;
            this.dgvGrid.AllowUserToOrderColumns = true;
            this.dgvGrid.AllowUserToResizeRows = false;
            this.dgvGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvGrid.ColumnHeadersHeight = 24;
            this.dgvGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.a_ID,
            this.a_interCode,
            this.a_begionLoc,
            this.a_endLoc,
            this.a_interType,
            this.a_state,
            this.a_createDatetime,
            this.a_useDatetime,
            this.a_finishDatetime,
            this.a_remark,
            this.a_category,
            this.a_agvId,
            this.a_sort,
            this.a_SalverType});
            this.dgvGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGrid.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvGrid.Location = new System.Drawing.Point(3, 17);
            this.dgvGrid.Name = "dgvGrid";
            this.dgvGrid.ReadOnly = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvGrid.RowHeadersWidth = 30;
            this.dgvGrid.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(248)))));
            this.dgvGrid.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgvGrid.RowTemplate.Height = 23;
            this.dgvGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGrid.Size = new System.Drawing.Size(981, 429);
            this.dgvGrid.TabIndex = 8;
            // 
            // a_ID
            // 
            this.a_ID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_ID.DataPropertyName = "ID";
            this.a_ID.HeaderText = "编号";
            this.a_ID.MinimumWidth = 6;
            this.a_ID.Name = "a_ID";
            this.a_ID.ReadOnly = true;
            this.a_ID.Width = 54;
            // 
            // a_interCode
            // 
            this.a_interCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_interCode.DataPropertyName = "interCode";
            this.a_interCode.HeaderText = "任务号";
            this.a_interCode.MinimumWidth = 6;
            this.a_interCode.Name = "a_interCode";
            this.a_interCode.ReadOnly = true;
            this.a_interCode.Width = 66;
            // 
            // a_begionLoc
            // 
            this.a_begionLoc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_begionLoc.DataPropertyName = "begionLoc";
            this.a_begionLoc.HeaderText = "起点";
            this.a_begionLoc.MinimumWidth = 6;
            this.a_begionLoc.Name = "a_begionLoc";
            this.a_begionLoc.ReadOnly = true;
            this.a_begionLoc.Width = 54;
            // 
            // a_endLoc
            // 
            this.a_endLoc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_endLoc.DataPropertyName = "endLoc";
            this.a_endLoc.HeaderText = "终点";
            this.a_endLoc.MinimumWidth = 6;
            this.a_endLoc.Name = "a_endLoc";
            this.a_endLoc.ReadOnly = true;
            this.a_endLoc.Width = 54;
            // 
            // a_interType
            // 
            this.a_interType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_interType.DataPropertyName = "interType";
            this.a_interType.HeaderText = "类型";
            this.a_interType.MinimumWidth = 6;
            this.a_interType.Name = "a_interType";
            this.a_interType.ReadOnly = true;
            this.a_interType.Width = 54;
            // 
            // a_state
            // 
            this.a_state.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_state.DataPropertyName = "state";
            this.a_state.HeaderText = "状态";
            this.a_state.MinimumWidth = 6;
            this.a_state.Name = "a_state";
            this.a_state.ReadOnly = true;
            this.a_state.Width = 54;
            // 
            // a_createDatetime
            // 
            this.a_createDatetime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_createDatetime.DataPropertyName = "createDatetime";
            this.a_createDatetime.HeaderText = "创建时间";
            this.a_createDatetime.MinimumWidth = 6;
            this.a_createDatetime.Name = "a_createDatetime";
            this.a_createDatetime.ReadOnly = true;
            this.a_createDatetime.Width = 78;
            // 
            // a_useDatetime
            // 
            this.a_useDatetime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_useDatetime.DataPropertyName = "useDatetime";
            this.a_useDatetime.HeaderText = "开始时间";
            this.a_useDatetime.MinimumWidth = 6;
            this.a_useDatetime.Name = "a_useDatetime";
            this.a_useDatetime.ReadOnly = true;
            this.a_useDatetime.Width = 78;
            // 
            // a_finishDatetime
            // 
            this.a_finishDatetime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_finishDatetime.DataPropertyName = "finishDatetime";
            this.a_finishDatetime.HeaderText = "完成时间";
            this.a_finishDatetime.MinimumWidth = 6;
            this.a_finishDatetime.Name = "a_finishDatetime";
            this.a_finishDatetime.ReadOnly = true;
            this.a_finishDatetime.Width = 78;
            // 
            // a_remark
            // 
            this.a_remark.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_remark.DataPropertyName = "remark";
            this.a_remark.HeaderText = "备注";
            this.a_remark.MinimumWidth = 6;
            this.a_remark.Name = "a_remark";
            this.a_remark.ReadOnly = true;
            this.a_remark.Width = 54;
            // 
            // a_category
            // 
            this.a_category.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_category.DataPropertyName = "category";
            this.a_category.HeaderText = "种类";
            this.a_category.MinimumWidth = 6;
            this.a_category.Name = "a_category";
            this.a_category.ReadOnly = true;
            this.a_category.Width = 54;
            // 
            // a_agvId
            // 
            this.a_agvId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_agvId.DataPropertyName = "agvId";
            this.a_agvId.HeaderText = "车号";
            this.a_agvId.MinimumWidth = 6;
            this.a_agvId.Name = "a_agvId";
            this.a_agvId.ReadOnly = true;
            this.a_agvId.Width = 54;
            // 
            // a_sort
            // 
            this.a_sort.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_sort.DataPropertyName = "sort";
            this.a_sort.HeaderText = "任务类别";
            this.a_sort.MinimumWidth = 6;
            this.a_sort.Name = "a_sort";
            this.a_sort.ReadOnly = true;
            this.a_sort.Width = 78;
            // 
            // a_SalverType
            // 
            this.a_SalverType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.a_SalverType.DataPropertyName = "SalverType";
            this.a_SalverType.HeaderText = "托盘类型";
            this.a_SalverType.MinimumWidth = 6;
            this.a_SalverType.Name = "a_SalverType";
            this.a_SalverType.ReadOnly = true;
            this.a_SalverType.Width = 78;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.btn_Rush);
            this.groupBox3.Location = new System.Drawing.Point(1005, 32);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(127, 633);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "功能操作区";
            // 
            // button1
            // 
            this.button1.AllowDrop = true;
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(14, 413);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(76, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "历史任务";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btn_Rush
            // 
            this.btn_Rush.AllowDrop = true;
            this.btn_Rush.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Rush.Location = new System.Drawing.Point(6, 21);
            this.btn_Rush.Name = "btn_Rush";
            this.btn_Rush.Size = new System.Drawing.Size(115, 36);
            this.btn_Rush.TabIndex = 0;
            this.btn_Rush.Text = "当前任务";
            this.btn_Rush.UseVisualStyleBackColor = true;
            // 
            // SinevaAGVUserInterfaceFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1271, 738);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SinevaAGVUserInterfaceFrm";
            this.Text = "SinevaAGVUserInterfaceFrm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrid)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbTaskType;
        private System.Windows.Forms.Button createTask;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox endLevel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox beginLevel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox endloc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox beginloc;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_interCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_begionLoc;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_endLoc;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_interType;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_state;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_createDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_useDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_finishDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_remark;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_category;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_agvId;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_sort;
        private System.Windows.Forms.DataGridViewTextBoxColumn a_SalverType;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_Rush;
    }
}