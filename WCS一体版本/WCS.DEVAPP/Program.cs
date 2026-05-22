using DevExpress.Skins;
using DevExpress.UserSkins;
using DevExpress.Utils.Drawing.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Wcs;

namespace WCS.APP
{
    static class Program
    {
        #region Win32 API 函数

        #endregion

        static Logger _logger = LogManager.GetCurrentClassLogger();

        //同步基元变量
        static System.Threading.Mutex instance;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            #region 单实例检测
            Boolean createdNew;
            //instance = new System.Threading.Mutex(true, "Wcs.App.exe", out createdNew);
            instance = new System.Threading.Mutex(true, typeof(Program).Assembly.CodeBase, out createdNew);
            if (!createdNew)
            {
                try
                {
                    var process = System.Diagnostics.Process
                        .GetProcessesByName("Wcs.App")
                        .Where(x => x.Id != Process.GetCurrentProcess().Id)
                        .FirstOrDefault(x => String.Equals(x.MainModule.FileName, new Uri(typeof(Program).Assembly.CodeBase).LocalPath));
                    if (process != null)
                    {
                        IntPtr wh = IntPtr.Zero;
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            wh = process.MainWindowHandle;
                        }
                        else
                        {
                            wh = NativeMethods.GetMainWindowHandle(Convert.ToUInt32(process.Id));
                        }

                        NativeMethods.PostMessage(wh, 0x400 + 1, 0x400 + 1, IntPtr.Zero);

                        //NativeMethods.ShowWindowAsync(wh, 1);

                        //NativeMethods.SetForegroundWindow(wh);
                        //if (NativeMethods.IsIconic(wh))
                        //{
                        //    NativeMethods.ShowWindowAsync(wh, NativeMethods.SW_RESTORE);
                        //}
                        //else
                        //{
                        //    NativeMethods.ShowWindowAsync(wh, NativeMethods.SW_SHOWNORMAL);
                        //}
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, null);
                }

                Application.Exit();
                MessageBox.Show("不允许重复启动程序，请从任务栏寻找已启动程序或者从任务管理器结束该进程任务后重试！","提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion

            if (args != null && args.Any(x => x.Equals("Debug", StringComparison.CurrentCultureIgnoreCase)))
            {
                NativeMethods.AllocConsole();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            //处理未捕获的异常
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);


            Application.Run(new frmMain(args));
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            try
            {
                _logger.Warn1(string.Format("{0} {1} 已关闭。用户：{2}",
                    Application.ProductName,
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    Wcs.Security.WcsPrincipal.CurrentPrincipal.IsEmpty ? "<Null>" : Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity.Name), null);
            }
            catch (Exception ex)
            {
                LogWriteToFileHelper.log("CurrentDomain_DomainUnload:严重错误！！！");
                LogWriteToFileHelper.log(ex);
                MessageBox.Show(ex.ToString(), "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(-1);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = (Exception)e.ExceptionObject;

                if (e.IsTerminating)
                {
                    LogWriteToFileHelper.log("CurrentDomain_UnhandledException:严重错误！！！" + exception.Message);
                    LogWriteToFileHelper.log(exception);

                    _logger.Error1(exception, null);

                    //string msg = String.Format("Wcs 发生未严重错误需要关闭(Domain)\n\n{0}", exception);

                    //MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    //Environment.Exit(-1);

                    //LogWriteToFileHelper.log(string.Format("{0} {1} 即将自动重启...",
                    //    Application.ProductName,
                    //    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));

                    //Application.Restart();

                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                else
                {
                    _logger.Error1(exception, null);

                    if (exception != null && exception is System.Security.SecurityException)
                    {
                        var securityException = (System.Security.SecurityException)exception;

                        MessageBox.Show(securityException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (exception != null && exception.InnerException != null && exception.InnerException is System.Security.SecurityException)
                    {

                        var securityException = (System.Security.SecurityException)exception.InnerException;

                        MessageBox.Show(securityException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriteToFileHelper.log(ex);

                if (e.IsTerminating)
                {
                    Application.Restart();

                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                else
                {
                    return;
                }
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                var exception = (Exception)e.Exception;

                _logger.Error1(exception, null);

                if (exception != null && exception is System.Security.SecurityException)
                {
                    var securityException = (System.Security.SecurityException)exception;

                    MessageBox.Show(securityException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (exception != null && exception.InnerException != null && exception.InnerException is System.Security.SecurityException)
                {

                    var securityException = (System.Security.SecurityException)exception.InnerException;

                    MessageBox.Show(securityException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LogWriteToFileHelper.log("Application_ThreadException 1" + exception.Message);
                LogWriteToFileHelper.log(exception);
            }
            catch (Exception ex)
            {
                LogWriteToFileHelper.log("Application_ThreadException 2" + ex.Message);
                LogWriteToFileHelper.log(ex);
            }
        }
    }
}
