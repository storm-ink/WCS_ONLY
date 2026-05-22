using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using Wcs;

namespace WCS.APP
{
    public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm, IWcsApplication
    {
        public WcsContext Context;

        public frmMain(String[] args)
        {
            this.Logger = NLog.LogManager.GetCurrentClassLogger();

            InitializeComponent();

            if (!String.IsNullOrWhiteSpace(Wcs.Framework.Cfg.WcsConfiguration.Area))
            {
                this.Text = string.Format("{0}控制子系统(v{1})", Wcs.Framework.Cfg.WcsConfiguration.Area, Assembly.GetExecutingAssembly().GetName().Version);

                createNotify();

                timer1.Enabled = true;
            }
            else
            {
                this.Text = string.Format("仓库设备控制系统(v{0})", Assembly.GetExecutingAssembly().GetName().Version);
                timer1.Enabled = false;
            }

            Context = new WcsContext(this);

            this.Logger.Trace1(string.Format("{0} 正在启动……", this.Text), this);

            new frmStarting(this, args).ShowDialog();

            Initialize();
            //执行自启动项
            //Wcs.Framework.Cfg.WcsConfiguration.StartApplication(this);

            this.Logger.Info1(string.Format("{0} 启动成功", this.Text), this);
        }

        private void Initialize()
        {
            IcoConfig = LoadIcoConfig();

            Dictionary<RibbonPage, int> ribbonPages = new Dictionary<RibbonPage, int>();
            Dictionary<KeyValuePair<RibbonPageGroup, RibbonPage>, int> ribbonPageGroups = new Dictionary<KeyValuePair<RibbonPageGroup, RibbonPage>, int>();
            Dictionary<KeyValuePair<BarButtonItem, KeyValuePair<RibbonPageGroup, RibbonPage>>, int> barButtonItems = new Dictionary<KeyValuePair<BarButtonItem, KeyValuePair<RibbonPageGroup, RibbonPage>>, int>();
            foreach (var plug in this.Context.LoadedPlugins)
            {
                if (plug == null || plug.barButtonItems == null)
                    continue;
                var type = plug.GetType();
                var attr = (WcsPluginInfo)type.GetCustomAttributes(typeof(WcsPluginInfo), false).FirstOrDefault();
                if (attr != null && type.IsPublic && type.IsSubclassOf(typeof(WcsPlugin)))
                {
                    RibbonPage ribbonPage;
                    if (ribbonPages.Any(x => x.Key.Text == attr.DevexpressRibbonInfo.RibbonPage))
                        ribbonPage = ribbonPages.First(x => x.Key.Text == attr.DevexpressRibbonInfo.RibbonPage).Key;
                    else
                    {
                        ribbonPage = new RibbonPage();
                        ribbonPage.Text = attr.DevexpressRibbonInfo.RibbonPage;
                        if (MenuGetSVGIcon(ribbonPage.Text, out DevExpress.Utils.Svg.SvgImage svgImage))
                            ribbonPage.ImageOptions.SvgImage = svgImage;
                        ribbonPages.Add(ribbonPage, attr.DevexpressRibbonInfo.RibbonPageIndex);
                    }

                    RibbonPageGroup ribbonPageGroup;
                    if (ribbonPageGroups.Any(x => x.Key.Key.Text == attr.DevexpressRibbonInfo.RibbonPageGroup))
                        ribbonPageGroup = ribbonPageGroups.FirstOrDefault(x => x.Key.Key.Text == attr.DevexpressRibbonInfo.RibbonPageGroup).Key.Key;
                    else
                    {
                        ///直接给RibbonPageGroup text参数 可以保证text完全显示
                        ribbonPageGroup = new RibbonPageGroup(attr.DevexpressRibbonInfo.RibbonPageGroup);
                        ribbonPageGroup.AllowTextClipping = true;
                        ribbonPageGroup.ShowCaptionButton = false;
                        ribbonPageGroups.Add(new KeyValuePair<RibbonPageGroup, RibbonPage>(ribbonPageGroup, ribbonPage), attr.DevexpressRibbonInfo.RibbonPageGroupIndex);
                    }

                    foreach (var barBtnItem in plug.barButtonItems)
                    {
                        if (MenuGetSVGIcon(barBtnItem.Caption, out DevExpress.Utils.Svg.SvgImage svgImage))
                            barBtnItem.ImageOptions.SvgImage = svgImage;
                        barButtonItems.Add(new KeyValuePair<BarButtonItem, KeyValuePair<RibbonPageGroup, RibbonPage>>(barBtnItem, new KeyValuePair<RibbonPageGroup, RibbonPage>(ribbonPageGroup, ribbonPage)), attr.DevexpressRibbonInfo.RibbonPageItemIndex);
                    }
                }
            }

            #region homePage 
            RibbonPage ribbonPage1;
            if (ribbonPages.Any(x => x.Key.Text == "欢迎页"))
                ribbonPage1 = ribbonPages.First(x => x.Key.Text == "欢迎页").Key;
            else
            {
                ribbonPage1 = new RibbonPage();
                ribbonPage1.Text = "欢迎页";
                if (MenuGetSVGIcon(ribbonPage1.Text, out DevExpress.Utils.Svg.SvgImage svgImage1))
                    ribbonPage1.ImageOptions.SvgImage = svgImage1;
                ribbonPages.Add(ribbonPage1, 0);
            }

            RibbonPageGroup ribbonPageGroup1;
            if (ribbonPageGroups.Any(x => x.Key.Key.Text == "欢迎页"))
                ribbonPageGroup1 = ribbonPageGroups.FirstOrDefault(x => x.Key.Key.Text == "欢迎页").Key.Key;
            else
            {
                ///直接给RibbonPageGroup text参数 可以保证text完全显示
                ribbonPageGroup1 = new RibbonPageGroup("欢迎页");
                ribbonPageGroup1.AllowTextClipping = true;
                ribbonPageGroup1.ShowCaptionButton = false;
                ribbonPageGroups.Add(new KeyValuePair<RibbonPageGroup, RibbonPage>(ribbonPageGroup1, ribbonPage1), 0);
            }

            BarButtonItem barButtonItem1 = new BarButtonItem();
            barButtonItem1.RibbonStyle = RibbonItemStyles.Large;
            barButtonItem1.Caption = "系统退出";
            barButtonItem1.Name = "barBtn_exit";
            barButtonItem1.ItemClick += BarButtonItem1_ItemClick;
            if (MenuGetSVGIcon(barButtonItem1.Caption, out DevExpress.Utils.Svg.SvgImage svgImage2))
                barButtonItem1.ImageOptions.SvgImage = svgImage2;
            barButtonItems.Add(new KeyValuePair<BarButtonItem, KeyValuePair<RibbonPageGroup, RibbonPage>>(barButtonItem1, new KeyValuePair<RibbonPageGroup, RibbonPage>(ribbonPageGroup1, ribbonPage1)), 99);
            #endregion

            ///排序后加入
            foreach (var rbp in ribbonPages.OrderBy(x => x.Value))
            {
                RibbonPage ribbonPage = rbp.Key;
                this.ribbon.Pages.Add(ribbonPage);
                var groups = ribbonPageGroups.Where(x => x.Key.Value.Text == ribbonPage.Text).OrderBy(x => x.Value).Select(x => x.Key.Key).ToArray();
                ribbonPage.Groups.AddRange(groups);
                foreach (var ribbonPageGroup in groups)
                {
                    var barBtnIems = barButtonItems
                                    .Where(x => x.Key.Value.Value.Text == ribbonPage.Text && x.Key.Value.Key.Text == ribbonPageGroup.Text)
                                    .OrderBy(x => x.Value)
                                    .ThenBy(x => x.Key.Key.Id)
                                    .Select(x => x.Key.Key).ToArray();
                    ribbonPageGroup.ItemLinks.AddRange(barBtnIems);
                }
                this.ribbon.Pages.Add(ribbonPage);
            }
        }

        void createNotify()
        {
            NotifyIcon ni = new NotifyIcon(this.components);
            ni.Icon = createIcon();
            ni.Text = Wcs.Framework.Cfg.WcsConfiguration.Area;
            ni.Visible = true;

            ni.DoubleClick += (sender, e) =>
            {
                this.Visible = true;
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                this.Show();
            };

            this.Icon = ni.Icon;

            NotifyIocn = ni;
        }
        NotifyIcon NotifyIocn { get; set; }

        Icon _ico;
        public Icon createIcon()
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.Clear(Color.Transparent);
                        g.FillEllipse(Brushes.Yellow, 0, 0, bmp.Width, bmp.Height);
                        g.DrawString(Wcs.Framework.Cfg.WcsConfiguration.Area.Substring(0, 2), new Font(this.Font.Name, 12f, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Red, new RectangleF(0, 0, bmp.Width, bmp.Height), sf);

                        using (Icon ico = Icon.FromHandle(bmp.GetHicon()))
                        {
                            return ico;
                        }
                    }
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //SaveIcoConfig();
            //IcoConfig = LoadIcoConfig();
            //HomePage();
            //TaskManagerPage();
            //_MappingHelper = new MappingHelper("D:\\WCSFileLogs\\MappingHelper.txt", "WCSHeartBeat");
            //timer2.Enabled = true;
        }

