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
using System.Net.Configuration;
using Wcs.Framework.Cfg;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace BOE
{
    public class 请求处理程序 : IApplicationStartup
    {
        static Thread _thread;
        bool _directRequest;
        Logger _logger;
        Location _start;
        Location _requestLoc;
        Int32 _interval;
        Location _end;
        string endloc;
        String _taskType;
        Wcs.Framework.Cfg.StartupElement _element;
        Boolean _autoAddContainerCode, _enable_IO_data, _addWeight;
        TaskSource _taskSource;
        string data;
        Dictionary<String, String> _data=new Dictionary<String, String>()
        {
            {"1022" ,"1024"},
            {"1042" ,"1044"},
            {"2022" ,"2024"},
            {"2042" ,"2044"},
        };
        public void Initialize(StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            _element = element;
            _interval = element.GetAttributeOrDefault<Int32>("interval", 10000);
            _directRequest = element.GetAttributeOrDefault<bool>("directRequest", false);
            //if (_directRequest)
            //    return;
            endloc = element.GetAttributeOrDefault<String>("end", " ");
            _taskType = element.GetAttributeOrDefault<String>("taskType", "WCSAUTO");
            data = element.GetAttributeOrDefault<String>("IO_Data", "0");
            _enable_IO_data = element.GetAttributeOrDefault<Boolean>("enableIO_Data", false);
            _taskSource = element.GetAttributeOrDefault<TaskSource>("taskSource", TaskSource.Unknow);

        }

        public void Run(IWcsApplication application)
        {
            var location = _element.GetAttribute<string>("requestLoc");
            _requestLoc = LocationConverter.ConvertibleCodeToLcation(location);
            //this.Init($"{_requestLoc.ToConvertibleCode()} 请求处理程序");

            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();

            _logger.Info1($"{this} 线程已经启动", this);
        }

        public void check(object obj)
        {
            while (true)
            {
                Thread.Sleep(_interval);
                try
                {
                    var conveyor = (ConveyorDevice)_requestLoc.Device;
                    if (!conveyor.IsConnected || conveyor.OccupiedSignals == null || conveyor.OccupiedSignals.Count() == 0 || conveyor.LocationCurrentTasks == null || conveyor.LocationCurrentTasks.Count() == 0)
                    {
                        //this.Log($"处理{_requestLoc.DeviceCode}位置请求时，由于设备未连接/占位信号为空/收到的占位信号上报数量为0/货位当前任务为空/收到的货位当前任务上报数量为0，结束本次循环，等待下次处理");
                        continue;
                    }

                    var holdSingle = conveyor.OccupiedSignals.SingleOrDefault(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.HandShake == HoldSignalNetTransferObjectHandShake.New);
                    if (holdSingle == null)
                    {
                        //this.Log($"处理{_requestLoc.DeviceCode}位置请求时，由于未读取到 {_requestLoc.DeviceCode} 的占位，结束本次循环，等待下次处理");
                        continue;
                    }



                    var locationTask = conveyor.LocationCurrentTasks.Any(x => x.PosNo == Convert.ToUInt16(_requestLoc.DeviceCode) && x.TaskNo < 100);
                    if (locationTask)
                        continue;
                    if (!data.Contains(holdSingle.IO_Data.ToString()))
                    {
                        continue;
                    }
                    DirectRequest(conveyor, _requestLoc, holdSingle);

                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);

                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                            new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Error,
                            String.Format($"占位{_requestLoc.DeviceCode}）"),
                            String.Format($"在处理占位{_requestLoc.DeviceCode}时发生异常，异常消息{ex}"),
                            null));
                }
            }
        }

        private void DirectRequest(ConveyorDevice conveyor, Location requestLoc, HoldSignalNetTransferObject holdSignal)
        {
            Boolean task;
            var requestId = holdSignal.Data_ID.ToString();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Query<Task>().Any(x => x.StartLocation.UserCode == requestLoc.UserCode && x.CurrentLocation.UserCode == requestLoc.UserCode && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
               
            }
            if (task)
                return;
            string msg;
            if (holdSignal.IO_Data == 3)
            {
                //请求补充母托盘垛
                bool hastasks;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    hastasks = unitOfWork.session.Query<Task>().Any(x => x.EndLocation.UserCode.Contains(requestLoc.UserCode));
                    unitOfWork.Commit();
                }
                if (!hastasks)
                {
                    var result = requestLoc.UserCode.Request(out msg, requestId);
                    _logger.Trace1($"位置 {requestLoc.UserCode} 向WMS请求结果：{result}，异常消息：{msg}", this);
                    Thread.Sleep(3000);
                }

            }
            else if (holdSignal.IO_Data == 2)
            {
                //托盘垛入库请求 叠盘机有满垛入库请求时判断托盘缓存位是否有占位及托盘入库任务 有占位没任务时 申请托盘缓存位托盘垛入库
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                     var exist = unitOfWork.session.Query<Task>().Any(x => x.StartLocation.DeviceCode == _data[requestLoc.DeviceCode] &&
                                                                            x.CurrentLocation.DeviceCode== _data[requestLoc.DeviceCode]
                                                                         && x.Status != TaskStatus.Cancelled && x.Status != TaskStatus.Completed);
                    if (exist)
                    {
                        return;
                    }
                }
                var signal = conveyor.ReadStatus<SimpleHoldSignalNetTransferObject>().FirstOrDefault(x => x.PosNo.ToString() == _data[requestLoc.DeviceCode]);
                if(signal != null && signal.HandShake == HoldSignalNetTransferObjectHandShake.New)
                {
                    var stackerNo = holdSignal.PosNo;
                    var stacker = conveyor.ReadStatus<StackerNetTransferObject>().FirstOrDefault(x => x.StackerNo == stackerNo);
                    int palletTotal = 0;
                    if (stacker != null)
                        palletTotal = stacker.Stacker_Actual_Number;
                    var result = requestLoc.UserCode.Request(out msg, palletTotal, requestId);
                    _logger.Trace1($"位置 {requestLoc.UserCode} 向WMS请求结果：{result}，异常消息：{msg}", this);
                }
               
            }
            //else if (holdSignal.IO_Data == 1)
            //{
            //    //托盘可用，给输送线PLC写值
            //    SendRequestResultCommand cmd = new SendRequestResultCommand();
            //    cmd.BarcodeEnable = 1;
            //    cmd.PosNo = holdSignal.PosNo;
            //    cmd.Data_ID = holdSignal.Data_ID;
            //    conveyor.Write(cmd, cmd.SendSuccess);

               

            //}
           
            else
            {
                return;
            }

        }


    }
}
