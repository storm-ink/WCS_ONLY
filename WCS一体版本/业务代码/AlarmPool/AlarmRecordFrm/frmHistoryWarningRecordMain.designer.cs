namespace ZHQXC.AlarmPool
{
    partial class frmHistoryWarningRecordMain
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label9 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.dgvGrid = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWcsCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmLeve = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeviceErrorCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDevice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeviceType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOwerDevice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBeginingAt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndingAt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTotalTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.btnExport);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dtpEndDate);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.dtpStartDate);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.btnSearch);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(943, 84);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "报警查询";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "【空字符串】",
            "输送线",
            "堆垛机"});
            this.comboBox1.Location = new System.Drawing.Point(643, 23);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 32;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(862, 23);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(50, 23);
            this.btnExport.TabIndex = 31;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btn_Excel_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(528, 23);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(89, 21);
            this.numericUpDown1.TabIndex = 30;
            this.numericUpDown1.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(453, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 29;
            this.label5.Text = "返回条数";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "yyyy/MM/dd HH:mm";
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(281, 22);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.ShowCheckBox = true;
            this.dtpEndDate.Size = new System.Drawing.Size(151, 21);
            this.dtpEndDate.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(222, 26);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 26;
            this.label9.Text = "结束日期";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "yyyy/MM/dd HH:mm";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(65, 21);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.ShowCheckBox = true;
            this.dtpStartDate.Size = new System.Drawing.Size(151, 21);
            this.dtpStartDate.TabIndex = 25;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 24;
            this.label8.Text = "开始日期";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(785, 23);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(50, 23);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "查询";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // dgvGrid
            // 
            this.dgvGrid.AllowUserToAddRows = false;
            this.dgvGrid.AllowUserToDeleteRows = false;
            this.dgvGrid.AllowUserToResizeRows = false;
            this.dgvGrid.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dgvGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvGrid.ColumnHeadersHeight = 24;
            this.dgvGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colWcsCode,
            this.colAlarmCategory,
            this.colAlarmLeve,
            this.colAlarmName,
            this.colDeviceErrorCode,
            this.colDevice,
            this.colDeviceType,
            this.colOwerDevice,
            this.colBeginingAt,
            this.colEndingAt,
            this.colTotalTime,
            this.colRemarks});
            this.dgvGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGrid.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvGrid.Location = new System.Drawing.Point(5, 89);
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
            this.dgvGrid.Size = new System.Drawing.Size(943, 427);
            this.dgvGrid.TabIndex = 3;
            // 
            // colId
            // 
            this.colId.DataPropertyName = "Id";
            this.colId.HeaderText = "编号";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 80;
            // 
            // colWcsCode
            // 
            this.colWcsCode.DataPropertyName = "WcsAlarmCode";
            this.colWcsCode.HeaderText = "WCS报警码";
            this.colWcsCode.Name = "colWcsCode";
            this.colWcsCode.ReadOnly = true;
            // 
            // colAlarmCategory
            // 
            this.colAlarmCategory.DataPropertyName = "AlarmCategory";
            this.colAlarmCategory.HeaderText = "故障分类";
            this.colAlarmCategory.Name = "colAlarmCategory";
            this.colAlarmCategory.ReadOnly = true;
            this.colAlarmCategory.Width = 80;
            // 
            // colAlarmLeve
            // 
            this.colAlarmLeve.DataPropertyName = "AlarmLevel";
            this.colAlarmLeve.HeaderText = "故障等级";
            this.colAlarmLeve.Name = "colAlarmLeve";
            this.colAlarmLeve.ReadOnly = true;
            this.colAlarmLeve.Width = 80;
            // 
            // colAlarmName
            // 
            this.colAlarmName.DataPropertyName = "AlarmName";
            this.colAlarmName.HeaderText = "故障名称";
            this.colAlarmName.Name = "colAlarmName";
            this.colAlarmName.ReadOnly = true;
            this.colAlarmName.Width = 80;
            // 
            // colDeviceErrorCode
            // 
            this.colDeviceErrorCode.DataPropertyName = "DeviceErrorCode";
            this.colDeviceErrorCode.HeaderText = "设备故障码";
            this.colDeviceErrorCode.Name = "colDeviceErrorCode";
            this.colDeviceErrorCode.ReadOnly = true;
            this.colDeviceErrorCode.Width = 80;
            // 
            // colDevice
            // 
            this.colDevice.DataPropertyName = "Device";
            this.colDevice.HeaderText = "设备";
            this.colDevice.Name = "colDevice";
            this.colDevice.ReadOnly = true;
            // 
            // colDeviceType
            // 
            this.colDeviceType.DataPropertyName = "DeviceType";
            this.colDeviceType.HeaderText = "设备类型";
            this.colDeviceType.Name = "colDeviceType";
            this.colDeviceType.ReadOnly = true;
            // 
            // colOwerDevice
            // 
            this.colOwerDevice.DataPropertyName = "OwnerDevice";
            this.colOwerDevice.HeaderText = "所属设备";
            this.colOwerDevice.Name = "colOwerDevice";
            this.colOwerDevice.ReadOnly = true;
            // 
            // colBeginingAt
            // 
            this.colBeginingAt.DataPropertyName = "BeginingAt";
            this.colBeginingAt.HeaderText = "报警时间";
            this.colBeginingAt.Name = "colBeginingAt";
            this.colBeginingAt.ReadOnly = true;
            // 
            // colEndingAt
            // 
            this.colEndingAt.DataPropertyName = "EndingAt";
            this.colEndingAt.HeaderText = "结束时间";
            this.colEndingAt.Name = "colEndingAt";
            this.colEndingAt.ReadOnly = true;
            // 
            // colTotalTime
            // 
            this.colTotalTime.DataPropertyName = "TotalMilliseconds";
            this.colTotalTime.HeaderText = "时长";
            this.colTotalTime.Name = "colTotalTime";
            this.colTotalTime.ReadOnly = true;
            // 
            // colRemarks
            // 
            this.colRemarks.DataPropertyName = "Remarks";
            this.colRemarks.HeaderText = "备注";
            this.colRemarks.Name = "colRemarks";
            this.colRemarks.ReadOnly = true;
            // 
            // frmHistoryWarningRecordMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 521);
            this.Controls.Add(this.dgvGrid);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmHistoryWarningRecordMain";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "报警信息查询";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridView dgvGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWcsCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmLeve;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeviceErrorCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDevice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeviceType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOwerDevice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBeginingAt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndingAt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTotalTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
    }
}
