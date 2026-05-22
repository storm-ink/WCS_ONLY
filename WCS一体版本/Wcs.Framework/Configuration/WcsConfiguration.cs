using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace Wcs.Framework.Cfg
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
                WcsConfiguration._logger.Trace1("开始读取配置……", this);
                sw.Start();
                RequestProcessesElement = new RequestProcessesElement(getOrGenerateNode(section, "requestProcesses"), this);

                SettingCollection = new SettingCollection(getOrGenerateNode(section, "settings"), this);

                ApplicationElement = new ApplicationElement(getOrGenerateNode(section, "application"), this);

                DataReceiverCollection = new DataReceiverCollection(getOrGenerateNode(section, "dataReceives"), this);

                sw.Stop();
                Console.WriteLine("read application configuration used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();
                DeviceCollection = new DeviceCollection(getOrGenerateNode(section, "devices"), this);
                sw.Stop();
                Console.WriteLine("read device configuration used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();

                LocationCollection = new LocationCollection(getOrGenerateNode(section, "locations"), this);

                sw.Stop();
                Console.WriteLine("read locations configuration used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();

                RouteCollection = new RouteCollection(getOrGenerateNode(section, "routes"), this);

                sw.Stop();
                Console.WriteLine("read routes configuration used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();

                generateNets();

                sw.Stop();
                Console.WriteLine("create nets used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();

                TaskEventHandlersElement = new TaskEventHandlersElement(getOrGenerateNode(section, "taskEventHandlers"), this);

                DeviceWarningEventHandlersElement = new DeviceWarningEventHandlersElement(getOrGenerateNode(section, "deviceWarningEventHandlers"), this);

                TaskChouseEndLocationHandlersElement = new TaskChouseEndLocationHandlersElement(getOrGenerateNode(section, "taskChouseEndLocationHandlers"), this);

                EquipmentFailureEventHandlersElement = new EquipmentFailureEventHandlersElement(getOrGenerateNode(section, "equipmentFailureEventHandlers"), this);

                RouteSelectorsElement = new RouteSelectorsElement(getOrGenerateNode(section, "RouteSelectors"), this);

                RouteStrategysElement = new RouteStrategysElement(getOrGenerateNode(section, "RouteStrategys"), this);

                WMSTaskCompeletedExternalHandlersElement = new WMSTaskCompeletedExternalHandlersElement(getOrGenerateNode(section, "wmsTaskCompeletedExternalHandlers"), this);

                TaskRequestHandlersElement = new TaskRequestHandlersElement(getOrGenerateNode(section, "taskRequestHandlers"), this);
                EquipmentActionSendPreHandlersElement = new EquipmentActionSendPreHandlersElement(getOrGenerateNode(section, "equipmentActionSendPreHandlers"), this);

                ServiceElement = new ServiceElement(getOrGenerateNode(section, "services"), this);


                MessageBoardElement = new MessageBoard.Cfg.MessageBoardElement(getOrGenerateNode(section, "messageBoard"), this);

                sw.Stop();
                Console.WriteLine("read others configuration used {0} milliseconds", sw.ElapsedMilliseconds);
                sw.Restart();

                WcsConfiguration._logger.Trace1("配置读取成功。", this);
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

        /// <summary>
        /// 获取当前应用程序所管理的区域（只有在进行分区设计时才应设置该值）。
        /// 在 appSettings["wcs-framework-area"]
        /// </summary>
        public static String Area
        {
            get
            {
                if (System.Configuration.ConfigurationManager.AppSettings["wcs-framework-area"] == null)
                {
                    return null;
                }

                if (String.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["wcs-framework-area"]))
                {
                    return null;
                }

                return System.Configuration.ConfigurationManager.AppSettings["wcs-framework-area"].Trim();
            }
        }

        public static WcsConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (WcsConfiguration)System.Configuration.ConfigurationManager.GetSection("wcs-configuration");
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
        /// <summary>
        /// 指示当前运行的主应用程序是否是模拟器
        /// </summary>
        public static Boolean IsSimulationApplication
        {
            get
            {
                if (System.Reflection.Assembly.LoadFile(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                .GetCustomAttributes(true)
                .Any(x => x.GetType() == typeof(System.Reflection.AssemblyProductAttribute)
                    && x.ToString().Contains("模拟")
                    ))
                {
                    return true;
                }

                if (!String.IsNullOrWhiteSpace(System.Diagnostics.Process.GetCurrentProcess().StartInfo.Arguments)
                    && System.Diagnostics
                    .Process.GetCurrentProcess()
                    .StartInfo.Arguments.Contains("模拟"))
                {
                    return true;
                }

                if (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Contains("模拟"))
                {
                    return true;
                }
                return false;
            }
        }

        public ApplicationElement ApplicationElement { get; private set; }

        public DataReceiverCollection DataReceiverCollection { get; private set; }

        public DeviceCollection DeviceCollection { get; private set; }

        public DeviceWarningEventHandlersElement DeviceWarningEventHandlersElement { get; private set; }

        public TaskChouseEndLocationHandlersElement TaskChouseEndLocationHandlersElement { get; private set; }

        public EquipmentFailureEventHandlersElement EquipmentFailureEventHandlersElement { get; private set; }

        public RouteStrategysElement RouteStrategysElement { get; set; }

        public RouteSelectorsElement RouteSelectorsElement { get; set; }

        public WMSTaskCompeletedExternalHandlersElement WMSTaskCompeletedExternalHandlersElement { get; set; }

        public TaskRequestHandlersElement TaskRequestHandlersElement { get; set; }

        public EquipmentActionSendPreHandlersElement EquipmentActionSendPreHandlersElement { get; set; }

        public LocationCollection LocationCollection { get; private set; }

        public MessageBoard.Cfg.MessageBoardElement MessageBoardElement { get; private set; }

        public Net[] Nets { get; private set; }

        public RequestProcessesElement RequestProcessesElement { get; private set; }
        public RouteCollection RouteCollection { get; private set; }

        public ServiceElement ServiceElement { get; private set; }

        public SettingCollection SettingCollection { get; private set; }
        public TaskEventHandlersElement TaskEventHandlersElement { get; private set; }
        public static WcsConfiguration StartApplication(IWcsApplication application)
        {
            _logger.Trace1("准备启动应用程序...", null);

            _logger.Trace1(string.Format("共找到 {0} 个自启动项", Instance.ApplicationElement.StartupSelection.StartupElements.Length), null);
            foreach (var startupElement in Instance.ApplicationElement.StartupSelection.StartupElements)
            {
                startupElement.ApplicationStartup.Run(application);
            }
            _logger.Trace1("自启动项加载完成", null);

            var taskableDevices = Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Select(x => x.Device)
                .Where(x => x is TaskableDevice)
                .Select(x => x as TaskableDevice);

            _logger.Trace1(string.Format("共找到 {0} 个可执行任务的设备，开始启动动作序列处理程序...", taskableDevices.Count()), null);
            foreach (var taskableDevice in taskableDevices)
            {
                taskableDevice.EquipmentActionScheduler.Start();
            }

            _logger.Trace1("动作序列处理程序启动结束", null);

            EventBus.EventBusEventPublisher.Initialization();

            return Instance;
        }

        /// <summary>
        /// 将系统可识别（可二次转换的）编码值转换为位置对象
        /// </summary>
        /// <param name="convertibleCode">系统可识别（可二次转换的）编码值</param>
        /// <returns>转换失败将返回 null。</returns>
        public static Location TryParseLocation(string convertibleCode)
        {
            var locationElement = WcsConfiguration
                .Instance
                .LocationCollection
                .ParticularLocationCollection
                .SelectMany(x => x.LocationElements)
                .SingleOrDefault(x => string.Equals(x.Location.ToConvertibleCode(), convertibleCode, StringComparison.CurrentCultureIgnoreCase));

            if (locationElement == null)
            {
                return null;
            }

            return locationElement.Location;
        }

        /// <summary>
        /// 将指定设备名称及位置设备编码值转换为位置对象
        /// </summary>
        /// <param name="deviceName">设备名称</param>
        /// <param name="deviceCode">位置设备编码</param>
        /// <returns>转换失败将返回 null。</returns>
        public static Location TryParseLocation(String deviceName, String deviceCode)
        {
            return TryParseLocation(String.Format("{0}@{1}", deviceCode, deviceName));
        }

        //创建连接网络
        void generateNets()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var _adjacencies = new AdjacencyList<Route>();
            var routes = RouteCollection.RouteElements.Select(x => x.Route)
                .ToList();
            //添加顶点
            foreach (var route in routes)
            {
                _adjacencies.AddVertex(route);
            }

            sw.Stop();
            Console.WriteLine("AddVertexs used {0} milliseconds", sw.ElapsedMilliseconds);
            sw.Restart();

            //添加有向边
            foreach (var route in routes)
            {
                foreach (var adjoin in route.Adjoins)
                {
                    if (!_adjacencies.DirectedEdgeIsContains(route, adjoin))
                    {
                        _adjacencies.AddDirectedEdge(route, adjoin);
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("AddDirectedEdges used {0} milliseconds", sw.ElapsedMilliseconds);
            sw.Restart();
            //_adjacencies.BFSTraverse();

            this.Nets = _adjacencies
                .Nets
                .Select(x => new Net(x.ToArray(), this))
                .ToArray();


            sw.Stop();
            Console.WriteLine("Convert nets to array used {0} milliseconds", sw.ElapsedMilliseconds);
        }

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