        public IcoConfig IcoConfig { get; set; }

        private void BarButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">name || showText</param>
        /// <param name="svgImage"></param>
        /// <returns></returns>
        bool MenuGetSVGIcon(string text, out DevExpress.Utils.Svg.SvgImage svgImage)
        {
            svgImage = null;
            if (IcoConfig == null)
                return false;
            var icoConfig = IcoConfig.Settings.FirstOrDefault(x => x.Name == text || x.ShowText == text);
            if (icoConfig == null)
                return false;
            else
            {
                if (File.Exists(icoConfig.Path))
                {
                    svgImage = DevExpress.Utils.Svg.SvgImage.FromFile(icoConfig.Path);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ico图标文件路径
        /// </summary>
        public string icoConfigFilePath
        {
            get
            {
                try
                {
                    return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("icoConfigFilePath", "./系统配置/基本配置/icoConfig.xml");
                }
                catch (Exception)
                {
                    return "./icoConfig.xml";
                }
            }
        }

        public Logger Logger { get; private set; }

        public RibbonStatusBar Status
        {
            get { return ribbonStatusBar; }
            //get { return ssMain; }
        }

        public ToolStrip Tool
        {
            get { return null; }
            //get { return mainTools; }
        }

        public Form MainForm
        {
            get
            {
                return this;
            }
        }

        public Control ButtomDock
        {
            get
            {
                return null;
                //return this.pnlButtomDock;
            }
        }

        IWcsApplication IWcsApplication.MainForm
        {
            get
            {
                return this;
            }
        }

        public IcoConfig LoadIcoConfig()
        {
            XmlDocument xml = new XmlDocument();
            if (!File.Exists(icoConfigFilePath))
                SaveIcoConfig();
            xml.Load(icoConfigFilePath);
            var strxml = xml.OuterXml;
            return xml.DESerializer<IcoConfig>(strxml);
        }

        public void SaveIcoConfig()
        {
            IcoConfig icoConfig = new IcoConfig();
            icoConfig.Settings = new List<Ico>()
            {
                new Ico(){ Name="home", ShowText="欢迎页", Path="./Ico/home_24.svg" },
                new Ico(){ Name="taskManager", ShowText="任务管理",Path="./Ico/taskManager_24.svg" },
                new Ico(){ Name="deviceManager", ShowText="设备管理",Path="./Ico/deivces_24.svg" },
                new Ico(){ Name="userManager", ShowText="日志管理",Path="./Ico/users_24.svg" },
                new Ico(){ Name="systemConfig", ShowText="当前任务",Path="./Ico/equalizer_24.svg" },
                new Ico(){ Name="systemSetting", ShowText="历史任务",Path="./Ico/setting-config_24.svg" },
                new Ico(){ Name="systemHelper", ShowText="手工任务",Path="./Ico/help_24.svg" },
                new Ico(){ Name="barBtn_exit", ShowText="系统退出",Path="./Ico/power_32.svg" },
                new Ico(){ Name="barBtn_taskList", ShowText="当前任务",Path="./Ico/tasklist_32.svg" },
                new Ico(){ Name="barBtn_historyTaskList", ShowText="历史任务",Path="./Ico/historyTaskList_32.svg" }
            };
            XmlDocument xml = new XmlDocument();
            string strxml = icoConfig.XmlSerialize<IcoConfig>();
            xml.LoadXml(strxml);
            xml.Save(icoConfigFilePath);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Logger.Warn1(string.Format("由于 {0} 原因，应用程序正在退出...", e.CloseReason), this);
            System.Threading.Thread.Sleep(500);

            if (Wcs.Security.WcsPermission.IsAdministratorMode)
            {

                if (MessageBox.Show("确定要退出 " + Application.ProductName + " 吗？", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                using (frmCloseReasonDialog frm = new frmCloseReasonDialog(this))
                {
                    if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            if (this.NotifyIocn != null)
            {
                this.NotifyIocn.Visible = false;
            }
            timer2.Enabled = false;
            this.Dispose();

            Process.GetCurrentProcess().Kill();
            Application.Exit();
        }

        public void OpenNewXtraForm<T>(string xtraTabPageName) where T : XtraForm, new()
        {
            try
            {
                //AGVCWaitingFormHelper.ShowWaitingForm(this);
                foreach (object obj in this.tcMain.TabPages)
                {
                    XtraTabPage tab = (XtraTabPage)obj;
                    bool flag = tab.Text.Trim() == xtraTabPageName;
                    if (flag)
                    {
                        this.tcMain.SelectedTabPage = tab;
                        //AGVCWaitingFormHelper.HideWaitingForm();
                        return;
                    }
                }
                XtraTabPage xpage = new XtraTabPage();
                xpage.Name = xtraTabPageName;
                xpage.Text = xtraTabPageName;
                xpage.Dock = DockStyle.Fill;
                xpage.ShowCloseButton = DevExpress.Utils.DefaultBoolean.True;

                var frm = new T();
                frm.TopLevel = false;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Parent = this;
                xpage.Controls.Add(frm);
                this.tcMain.TabPages.Add(xpage);
                frm.Dock = DockStyle.Fill;
                frm.Show();
                this.tcMain.ClosePageButtonShowMode = ClosePageButtonShowMode.InActiveTabPageHeaderAndOnMouseHover;
                this.tcMain.SelectedTabPage = xpage;
                //frmMonitor.MonitorExist = true;
                //AGVCWaitingFormHelper.HideWaitingForm();
            }
            catch (Exception ex)
            {
                //AGVCWaitingFormHelper.HideWaitingForm();
                XtraMessageBox.Show($"打开失败，异常消息{ex}");
            }
        }

        public void OpenNewForm<T>(string xtraTabPageName) where T : Form, new()
        {
            try
            {
                //AGVCWaitingFormHelper.ShowWaitingForm(this);
                foreach (object obj in this.tcMain.TabPages)
                {
                    XtraTabPage tab = (XtraTabPage)obj;
                    bool flag = tab.Text.Trim() == xtraTabPageName;
                    if (flag)
                    {
                        this.tcMain.SelectedTabPage = tab;
                        //AGVCWaitingFormHelper.HideWaitingForm();
                        return;
                    }
                }
                XtraTabPage xpage = new XtraTabPage();
                xpage.Name = xtraTabPageName;
                xpage.Text = xtraTabPageName;
                xpage.Dock = DockStyle.Fill;
                xpage.ShowCloseButton = DevExpress.Utils.DefaultBoolean.True;

                var frm = new T();
                frm.TopLevel = false;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Parent = this;
                xpage.Controls.Add(frm);
                this.tcMain.TabPages.Add(xpage);
                frm.Dock = DockStyle.Fill; 
                frm.Show();
                this.tcMain.ClosePageButtonShowMode = ClosePageButtonShowMode.InActiveTabPageHeaderAndOnMouseHover;
                this.tcMain.SelectedTabPage = xpage;
                xpage.ControlRemoved += Xpage_ControlRemoved;
                //frmMonitor.MonitorExist = true;
                //AGVCWaitingFormHelper.HideWaitingForm();
            }
            catch (Exception ex)
            {
                //AGVCWaitingFormHelper.HideWaitingForm();
                //XtraMessageBox.Show($"{ex.InnerException.Message}");
                if (ex.InnerException != null && ex.InnerException is System.Security.SecurityException)
                    throw ex.InnerException;
                else
                    MessageBox.Show($"打开失败，异常消息：{ex.Message}");
            }
        }

        private void Xpage_ControlRemoved(object sender, ControlEventArgs e)
        {
            XtraTabPage xtraTabPage = (XtraTabPage)sender;
            string name = xtraTabPage.Text;
            foreach (object obj in this.tcMain.TabPages)
            {
                XtraTabPage page = (XtraTabPage)obj;
                bool flag = page.Text == name;
                if (flag)
                {
                    foreach (var item in page.Controls)
                    {
                        if (item is IFrmClosed)
                        {
                            var frm = (IFrmClosed)item;
                            frm.FrmClosed();
                        }
                    }
                    this.tcMain.TabPages.Remove(page);
                    page.Dispose();
                    break;
                }
            }
            GC.Collect();
        }

        private void tcMain_CloseButtonClick(object sender, EventArgs e)
        {
            ClosePageButtonEventArgs EArg = (ClosePageButtonEventArgs)e;
            string name = EArg.Page.Text;
            foreach (object obj in this.tcMain.TabPages)
            {
                XtraTabPage page = (XtraTabPage)obj;
                bool flag = page.Text == name;
                if (flag)
                {
                    foreach (var item in page.Controls)
                    {
                        if (item is IFrmClosed)
                        {
                            var frm = (IFrmClosed)item;
                            frm.FrmClosed();
                        }
                    }
                    this.tcMain.TabPages.Remove(page);
                    page.Dispose();
                    break;
                }
            }
            GC.Collect();
        }

        public void ControlAdd(Control control)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Action<Control> act = (_control) =>
                    {
                        ControlAdd(_control);
                    };

                    //this.Invoke(act, args);
                    this.BeginInvoke(act, control);
                }
                else 
                {
                    this.Controls.Add(control);
                    this.Controls.SetChildIndex(tcMain, 0);
                    this.Controls.SetChildIndex(control, 1);
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_ico == null)
                {
                    _ico = createIcon();
                }

                if (this.Icon != _ico)
                {
                    this.Icon = _ico;
                }

                while (Application.OpenForms.Cast<Form>().Any(x => !x.Text.Contains(Wcs.Framework.Cfg.WcsConfiguration.Area)))
                {
                    var frm = (Form)Application.OpenForms.Cast<Form>().First(x => !x.Text.Contains(Wcs.Framework.Cfg.WcsConfiguration.Area));
                    frm.Icon = _ico;
                    frm.Text += "****" + Wcs.Framework.Cfg.WcsConfiguration.Area + "****";
                }
                var h = NativeMethods.GetForegroundWindow();
                if (Application.OpenForms.Cast<Form>().Any(x => x.Handle == h) || h == this.Handle)
                {
                    createTip();
                }
            }
            catch (Exception ex)
            {
                Logger.Error1(ex, this);
            }
        }
        StringFormat sf;
        Color _color = Color.Empty;
        void createTip()
        {
            try
            {
                if (sf == null)
                {
                    sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                }

                var rect = new Rectangle(this.Left, this.Top, this.ClientSize.Width, this.ClientSize.Height);

                IntPtr desk = NativeMethods.GetDesktopWindow();
                IntPtr deskDC = NativeMethods.GetDCEx(desk, IntPtr.Zero, 0x403);

                var newColor = NativeMethods.GetPixelColor(deskDC, rect.Left, rect.Top);

                if (newColor != _color)
                {
                    var g = Graphics.FromHdc(deskDC);
                    using (Brush brush = new SolidBrush(Color.FromArgb(30, Color.Red)))
                    {
                        g.DrawString(Wcs.Framework.Cfg.WcsConfiguration.Area, new Font("宋体", 50, FontStyle.Bold), brush, rect, sf);
                        g.FillRectangle(brush, rect.Left, rect.Top, 1, 1);
                    }


                    _color = NativeMethods.GetPixelColor(deskDC, rect.Left, rect.Top);

                    g.Dispose();
                }


                NativeMethods.ReleaseDC(desk, deskDC);
            }
            catch (Exception ex)
            {
                Logger.Error1(ex, this);
            }
        }

        //将指定文件映射到内存中，监控进程读取时，要用一样的内存映射名
        MappingHelper _MappingHelper = null;
        ////通过定时器定时写入变化的数据信号
        //private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    _MappingHelper.WriteString(DateTime.Now.ToString("yyyyMMddHHmmss"));
        //}

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_MappingHelper != null)
            {
                var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
                _MappingHelper.WriteString(dt);
                Console.WriteLine(dt);
            }
        }
    }
}