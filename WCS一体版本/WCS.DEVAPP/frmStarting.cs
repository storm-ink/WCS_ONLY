using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Wcs;

namespace WCS.APP
{
    public partial class frmStarting : SplashScreen
    {
        frmMain mainWindow;
        String[] applicationStartupArgs;
        public frmStarting(frmMain main, String[] args)
        {
            InitializeComponent();
            mainWindow = main;
            this.applicationStartupArgs = args;
            //if (System.Configuration.ConfigurationManager.AppSettings["companyName"] != null
            //    && !String.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["companyName"]))
            //{
            //    lblCompanyName.Text = System.Configuration.ConfigurationManager.AppSettings["companyName"];
            //}

            //if (!String.IsNullOrWhiteSpace(Wcs.Framework.Cfg.WcsConfiguration.Area))
            //{
            //    this.Text = string.Format("{0}控制子系统(v{1})", Wcs.Framework.Cfg.WcsConfiguration.Area, Assembly.GetExecutingAssembly().GetName().Version);
            //    lblSoftName.Text = string.Format("{0}控制子系统", Wcs.Framework.Cfg.WcsConfiguration.Area);
            //}
            //else
            //{
            //    this.Text = string.Format("仓库设备控制系统(v{0})", Assembly.GetExecutingAssembly().GetName().Version);
            //    lblSoftName.Text = "WCS 仓库设备控制系统";
            //}
        }

        System.Threading.Timer tmr = null;
        private void frmStarting_Load(object sender, EventArgs e)
        {
            //string fileName = "./sineva.png";
            //FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            //byte[] bt = new byte[fs.Length];
            //fs.Close();
            //if (bt != null)
            //{
            //    Stream stream = new MemoryStream(bt);
            //    Bitmap bmtemp = new Bitmap(stream);
            //    Image img = new Bitmap(bmtemp, pictureEdit1.Width, pictureEdit1.Height); //指定图片显示尺寸与控件大小一样
            //    pictureEdit1.EditValue = img;
            //}

            lblExitApplication.Visible = false;
            tbxInfo.ReadOnly = true;
            tbxInfo.SelectionStart = 0;
            tbxInfo.SelectionLength = 0;
            lblVersion.Text = "版本号:v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            mainWindow.Logger.Trace1("版本号:v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(), this);

            tmr = new System.Threading.Timer((state) =>
            {
                tmr.Dispose();
                tmr = null;

                Thread td = new Thread(() =>
                {
                    bool loadConfigurationError = false;

                    this.Invoke(new MethodInvoker(() =>
                    {
                        if (this.applicationStartupArgs == null || this.applicationStartupArgs.Length == 0)
                        {
                            return;
                        }

                        try
                        {
                            foreach (var par in this.applicationStartupArgs.Where(x => x.Split('=').Length == 2).Select(x => x.Split('=')))
                            {
                                switch (par[0].ToUpper())
                                {
                                    case "CREATE_DATABASE":
                                        if (par[1] == "1" || par[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            if (MessageBox.Show("Wcs 正在尝试重新创建数据库，这会导致所有已存在的任务数据丢失且不可恢复!!！\n\n是否继续？", Application.ProductName,
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                                            {
                                                mainWindow.Logger.Trace1("开始创建数据库...", this);
                                                Wcs.Framework.DatabaseHelper.CreateDatabase();
                                                mainWindow.Logger.Info1("数据库创建成功", this);
                                            }
                                        }
                                        break;
                                    case "TRACE_SQL":
                                        if (par[1] == "1" || par[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            mainWindow.Logger.Trace1("添加 SQL 跟踪支持...", this);
                                            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
                                            mainWindow.Logger.Trace1("SQL 跟踪支持添加成功", this);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = "处理启动参数时发行错误：" + ex.ToString();
                            this.Invoke(new MethodInvoker(() =>
                            {
                                tbxInfo.ScrollBars = RichTextBoxScrollBars.Vertical;
                                tbxInfo.ForeColor = Color.Red;
                                tbxInfo.Text = msg;
                                lblExitApplication.Visible = true;
                            }));
                            mainWindow.Logger.Error1(ex, this);
                            loadConfigurationError = true;
                            return;
                        }
                    }));

                    if (loadConfigurationError)
                    {
                        return;
                    }

                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            string msg = string.Format("读取配置数据...");
                            mainWindow.Logger.Trace1(msg, this);
                            tbxInfo.Text = msg;

                            Wcs.Framework.Cfg.WcsConfiguration.StartApplication(mainWindow);
                        }
                        catch (Exception ex)
                        {
                            string msg = "读取配置数据时发生错误：" + ex.ToString();
                            this.Invoke(new MethodInvoker(() =>
                            {
                                tbxInfo.ScrollBars = RichTextBoxScrollBars.Vertical;
                                tbxInfo.ForeColor = Color.Red;
                                tbxInfo.Text = msg;
                                lblExitApplication.Visible = true;
                            }));
                            mainWindow.Logger.Error1(ex, this);
                            loadConfigurationError = true;
                            return;
                        }
                    }));

                    if (loadConfigurationError)
                    {
                        return;
                    }

                    var plugins = mainWindow.Context.FindPlugins(Application.StartupPath);
                    mainWindow.Logger.Trace1(string.Format("找到 {0} 个插件，准备加载...", plugins.Count), this);
                    foreach (var item in plugins)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                string msg = string.Format("加载 {0}({1})...", item.Key.FullName, item.Value.Name);
                                mainWindow.Logger.Trace1(msg, this, item);
                                tbxInfo.Text = msg;

                                mainWindow.Context.LoadPlugin(item.Key);
                            }
                            catch (Exception ex)
                            {
                                mainWindow.Logger.Warn1(string.Format("加载 {0}({1})时发生错误：{2}", item.Key.FullName, item.Value.Name, ex), this, item);

                                string msg = string.Format("加载 {0}({1})时发生错误：{2}", item.Key.FullName, item.Value.Name, ex);
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    tbxInfo.ScrollBars = RichTextBoxScrollBars.Vertical;
                                    tbxInfo.ForeColor = Color.Red;
                                    tbxInfo.Text = msg;
                                    lblExitApplication.Visible = true;
                                }));
                                mainWindow.Logger.Error1(ex, this);
                                loadConfigurationError = true;
                                return;
                            }
                        }));
                    }
                    mainWindow.Logger.Trace1("插件加载完成.", this);

                    this.Invoke(new MethodInvoker(() =>
                    {
                        string msg = "系统加载完成，请稍候...";

                        mainWindow.Logger.Trace1(msg, this);
                        tbxInfo.Text = msg;

                        this.Close();
                    }));
                });
                td.Name = "系统初始化";
                td.StartAndManaged();
            }, tmr, 1000, Timeout.Infinite);
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
        }

        private void lblExitApplication_Click(object sender, EventArgs e)
        {
            this.Close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Application.Exit();
        }
    }
}