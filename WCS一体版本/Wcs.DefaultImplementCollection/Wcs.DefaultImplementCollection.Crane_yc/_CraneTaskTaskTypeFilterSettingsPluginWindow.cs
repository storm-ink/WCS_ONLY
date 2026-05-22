using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NHibernate;
using NHibernate.Linq;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public partial class _CraneTaskTaskTypeFilterSettingsPluginWindow : Form
    {
        public _CraneTaskTaskTypeFilterSettingsPluginWindow()
        {
            InitializeComponent();

            init();
        }

        private void _CraneTaskTaskTypeFilterSettingsPluginWindow_Load(object sender, EventArgs e)
        {
            
        }

        void init()
        {
            try
            {
                String[] taskTypes;
                using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(IsolationLevel.ReadUncommitted))
                {
                    taskTypes = unitOfWork
                        .session
                        .Query<Task>()
                        .Where(x => x.TaskType != null && x.TaskType!="")
                        .GroupBy(x => x.TaskType)
                        .Select(x => x.Key)
                        .ToArray();

                    unitOfWork.Commit();
                }

                cbxDevices.Items.Clear();
                cbxDevices.Items.Add("");
                int i = 0;
                int height = this.Height;
                foreach (var craneDevice in Wcs.Framework
                    .Cfg
                    .WcsConfiguration
                    .Instance
                    .DeviceCollection
                    .ParticularDeviceCollection
                    .SelectMany(x => x.DeviceElements)
                    .Where(x => x.Device is CraneDevice)
                    .Select(x => x.Device as CraneDevice)
                    .OrderBy(x => x.No)
                    )
                {
                    cbxDevices.Items.Add(craneDevice.Name);

                    var gb = new GroupBox();
                    gb.Size = new Size(938, 70);
                    var top = 40 + gb.Size.Height * i + 5 * i;
                    gb.Left = 12;
                    gb.Top = top;
                    gb.Text = craneDevice.Name;

                    height = gb.Top + gb.Height + 70;

                    var cbl = new CheckedListBox();
                    cbl.Size = new Size(914, 60);
                    cbl.Location = new Point(6, 12);
                    cbl.MultiColumn = true;
                    cbl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    i++;

                    var currentSetting =
                        CraneTaskTaskTypeEquipmentActionSchedulerFilter.GetTypes(craneDevice.Name);

                    cbl.Items.AddRange(taskTypes);

                    for (int j = 0; j < cbl.Items.Count; j++)
                    {
                        var item = Convert.ToString(cbl.Items[j]);
                        if (currentSetting.Any(x => String.Equals(x, item, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            cbl.SetItemChecked(j, true);
                        }
                        else
                        {
                            cbl.SetItemChecked(j, false);
                        }
                    }

                    gb.Controls.Add(cbl);

                    this.Controls.Add(gb);
                }


                int btnCount = 0;
                for (int index = 1; index <= 5; index++)
                {
                    var key = "按钮" + index;
                    var v = Wcs.Framework.Cfg
                        .WcsConfiguration
                        .Instance
                        .SettingCollection
                        .GetSetting<String>("/堆垛机任务类型过滤器/" + key, "");

                    if (String.IsNullOrWhiteSpace(v))
                    {
                        continue;
                    }

                    if (v.Split('|').Length != 2)
                    {
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(v.Split('|')[1]))
                    {
                        continue;
                    }

                    var types = v.Split('|')[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var name = v.Split('|')[0];
                    var btn = new Button();
                    btn.Text = name;
                    btn.Tag = types;
                    btn.Size = new Size(75, 23);
                    btn.Left = cbxDevices.Right+12 + btn.Width * btnCount + 12 * btnCount;
                    btn.Top = 120;
                    btn.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                    btnCount++;

                    btn.Click += Btn_Click;
                    this.Controls.Add(btn);
                }



                this.Height = height;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            try
            {
                var name = Convert.ToString(cbxDevices.SelectedItem);
                var btn = (Button)sender;
                var types = btn.Tag as string[];
                foreach (var gbx in this.Controls.Cast<Control>().Where(x => x is GroupBox))
                {
                    if(!string.IsNullOrWhiteSpace(name) && name != gbx.Text)
                    {
                        continue;
                    }

                    var cbl = gbx.Controls.Cast<Control>().FirstOrDefault(x => x is CheckedListBox) as CheckedListBox;

                    if (cbl == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < cbl.Items.Count; i++)
                    {
                        var item = Convert.ToString(cbl.Items[i]);
                        if (!String.IsNullOrWhiteSpace(item) && types.Any(x=>String.Equals(item,x,StringComparison.CurrentCultureIgnoreCase)))
                        {
                            cbl.SetItemChecked(i, true);
                        }
                        else
                        {
                            cbl.SetItemChecked(i, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var gbx in this.Controls.Cast<Control>().Where(x => x is GroupBox))
                {
                    var key = gbx.Text;
                    String[] v = new String[0];

                    var cbl = gbx.Controls.Cast<Control>().FirstOrDefault(x => x is CheckedListBox) as CheckedListBox;

                    if (cbl != null)
                    {
                        v = cbl.CheckedItems.Cast<String>().ToArray();
                    }

                    CraneTaskTaskTypeEquipmentActionSchedulerFilter.SetTypes(key, v);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }


    //public class _CraneTaskTaskTypeFilterSettingsPlugin : Wcs.WcsPlugin
    //{
    //    public override bool Initialization(WcsContext context)
    //    {
    //        if (Wcs.Framework
    //        .Cfg
    //        .WcsConfiguration
    //        .Instance
    //        .DeviceCollection
    //        .ParticularDeviceCollection
    //        .SelectMany(x => x.DeviceElements)
    //        .Where(x => x.Device is CraneDevice)
    //        .Select(x => x as TaskableDeviceElement)
    //        .Any(x =>
    //            x.EquipmentActionSchedulerElement
    //            .FilterElements
    //                .Any(fe => fe.EquipmentActionSchedulerFilter.GetType() == typeof(CraneTaskTaskTypeEquipmentActionSchedulerFilter))
    //             )
    //         )
    //        {
    //            var item = (System.Windows.Forms.ToolStripMenuItem)context.Application.GetMenu(WcsApplicationMenuType.Edit)
    //                .DropDownItems.Add("堆垛机任务类型过滤");

    //            item.Click += Item_Click;
    //        }

    //        return base.Initialization(context);
    //    }

    //    private void Item_Click(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            using (_CraneTaskTaskTypeFilterSettingsPluginWindow frm = new _CraneTaskTaskTypeFilterSettingsPluginWindow())
    //            {
    //                frm.ShowDialog();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            this.Context.Application.Logger.Error1(ex, this);
    //        }
    //    }
    //}
}
