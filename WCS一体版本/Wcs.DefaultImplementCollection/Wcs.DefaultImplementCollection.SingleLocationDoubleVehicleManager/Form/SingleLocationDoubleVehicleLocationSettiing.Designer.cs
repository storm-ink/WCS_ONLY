
namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    partial class SingleLocationDoubleVehicleLocationSettiing
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
            this.lv_select = new System.Windows.Forms.ListView();
            this.lv_stations = new System.Windows.Forms.ListView();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.lv_group = new System.Windows.Forms.ListView();
            this.btn_addGroup = new System.Windows.Forms.Button();
            this.btn_deleteGroup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lv_select
            // 
            this.lv_select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lv_select.HideSelection = false;
            this.lv_select.Location = new System.Drawing.Point(181, 28);
            this.lv_select.Name = "lv_select";
            this.lv_select.Size = new System.Drawing.Size(185, 401);
            this.lv_select.TabIndex = 0;
            this.lv_select.UseCompatibleStateImageBehavior = false;
            this.lv_select.View = System.Windows.Forms.View.List;
            // 
            // lv_stations
            // 
            this.lv_stations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lv_stations.HideSelection = false;
            this.lv_stations.Location = new System.Drawing.Point(412, 28);
            this.lv_stations.Name = "lv_stations";
            this.lv_stations.Size = new System.Drawing.Size(200, 401);
            this.lv_stations.TabIndex = 1;
            this.lv_stations.UseCompatibleStateImageBehavior = false;
            this.lv_stations.View = System.Windows.Forms.View.List;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel1.Location = new System.Drawing.Point(378, 155);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(22, 21);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "<";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(240, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "分配列表";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(478, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "站点列表";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(527, 435);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "返回";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(446, 435);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "设置";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel2.Location = new System.Drawing.Point(378, 189);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(22, 21);
            this.linkLabel2.TabIndex = 7;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = ">";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel3.Location = new System.Drawing.Point(373, 220);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(34, 21);
            this.linkLabel3.TabIndex = 8;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "<<";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // linkLabel4
            // 
            this.linkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel4.Location = new System.Drawing.Point(373, 250);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(34, 21);
            this.linkLabel4.TabIndex = 9;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = ">>";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(59, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "分组列表";
            // 
            // lv_group
            // 
            this.lv_group.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lv_group.HideSelection = false;
            this.lv_group.Location = new System.Drawing.Point(8, 28);
            this.lv_group.MultiSelect = false;
            this.lv_group.Name = "lv_group";
            this.lv_group.Size = new System.Drawing.Size(160, 401);
            this.lv_group.TabIndex = 10;
            this.lv_group.UseCompatibleStateImageBehavior = false;
            this.lv_group.View = System.Windows.Forms.View.List;
            this.lv_group.SelectedIndexChanged += new System.EventHandler(this.lv_group_SelectedIndexChanged);
            // 
            // btn_addGroup
            // 
            this.btn_addGroup.Location = new System.Drawing.Point(8, 435);
            this.btn_addGroup.Name = "btn_addGroup";
            this.btn_addGroup.Size = new System.Drawing.Size(104, 23);
            this.btn_addGroup.TabIndex = 12;
            this.btn_addGroup.Text = "新增站点分组";
            this.btn_addGroup.UseVisualStyleBackColor = true;
            this.btn_addGroup.Click += new System.EventHandler(this.btn_addGroup_Click);
            // 
            // btn_deleteGroup
            // 
            this.btn_deleteGroup.Location = new System.Drawing.Point(118, 435);
            this.btn_deleteGroup.Name = "btn_deleteGroup";
            this.btn_deleteGroup.Size = new System.Drawing.Size(107, 23);
            this.btn_deleteGroup.TabIndex = 13;
            this.btn_deleteGroup.Text = "删除站点分组";
            this.btn_deleteGroup.UseVisualStyleBackColor = true;
            this.btn_deleteGroup.Click += new System.EventHandler(this.btn_deleteGroup_Click);
            // 
            // SingleLocationDoubleVehicleLocationSettiing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 465);
            this.Controls.Add(this.btn_deleteGroup);
            this.Controls.Add(this.btn_addGroup);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lv_group);
            this.Controls.Add(this.linkLabel4);
            this.Controls.Add(this.linkLabel3);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.lv_stations);
            this.Controls.Add(this.lv_select);
            this.MaximumSize = new System.Drawing.Size(631, 504);
            this.MinimumSize = new System.Drawing.Size(631, 504);
            this.Name = "SingleLocationDoubleVehicleLocationSettiing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SingleLocationDoubleVehicleLocationSettiing";
            this.Load += new System.EventHandler(this.SingleLocationDoubleVehicleLocationSettiing_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lv_select;
        private System.Windows.Forms.ListView lv_stations;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView lv_group;
        private System.Windows.Forms.Button btn_addGroup;
        private System.Windows.Forms.Button btn_deleteGroup;
    }
}