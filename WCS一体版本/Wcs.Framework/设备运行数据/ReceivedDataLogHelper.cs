using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Wcs.Framework
{
    public class ReceivedDataLogHelper:Wcs.WcsPlugin
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        //public override bool Initialization(WcsContext context)
        //{
        //    var menu = (System.Windows.Forms.ToolStripMenuItem)context.Application.GetMenu(WcsApplicationMenuType.Edit).DropDownItems.Add("记录设备运行数据");
        //    menu.Click += menu_Click;
        //    bool v = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<bool>("记录设备运行数据");
        //    menu.Checked = v;
        //    return base.Initialization(context);
        //}

        //[Wcs.Security.WcsPermission(System.Security.Permissions.SecurityAction.Demand, OperationName = "跟踪管理\\设置记录设备运行数据选项")]
        //void menu_Click(object sender, EventArgs e)
        //{
        //    var item = (System.Windows.Forms.ToolStripMenuItem)sender;
        //    var v = !item.Checked;
        //    item.Checked = v;
        //    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<bool>("记录设备运行数据", v);
        //}
        public static void Log(params ReceivedDataLog[] data)
        {
            try
            {
                if (!Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("记录设备运行数据", false))
                {
                    return;
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                {
                    using (NHDeviceTrackingDataUnitOfWork unitOfWork = new NHDeviceTrackingDataUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        foreach (var item in data)
                        {
                            if (item == null)
                            {
                                continue;
                            }

                            unitOfWork.session.Save(item);
                        }
                        unitOfWork.Commit();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                var item = data.Where(x => x != null).FirstOrDefault();

                _logger.Error1(new Exception(String.Format("{0} 保存失败。",item),ex), "数据日志");
            }
        }
    }
}
