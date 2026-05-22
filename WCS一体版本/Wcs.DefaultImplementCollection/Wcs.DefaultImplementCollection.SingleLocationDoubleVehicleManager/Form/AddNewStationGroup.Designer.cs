
namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    partial class AddNewStationGroup
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
            this.tbx_groupName = new System.Windows.Forms.TextBox();
            this.btn_add = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbx_groupName
            // 
            this.tbx_groupName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.tbx_groupName.Location = new System.Drawing.Point(12, 12);
            this.tbx_groupName.Name = "tbx_groupName";
            this.tbx_groupName.Size = new System.Drawing.Size(171, 21);
            this.tbx_groupName.TabIndex = 0;
            // 
            // btn_add
            // 
            this.btn_add.Location = new System.Drawing.Point(189, 10);
            this.btn_add.Name = "btn_add";
            this.btn_add.Size = new System.Drawing.Size(55, 23);
            this.btn_add.TabIndex = 1;
            this.btn_add.Text = "Add";
            this.btn_add.UseVisualStyleBackColor = true;
            this.btn_add.Click += new System.EventHandler(this.btn_add_Click);
            // 
            // AddNewStationGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lime;
            this.ClientSize = new System.Drawing.Size(256, 45);
            this.Controls.Add(this.btn_add);
            this.Controls.Add(this.tbx_groupName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AddNewStationGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddNewStationGroup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbx_groupName;
        private System.Windows.Forms.Button btn_add;
    }
}