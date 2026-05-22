using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using NHibernate.Linq;
using Wcs.DefaultImpls.Conveyor;
using BOE.WebAPI;
using Wcs.Framework.Events;
using Sineva.WMS.Dto.WCSDto.ReplyDto;

namespace BOE
{
    /// <summary>
    /// 自复位占位请求处理程序
    /// </summary>
    public class AutoClearHoldSingleRequestTaskStartUp_EmptyPallets_1011 : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger;
        Location _requestLoc;
        Int32 _interval;
        Wcs.Framework.Cfg.StartupElement _element;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            _element = element;
            _interval = element.GetAttributeOrDefault<Int32>("interval", 3000);
        }

        public void Run(IWcsApplication application)
        {
            var location = _element.GetAttribute<string>("requestLoc");
            this.Init($"{location}请求处理线程");
            _requestLoc = LocationConverter.ConvertibleCodeToLcation(location);

            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();

            _logger.Info1($"{_requestLoc} 请求处理线程已经启动", this);
        }

        Dictionary<string,DateTime> lastRequestOK = new Dictionary<string,DateTime>();
        public void check(object obj)
        {
            while (true)
            {
                string massage = "";//zhj-log
                Thread.Sleep(_interval);
                try
                {
                    var conveyor = (ConveyorDevice)_requestLoc.Device;
                    if (!conveyor.IsConnected || conveyor.OccupiedSignals == null || conveyor.OccupiedSignals.Count() == 0 || conveyor.LocationCurrentTasks == null || conveyor.LocationCurrentTasks.Count() == 0)
                    {
                        massage = $"设备未连接 或者 设备上报的占位信号为空或数量为0 或者 设备上报的货位任务为空或数量为0，本次{_requestLoc.DeviceCode}占位未处理，开始下次循环处理";
                        this.Log(massage);
                        continue;
                    }

                    //io_data=2 母托盘补充
                    var holdSingle = conveyor.OccupiedSignals.SingleOrDefault(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.HandShake == HoldSignalNetTransferObjectHandShake.New);
                    if (holdSingle == null)
                    {
                        massage = $"未读取设备 {conveyor.Name} 位置{_requestLoc.DeviceCode} 占位信号，开始下次循环处理";
                        this.Log(massage);
                        continue;
                    }

                    var locationTask = conveyor.LocationCurrentTasks.Any(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.TaskNo == 0);
                    if (!locationTask)
                    {
                        massage = $"设备 {conveyor.Name} 位置{_requestLoc.DeviceCode}的货位任务不为0，本次占位{holdSingle}未处理，开始下次循环处理";
                        this.Log(massage);
                        continue;
                    }

                    //if (lastRequestOK != null && DateTime.Now.Subtract((DateTime)lastRequestOK).TotalMilliseconds < 15 * 1000)
                    //{
                    //    //Console.WriteLine($"位置 {holdSingle.PosNo} 已成功请求一次补充空托盘，等待{15 * 1000 - DateTime.Now.Subtract((DateTime)lastRequestOK).TotalMilliseconds}ms后重新请求");
                    //    this.Log($"位置 {holdSingle.PosNo} 已成功请求一次，等待{15 * 1000 - DateTime.Now.Subtract((DateTime)lastRequestOK).TotalMilliseconds}ms后重新请求");
                    //    continue;
                    //}

                    if (holdSingle.IO_Data == 2)
                    {
                        var key = $"{holdSingle.PosNo}-{holdSingle.IO_Data}";
                        if (lastRequestOK != null && lastRequestOK.ContainsKey(key) && DateTime.Now.Subtract(lastRequestOK[key]).TotalMilliseconds < 15 * 1000)
                        {
                            //Console.WriteLine($"位置 {holdSingle.PosNo} 已成功请求一次补充空托盘，等待{15 * 1000 - DateTime.Now.Subtract((DateTime)lastRequestOK).TotalMilliseconds}ms后重新请求");
                            this.Log($"位置 {holdSingle.PosNo} 已成功请求一次母托盘垛补充，等待{15 * 1000 - DateTime.Now.Subtract(lastRequestOK[key]).TotalMilliseconds}ms后重新请求");
                            continue;
                        }
                        string requestId = $"{holdSingle.PosNo}-{holdSingle.Data_ID.ToString()}";

                        bool task;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            task = unitOfWork.session.Query<Task>().Any(x => x.AdditionalInfo.ContainsKey("REQUEST") && x.AdditionalInfo["REQUEST"] == requestId && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
                            if (!task)
                                task = unitOfWork.session.Query<Task>().Any(x => x.EndLocation.DeviceCode == _requestLoc.DeviceCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
                            unitOfWork.Commit();
                        }
                        if (task)
                        {
                            massage = $"已存在附加属性包含REQUEST={requestId}或者终点是{_requestLoc.DeviceCode}的任务，本次占位{holdSingle}未处理，开始下次循环处理";
                            this.Log(massage);
                            continue;
                        }

                        string requestType = "Tray";
                        var result = _requestLoc.UserCode.Request(out string msg, requestId, requestType);
                        _logger.Info1($"位置 {_requestLoc.UserCode} 向WMS请求结果：{result}，异常消息：{msg}", this);
                        this.Log(msg);
                        if (result)
                        {
                            if (lastRequestOK.ContainsKey(key))
                                lastRequestOK[key] = DateTime.Now;
                            else
                                lastRequestOK.Add(key, DateTime.Now);
                        }
                    }
                    else if (holdSingle.IO_Data == 1)
                    {
                        var key = $"{holdSingle.PosNo}-{holdSingle.IO_Data}";
                        if (lastRequestOK != null && lastRequestOK.ContainsKey(key) && DateTime.Now.Subtract(lastRequestOK[key]).TotalMilliseconds < 15 * 1000)
                        {
                            //Console.WriteLine($"位置 {holdSingle.PosNo} 已成功请求一次补充空托盘，等待{15 * 1000 - DateTime.Now.Subtract((DateTime)lastRequestOK).TotalMilliseconds}ms后重新请求");
                            this.Log($"位置 {holdSingle.PosNo} 已成功请求一次母托盘垛回收，等待{15 * 1000 - DateTime.Now.Subtract(lastRequestOK[key]).TotalMilliseconds}ms后重新请求");
                            continue;
                        }
                        string requestId = $"{holdSingle.PosNo}-{holdSingle.Data_ID.ToString()}";

                        bool task;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            task = unitOfWork.session.Query<Task>().Any(x => x.AdditionalInfo.ContainsKey("REQUEST") && x.AdditionalInfo["REQUEST"] == requestId && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
                            if (!task)
                                task = unitOfWork.session.Query<Task>().Any(x => x.StartLocation.DeviceCode == _requestLoc.DeviceCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
                            unitOfWork.Commit();
                        }
                        if (task)
                        {
                            massage = $"已存在附加属性包含REQUEST={requestId}或者起点是{_requestLoc.DeviceCode}的任务，本次占位{holdSingle}未处理，开始下次循环处理";
                            this.Log(massage);
                            continue;
                        }

                        string requestType = "Tray";
                        var result = _requestLoc.UserCode.Request(out string msg, 8, requestId, requestType);
                        _logger.Info1($"位置 {_requestLoc.UserCode} 向WMS请求结果：{result}，异常消息：{msg}", this);
                        this.Log(msg);
                        if (result)
                        {
                            if (lastRequestOK.ContainsKey(key))
                                lastRequestOK[key] = DateTime.Now;
                            else
                                lastRequestOK.Add(key, DateTime.Now);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);

                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Error,
                            String.Format($"占位{_requestLoc.DeviceCode}"),
                            String.Format($"在处理占位{_requestLoc.DeviceCode}时发生异常，异常消息{ex}"),
                            null));
                }
            }
        }
    }
}
