using DevExpress.XtraBars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// Wcs插件对象
    /// </summary>
    public abstract class WcsPlugin
    {
        /// <summary>
        /// 插件信息
        /// </summary>
        private WcsPluginInfo m_PluginInfo;
        /// <summary>
        /// 上下文对象
        /// </summary>
        public WcsContext Context { get; set; }
        /// <summary>
        /// BarButtonItem
        /// </summary>
        public BarButtonItem[] barButtonItems { get; set; }
        /// <summary>
        /// 被加载的优先级
        /// </summary>
        public virtual int Priority
        {
            get
            {
                return 10;
            }
        }
        /// <summary>
        /// 获取插件Id，默认为实现类的全路径名
        /// </summary>
        public virtual String Id
        {
            get
            {
                return this.GetType().FullName;
            }
        }
        /// <summary>
        /// 获取插件的信息
        /// </summary>
        public WcsPluginInfo PluginInfo
        {
            get
            {
                if (m_PluginInfo == null)
                {
                    m_PluginInfo = this.GetType().GetCustomAttributes(typeof(WcsPluginInfo), false).Cast<WcsPluginInfo>().FirstOrDefault();
                    if (m_PluginInfo == null)
                    {
                        m_PluginInfo = new WcsPluginInfo(typeof(WcsPluginInfo), "<Null>", "<Null>", "<Null>", "<Null>", "<Null>", "<Null>", 0, 0, 0);
                    }
                }

                return m_PluginInfo;
            }
        }
        /// <summary>
        /// 初始化插件，通常在插件被加载时进行的第一个操作
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool Initialization(WcsContext context)
        {
            this.Context = context;
            return true;
        }
        /// <summary>
        /// 销毁插件
        /// </summary>
        public virtual void Dispose()
        {
            
        }

    }
}
