using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Wcs.Framework.Devices;
using Wcs.Framework.Cfg;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 系统配置信息
    /// </summary>
    public partial class Configuration
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static Boolean Initialized{get;private set;}
        /// <summary>
        /// 获取配置文件名称
        /// </summary>
        public static String ConfigurationFileName
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Configuration).Assembly.Location), "wcs.config.xml");
            }
        }
        public NetPackageDecoder[] NetPackageDecoders { get; private set; }
        public static Device[] Devices
        {
            get
            {
                return DevicesSelection.Devices;
            }
        }
        public static DeviceRoute[] Routes
        {
            get
            {
                return RouteSelection.Routes;
            }
        }
        public static TaskDispatcher TaskDispatcher { get; private set; }

        static object locker1 = new object();
        static AdjacencyList<DeviceRoute> _adjacencies;
        /// <summary>
        /// 获取连通对象
        /// </summary>
        public static AdjacencyList<DeviceRoute> Adjacencies
        {
            get
            {
                lock (locker1)
                {
                    if (_adjacencies == null)
                    {
                        _adjacencies = new AdjacencyList<DeviceRoute>();

                        //添加顶点
                        foreach (var route in Routes)
                        {
                            _adjacencies.AddVertex(route);
                        }

                        //添加有向边
                        foreach (var route in Routes)
                        {
                            foreach (var adjoin in route.Adjoins)
                            {
                                if (!_adjacencies.DirectedEdgeIsContains(route, adjoin))
                                {
                                    _adjacencies.AddDirectedEdge(route, adjoin);
                                }
                            }
                        }
                    }
                }

                return _adjacencies;
            }
        }

        static object locker2 = new object();
        static Net[] _nets;
        /// <summary>
        /// 获取所有连通路径
        /// </summary>
        public static Net[] Nets
        {
            get
            {
                lock (locker2)
                {
                    if (_nets == null)
                    {
                        List<Net> reuslt = new List<Net>();
                        foreach (var routes in Adjacencies.Nets)
                        {
                            Net net = new Net();
                            net.Routes = routes.ToArray();
                            reuslt.Add(net);
                        }

                        _nets = reuslt.ToArray();
                    }

                    return _nets;
                }
            }
        }
        /// <summary>
        /// 获取所有设备货位
        /// </summary>
        public static Location[] Locations
        {
            get
            {
                return DevicesSelection.Locations;
            }
        }
        /// <summary>
        /// 日志输出对象
        /// </summary>
        public static LogTarget[] LoggerTargets
        {
            get
            {
                return LogTargetsSelection.LogTargetElements.Select(x => x.LogTarget).ToArray();
            }
        }

        /// <summary>
        /// 初始化系统配置数据
        /// </summary>
        public static void Initialize()
        {
            if (Configuration.Initialized)
            {
                throw new Exception(string.Format("正在尝试重复初始化 {0}", typeof(Configuration)));
            }

            Configuration.Initialized = true;

            //初始化延迟 Wms 通知处理进程
            DelayedWmsNotifyProcessor.GetInstance().Initialize();

            //加载任务派发器
            TaskDispatcher = new Framework.TaskDispatcher();

            //加载任务管理器
            TaskManager.GetInstance();

            //加载设备管理器，并连接到所有设备
            DeviceManager.GetInstance().ConnectAllDevice();
        }

        /// <summary>
        /// 获取一个配置节点
        /// </summary>
        /// <param name="path">一个不包含/configuration/的路径</param>
        /// <returns></returns>
        public static XmlNode GetSelection(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigurationFileName);
            var node = doc.SelectSingleNode("/configuration/" + path);
            doc = null;

            return node;
        }
        /// <summary>
        /// 通过 <see cref="T:Wcs.Framework.Cfg.Configuration.GetSetting"/> 方法获取配置数据缓存，用于减少 IO 读取.<br />
        /// 缓存内数据值在 <see cref="T:Wcs.Framework.Cfg.Configuration.SetSetting"/> 时将被更新.
        /// </summary>
        static Dictionary<string, dynamic> settingsCahce = new Dictionary<string, dynamic>();
        /// <summary>
        /// 从 /configuration/settings 节获取指定名称节点的 InnerText 值
        /// </summary>
        /// <param name="settingName">  节点名. </param>
        /// <param name="defaultValue"> 在节点为 null 时，指定返回的默认值. </param>
        public static T GetSetting<T>(string settingName, T defaultValue)
        {
            lock (settingsCahce)
            {
                if (settingsCahce.ContainsKey(settingName))
                {
                    return settingsCahce[settingName];
                }
                T result;
                var node = GetSelection("settings/" + settingName);
                if (node == null)
                {
                    result = defaultValue;
                }
                else
                {
                    result = Utils.ConvertTo<T>(node.InnerText);
                }

                settingsCahce[settingName] = result;

                return result;
            }
        }
        /// <summary>
        /// 设置 /configuration/settings 指定名称节点的 InnerText 值
        /// </summary>
        /// <param name="settingName">  节点名. </param>
        /// <param name="value">        值(String 表现形式). </param>
        public static void SetSetting<T>(string settingName, T value)
        {
            lock (settingsCahce)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ConfigurationFileName);
                var settingsSelection = doc.SelectSingleNode("/configuration/settings");
                if (settingsSelection == null)
                {
                    settingsSelection = doc.CreateElement("settings");
                    doc.SelectSingleNode("/configuration").AppendChild(settingsSelection);
                }

                var settingNode = settingsSelection.SelectSingleNode(settingName);
                if (settingNode == null)
                {
                    string path = "/configuration/settings";
                    foreach (var item in settingName.Split('/').Where(x=>!string.IsNullOrWhiteSpace(x)))
                    {
                        settingsSelection = settingsSelection.SelectSingleNode(path);
                        path = path + "/" + item;
                        settingNode = doc.SelectSingleNode(path);
                        if (settingNode == null)
                        {
                            settingNode = doc.CreateElement(item);
                            settingsSelection.AppendChild(settingNode);
                        }
                    }
                    settingsSelection.AppendChild(settingNode);
                }

                settingNode.InnerText = Convert.ToString(value) ?? "";

                doc.Save(ConfigurationFileName);
                doc = null;

                settingsCahce[settingName] = value;
            }
        }
    }

    public partial class Configuration
    {
        static Configuration()
        {
            //堆垛机配置
            CraneControl.Config.Init();

            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigurationFileName);

            //日志输出目标
            LogTargetsSelection = new LogTargetsSelection(doc.SelectSingleNode("configuration/logTargets"));
            //应用程序配置
            ApplicationElement = new ApplicationElement(doc.SelectSingleNode("configuration/application"));

            //请求处理程序
            if (doc.SelectSingleNode("configuration/requestSignalHandlers") != null)
            {
                RequestSignalHandlersSelection = new RequestSignalHandlersSelection(doc.SelectSingleNode("configuration/requestSignalHandlers"));
            }
            //解码器
            NetPackageDecodersSelection = new NetPackageDecodersSelection(doc.SelectSingleNode("configuration/netPackageDecoders"));
            //序列组
            SequenceGroupSelection = new Cfg.SequenceGroupSelection(doc.SelectSingleNode("configuration/sequenceGroups"));
            //设备
            DevicesSelection = new DevicesSelection(doc.SelectSingleNode("configuration/devices"));
            
            DevicesSelection.ResolveLocationSameAs();

            //路径
            RouteSelection = new RouteSelection(doc.SelectSingleNode("configuration/routes"));
            RouteSelection.ResolveAdjoins();

            //创建并启动动作序列
            DevicesSelection.CreateSequences();
        }
        /// <summary>
        /// 应用程序配置节点
        /// </summary>
        public static ApplicationElement ApplicationElement { get; private set; }
        /// <summary>
        /// 占位请求处理程序(有可能为 null)
        /// </summary>
        public static RequestSignalHandlersSelection RequestSignalHandlersSelection { get; private set; }
        /// <summary>
        /// 网络编、解码器节点
        /// </summary>
        public static NetPackageDecodersSelection NetPackageDecodersSelection{get;private set;}
        /// <summary>
        /// 日志输出目标节点
        /// </summary>
        public static LogTargetsSelection LogTargetsSelection { get; private set; }
        /// <summary>
        /// 设备节点
        /// </summary>
        public static DevicesSelection DevicesSelection { get; private set; }
        /// <summary>
        /// 路径节点
        /// </summary>
        public static RouteSelection RouteSelection { get; private set; }
        /// <summary>
        /// 获取日志输出对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static LogTarget GetLogTarget(string name)
        {
            return LogTargetsSelection
                .LogTargetElements
                .Select(x=>x.LogTarget)
                .SingleOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 获取日志输出对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static NetPackageDecoder GetNetPackageDecoder(string name)
        {
            return NetPackageDecodersSelection
                .NetPackageDecoderElements
                .Select(x => x.NetPackageDecoder)
                .SingleOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 根据名称获取设备对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Device GetDevice(string name)
        {
            return DevicesSelection
               .Devices
               .SingleOrDefault(x => x.DeviceName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 根据id获取路径
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public static DeviceRoute GetRoute(int routeId)
        {
            return RouteSelection
                .RouteElements
                .Single(x => x.Route.Id == routeId)
                .Route;
        }
        /// <summary>
        /// 序列组节点
        /// </summary>
        public static SequenceGroupSelection SequenceGroupSelection { get; private set; }
    }
}
