using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using NHibernate.Linq;
using Wcs.DefaultImplementCollection.Conveyor;
using ZHQXC.WebAPI;
using Wcs.Framework.Events;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using ZHQXC.WebAPI.Entity;
using Wcs.FrameworkExtend;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Net;
using System.Xml;
using Wcs.DefaultImplementCollection.Crane;

namespace ZHQXC
{
    /// <summary>
    /// 自复位占位请求处理程序：放行货物
    /// </summary>
    public class Circle_Warehouse : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger;
        Location _requestLoc;
        Int32 _interval;
        Wcs.Framework.Cfg.StartupElement _element;
        TaskSource _taskSource;
        String _taskType;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            _element = element;
            _interval = element.GetAttributeOrDefault<Int32>("interval", 3000);
            _taskType = element.GetAttributeOrDefault<String>("taskType", "Circler任务");
            _taskSource = element.GetAttributeOrDefault<TaskSource>("taskSource", TaskSource.Wcs);
        }

        public void Run(IWcsApplication application)
        {
            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();
            _logger.Info1($"{_requestLoc} 请求处理线程已经启动", this);
        }

        Dictionary<string, DateTime> lastRequestOK = new Dictionary<string, DateTime>();
        public void check(object obj)
        {
            Thread.Sleep(_interval);
            int flag = 1;
            string _end = string.Empty;
            string _start = string.Empty;
            string start_temp = string.Empty;
            string end_temp = string.Empty;

            List<string> userCodes_C001 = new List<string>();
            string xmlFilePath_C001 = "../../WCS程序/Debug_circle0102/系统配置/堆垛机/C001.xml"; // 替换为你的 XML 文件路径
            //string xmlFilePath_C001 = "../../系统配置/堆垛机/C001.xml"; // 替换为你的 XML 文件路径
            // 创建一个 XmlDocument 对象并加载 XML 文件
            XmlDocument xmlDoc_C001 = new XmlDocument();
            xmlDoc_C001.Load(xmlFilePath_C001);

            // 获取所有 <location> 节点
            XmlNodeList locationNodes_C001 = xmlDoc_C001.SelectNodes("//location");

            // 提取 userCode 的值并保存到列表中
            foreach (XmlNode node in locationNodes_C001)
            {
                string userCode = node.Attributes["userCode"]?.Value;
                if (!string.IsNullOrEmpty(userCode))
                {
                    userCodes_C001.Add(userCode);
                }
            }
            if (userCodes_C001.Count >= 3)
            {
                userCodes_C001.RemoveRange(0, 3);
            }

            
            List<string> userCodes_C002 = new List<string>();
            string xmlFilePath_C002 = "../../WCS程序/Debug_circle0102/系统配置/堆垛机/C002.xml"; // 替换为你的 XML 文件路径

            //string xmlFilePath_C002 = "../../系统配置/堆垛机/C002.xml"; // 替换为你的 XML 文件路径
            // 创建一个 XmlDocument 对象并加载 XML 文件
            XmlDocument xmlDoc_C002 = new XmlDocument();
            xmlDoc_C002.Load(xmlFilePath_C002);

            // 获取所有 <location> 节点
            XmlNodeList locationNodes_C002 = xmlDoc_C002.SelectNodes("//location");

            // 提取 userCode 的值并保存到列表中
            foreach (XmlNode node in locationNodes_C002)
            {
                string userCode = node.Attributes["userCode"]?.Value;
                if (!string.IsNullOrEmpty(userCode))
                {
                    userCodes_C002.Add(userCode);
                }
            }
            if (userCodes_C002.Count >= 3)
            {
                userCodes_C002.RemoveRange(0, 3);
            }

            userCodes_C001.AddRange(userCodes_C002);
            List<string> endpoint = userCodes_C001;
            List<string> startpoint = new List<string>{ "00-001-1028" , "00-001-1027", "00-001-1026", "00-001-1025" };
           


            while (true)
            {
                string msg = "";
                try
                {
                    Thread.Sleep(_interval);

                    //第一步：判断输送线是否连接
                    var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("立库CV");
                    if (!conveyor.IsConnected || conveyor.RequestBlocks == null)
                    {
                        msg = $"设备未连接";
                        this.Log(msg);
                        continue;
                    }
                    //第二步：判断堆垛机是否连接
                    var crane_C001 = DeviceConverter.ToDevice<CraneDevice>("c001");
                    if (!crane_C001.IsConnected)
                    {
                        msg = $"设备未连接";
                        this.Log(msg);
                        continue;
                    }
                    var crane_C002 = DeviceConverter.ToDevice<CraneDevice>("c002");
                    if (!crane_C002.IsConnected)
                    {
                        msg = $"设备未连接";
                        this.Log(msg);
                        continue;
                    }
                    //第三步：判断是否存在执行任务
                    List<PreTask> alreadyCreateTasks = null;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        alreadyCreateTasks = unitOfWork.session.Query<PreTask>().ToList();
                        unitOfWork.Commit();
                    }
                    if (alreadyCreateTasks.Count() != 0)
                    {
                        continue;
                    }
                    //第四步：判断是入库还是出库，flag=0是入库，flag=1是出库
                    if(flag == 1)
                    {
                        Random random = new Random();
                        int randomIndex_end = random.Next(endpoint.Count);
                        int randomIndex_start = random.Next(startpoint.Count);
                        _start = endpoint[randomIndex_end];
                        _end = startpoint[randomIndex_start];
                        flag = 0;
                        start_temp = _start;
                        end_temp = _end;
                    }
                    else if (flag == 0)
                    {
                        _start = end_temp;
                        _end = start_temp;
                        flag = 1;
                    }
                    Location startLocation = LocationConverter.UserCodeToLcation(_start);
                    Location endLocation = LocationConverter.UserCodeToLcation(_end);
                    //第五步：开始创建任务
                    String taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                    TaskSource _source = _taskSource;
                    String __taskType = _taskType;
                    PreTask preTask = new PreTask(taskCode, LocationConverter.ToLocationInfo(startLocation), LocationConverter.ToLocationInfo(endLocation))
                    {
                        Source = _source,
                        TaskType = __taskType
                    };
                    //第六步：下发任务
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        unitOfWork.session.Save(preTask);
                        unitOfWork.Commit();
                    }
                    Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(new Wcs.FrameworkExtend.Events.PreTaskAddedEvent(preTask));
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
    }
}
