using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wcs;
using Wcs.Framework;
using Wcs.Framework.EventBus;
using Wcs.Framework.Events;

namespace ZHQXC.AlarmPool
{
    [WcsPluginInfo(typeof(HistoryWarningRecordPlugin), "历史报警", "Sineva", "2023年4月", "", false, "报警记录", "报警查询", 6, 0, 2)]
    public class HistoryWarningRecordPlugin : Wcs.WcsPlugin
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public override bool Initialization(WcsContext context)
        {
            var barButtonItem = new BarButtonItem();
            barButtonItem.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItem.Caption = "历史报警";
            barButtonItem.Name = $"barBtn_{this.GetType().Name.ToLower()}";
            barButtonItem.ItemClick += tsmi_Click;
            this.barButtonItems = new BarButtonItem[] { barButtonItem };

            return base.Initialization(context);
        }

        [Wcs.Security.WcsPermission(System.Security.Permissions.SecurityAction.Demand, OperationName = "报警记录\\历史报警")]
        void tsmi_Click(object sender, EventArgs e)
        {
            try
            {
                this.Context.Application.MainForm.OpenNewForm<frmHistoryWarningRecordMain>("历史报警");
            }
            catch (System.Security.SecurityException securityException)
            {
                _logger.Error1(securityException, this);
                XtraMessageBox.Show(securityException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is System.Security.SecurityException)
                {
                    XtraMessageBox.Show(ex.InnerException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                _logger.Error1(ex, this);
            }
        }
    }
}
