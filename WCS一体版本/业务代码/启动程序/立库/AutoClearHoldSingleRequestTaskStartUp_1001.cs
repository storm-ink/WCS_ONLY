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
using Wcs.DefaultImplementCollection.Scanner;



namespace ZHQXC
{
    /// <summary>
    /// 自复位占位请求处理程序：放行货物
    /// </summary>
    public class AutoClearHoldSingleRequestTaskStartUp_1001 : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger;
        Location _requestLoc;
        Int32 _interval;
        Wcs.Framework.Cfg.StartupElement _element;
        Location _start;
        Location _end;
        TaskSource _taskSource;
        String _taskType;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            _element = element;
            _interval = element.GetAttributeOrDefault<Int32>("interval", 3000);
            _taskType = element.GetAttributeOrDefault<String>("taskType", "入库口前置任务");
            _taskSource = element.GetAttributeOrDefault<TaskSource>("taskSource", TaskSource.Wcs);
        }

        public void Run(IWcsApplication application)
        {
            var location = _element.GetAttribute<string>("requestLoc");
            _requestLoc = LocationConverter.ConvertibleCodeToLcation(location);
            this.Init($"{location}请求处理线程");
            _start = LocationConverter.ConvertibleCodeToLcation(_element.GetAttribute<String>("start"));
            _end = LocationConverter.ConvertibleCodeToLcation(_element.GetAttribute<String>("end"));
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
            while (true)
            {
                string msg = "";
                int requestID = 0;
                try
                {
                  Thread.Sleep(_interval);

                    //第一步：判断是否连接硬件
                    var conveyor = (ConveyorDevice)_requestLoc.Device;
                    if (!conveyor.IsConnected || conveyor.RequestBlocks == null || conveyor.RequestBlocks.Count() == 0 || conveyor.LocationInfoBlocks == null || conveyor.LocationInfoBlocks.Count() == 0)
                    {
                        msg = $"设备未连接 或者 设备上报的占位信号为空或数量为0 或者 设备上报的货位任务为空或数量为0，本次{_requestLoc.DeviceCode}占位未处理，开始下次循环处理";
                        this.Log(msg);
                        continue;
                    }
                    //第二步：判断是否有业务占位
                    var holdSingle = conveyor.RequestBlocks.SingleOrDefault(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.HandShake == RequestHandShakes.New && (x.IOData == 0));
                    if (holdSingle == null)
                    {
                        msg = $"设备 {conveyor.Name} 位置{_requestLoc.DeviceCode} 并且IO_DATA !=【1-满料入库】或者【3-空箱返库】】的占位信号，开始下次循环处理";
                        this.Log(msg);
                        continue;
                    }
                    //第三步：判断1001位置上是否
                    var locationTask = conveyor.LocationInfoBlocks.Any(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.TaskNo == 0);
                    if (!locationTask)
                    {
                        msg = $"设备 {conveyor.Name} 位置{_requestLoc.DeviceCode}的货位任务不为0，本次占位{holdSingle}未处理，开始下次循环处理";
                        this.Log(msg);
                        continue;
                    }
                    //重复请求ID
                    if (requestID == holdSingle.RequestID)
                        continue;
                    //第五步：开始创建任务，从1001到1003的任务
                    String taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                    TaskSource _source = _taskSource;
                    String __taskType = _taskType;
                    PreTask preTask = new PreTask(taskCode, LocationConverter.ToLocationInfo(_start), LocationConverter.ToLocationInfo(_end))
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
                    requestID = holdSingle.RequestID;
                    msg = $"为占位{holdSingle}成功创建任务 {preTask}(起点 {preTask.StartLocation.DeviceCode} 终点 {preTask.EndLocation.DeviceCode} 任务类型 {preTask.TaskType})";
                    this.Log(msg);
                    _logger.Info($"为占位 {holdSingle} 成功创建一条任务{preTask}(从 {preTask.StartLocation.DeviceCode} 到 {preTask.EndLocation.DeviceCode} 的 {preTask.TaskType} 任务)", this);
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                    msg = $"在处理出入口PLC自动触发任务占位任务时{_requestLoc.DeviceCode}时发生异常，异常消息{ex}";
                    this.Log(msg);
                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Error,
                            String.Format($"占位{_requestLoc.DeviceCode}）"),
                            String.Format($"在处理占位{_requestLoc.DeviceCode}时发生异常，异常消息{ex}"),
                            null));
                }
            }
        }
    }
}
