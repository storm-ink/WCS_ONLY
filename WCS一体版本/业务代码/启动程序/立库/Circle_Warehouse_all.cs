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
    /// Circle_Warehouse_all
    public class Circle_Warehouse_all : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger;
        Location _requestLoc;
        Int32 _interval;
        Wcs.Framework.Cfg.StartupElement _element;
        TaskSource _taskSource;
        String _taskType;
        static int task_order;


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
            _logger.Info1($"请求处理线程已经启动", this);
        }

        public void check(object obj)
        {
            string msg ;
            task_order = 0;
            while (true)
            {
                try
                {
                    Thread.Sleep(_interval);
                    ////第一步：判断输送线是否连接
                    //var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("立库CV");
                    //if (!conveyor.IsConnected || conveyor.RequestBlocks == null)
                    //{
                    //    msg = $"设备未连接";
                    //    this.Log(msg);
                    //    continue;
                    //}
                    ////第二步：判断堆垛机是否连接
                    //var crane_C001 = DeviceConverter.ToDevice<CraneDevice>("c001");
                    //if (!crane_C001.IsConnected)
                    //{
                    //    msg = $"c001设备未连接";
                    //    this.Log(msg);
                    //    continue;
                    //}
                    //var crane_C002 = DeviceConverter.ToDevice<CraneDevice>("c002");
                    //if (!crane_C002.IsConnected)
                    //{
                    //    msg = $"c002设备未连接";
                    //    this.Log(msg);
                    //    continue;
                    //}
                    //第三步：判断是否存在执行任务
                    List<PreTask> alreadyCreateTasks = null;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        alreadyCreateTasks = unitOfWork.session.Query<PreTask>().ToList();
                        unitOfWork.Commit();
                    }
                    if (alreadyCreateTasks.Count() != 0)
                    {
                        msg = $"存在任务为完成，请等待，或人工处理完";
                        this.Log(msg);
                        continue;
                    }
                    //第四步：创建任务

                    switch (task_order)
                    {
                        case 0:
                            createCircleTask("00-001-1001", "01-001-002", task_order);
                            break;
                        case 1:
                            createCircleTask("01-001-002", "00-001-1021", task_order);
                            break;
                        case 2:
                            createCircleTask("00-001-1021", "00-001-1024", task_order);
                            break;
                        case 3:
                            createCircleTask("00-001-1024", "00-001-1021", task_order);
                            break;
                        case 4:
                            createCircleTask("00-001-1021", "01-001-002", task_order);
                            break;
                        case 5:
                            createCircleTask("01-001-002", "00-001-1001", task_order);
                            break;
                        default:
                            task_order = 0;
                            throw new NotImplementedException("未实现对属性 {1} 的赋值操作");
                    }
                }


                catch (Exception ex)
                {
                    //this.Log($"{ex}");
                }
            }
        }


        private void createCircleTask(string _start, string _end,int to)
        {
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
            task_order = ++to;
        }
    }
}
