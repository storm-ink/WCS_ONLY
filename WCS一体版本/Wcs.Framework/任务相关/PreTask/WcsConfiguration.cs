using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace Wcs.FrameworkExtend.Cfg
{
    public class WcsConfiguration
    {
        public static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static WcsConfiguration _instance;

        public WcsConfiguration(XmlNode section)
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                WcsConfiguration._logger.Trace1("开始读取扩展配置……", this);
                sw.Start();

                PreTaskSchedulerHandlerElement = new PreTaskSchedulerHandlerElement(getOrGenerateNode(section, "PreTaskSchedulerFilter"), this);

                sw.Stop();
                Console.WriteLine("read FrameworkExtend.configuration used {0} milliseconds", sw.ElapsedMilliseconds);

                WcsConfiguration._logger.Trace1("扩展配置读取成功。", this);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                throw;
            }
        }

        /// <summary>
        /// 在配置文件加载结束后发生
        /// </summary>
        public static event EventHandler Loaded;

        public static WcsConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (WcsConfiguration)System.Configuration.ConfigurationManager.GetSection("wcs-configuration-extend");
                    IsLoaded = true;

                    if (Loaded != null)
                    {
                        Loaded.Invoke(_instance, EventArgs.Empty);
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 指示配置文件是否已加载结束
        /// </summary>
        public static Boolean IsLoaded { get; private set; }

        public PreTaskSchedulerHandlerElement PreTaskSchedulerHandlerElement { get; set; }
        XmlNode getOrGenerateNode(XmlNode parent, String nodeName)
        {
            XmlNode node = parent.SelectSingleNode(nodeName);
            if (node == null)
            {
                node = parent.OwnerDocument.CreateElement(nodeName);
                parent.AppendChild(node);

                _logger.Warn1(string.Format("未找到 {0}/{1} 节点，系统自动创建了一个空的节点取代", parent.GetXPath(), nodeName), this);
            }

            return node;
        }
    }
}
