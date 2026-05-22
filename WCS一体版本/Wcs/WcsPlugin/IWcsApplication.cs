using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using NLog;

namespace Wcs
{
    /// <summary>
    /// Wcs主应用程序接口
    /// </summary>
    public interface IWcsApplication
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        Logger Logger { get; }
        RibbonStatusBar Status { get; }
        ToolStrip Tool { get; }
        /// <summary>
        /// 主窗体对象
        /// </summary>
        IWcsApplication MainForm { get;  }
        Control ButtomDock { get; }

        void OpenNewXtraForm<T>(string xtraTabPageName) where T : XtraForm, new();
        void OpenNewForm<T>(string xtraTabPageName) where T : Form, new();
        void ControlAdd(Control control);
    }
}
