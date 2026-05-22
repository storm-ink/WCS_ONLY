

using Newtonsoft.Json;
using NHibernate.Linq;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using BOE.Helps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.DefaultImpls.Crane;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


namespace BOE
{
    public class 大屏数据采集程序 : IApplicationStartup
    {
        static Thread _thread;

        Int32 _interval;

        Logger _logger;
        Wcs.Framework.Cfg.StartupElement _element;
        private List<Socket> socketList = new List<Socket>();
        Dictionary<string, Socket> _dic = new Dictionary<string, Socket>();
        string _ipAddress;
        int _port;

        public void Initialize(StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _element = element;

        }

        public void Run(IWcsApplication application)
        {
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 5000);
            _port = _element.GetAttribute<int>("Port");
            _ipAddress = "192.168.1.199";
            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
        }
        public void StartServer(object obj)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(200);
                    Socket socket = obj as Socket;

                    Socket socket2 = socket.Accept();
                    socketList.Add(socket2);
                    _dic.Add(socket2.RemoteEndPoint.ToString(), socket2);
                    Thread t = new Thread(SendMsg);
                    t.Start(socket2);
                }
                catch(Exception e)
                {

                }
               
            }
        }

        private List<dynamic> BuildCraneImage()
        {
            var result = new List<dynamic>();

            var CraneDevices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                                .Where(x => x.Device is Wcs.DefaultImpls.Crane.CraneDevice);

            foreach (var crane in CraneDevices)
            {
                CraneInfo craneInfo = new CraneInfo();
                var info = ((CraneDevice)crane.Device).LastStatus;                
                if (crane.Device.IsConnected && info != null)
                {
                    craneInfo.lastStatus = info;
                    craneInfo.Name = crane.Name;
                    //craneInfo.Holder = ((CraneDevice)crane.Device).Holder.ToString();
                    craneInfo.IsConnected = ((CraneDevice)crane.Device).IsConnected;
                    craneInfo.WorkMode = (int)((CraneDevice)crane.Device).WorkMode;
                    craneInfo.CurrentAtUserCode = crane.Device.Name;
                    craneInfo.StateName = info.State.GetDescription();
                    craneInfo.ForkPositionName = info.ForkVerticalPosition.GetDescription();

                    try
                    {
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            if (info.ErrorCode != 0)
                            {
                                var error = unitOfWork.session.Get<DeviceErrorType>( info.ErrorCode).ErrorName;
                                craneInfo.Warning=error;
                            }
                            
                            EquipmentAction currentAction = unitOfWork.session.Query<EquipmentAction>().Where(x => x.DeviceName==crane.Name).FirstOrDefault();
                            if (currentAction != null)
                            {
                                LogicMovement logicMovement = unitOfWork.session.Query<LogicMovement>().Where(x => x.Id == currentAction.Id).FirstOrDefault();
                                var tasks2 = unitOfWork
                                    .session
                                    .Get<Task>(logicMovement.Task.Id);
                                craneInfo.Start = tasks2.StartLocation.DeviceCode;
                                craneInfo.End = tasks2.EndLocation.UserCode;
                                craneInfo.TaskType = tasks2.TaskType;
                                craneInfo.TaskCode = tasks2.TaskCode;
                                if (tasks2.ContainerCodes.Count>0)
                                {
                                    craneInfo.ContainerCode = tasks2.ContainerCodes.FirstOrDefault().ToString();
                                }                                
                                unitOfWork.Commit();
                            }

                        }
                    }
                    catch
                    {

                    }
                    result.Add(craneInfo);
                }

            }

            return result;
        }

        private List<dynamic> BuildRGVImage()
        {
            var result = new List<dynamic>();

            var ConveyorDevices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                                .Where(x => x.Device is Wcs.DefaultImpls.Conveyor.ConveyorDevice);

            foreach (var conveyor in ConveyorDevices)
            {
                if (!conveyor.Device.IsConnected)
                {
                    continue;
                }
                Wcs.Framework.Task[] tasks;
              
                if (conveyor.Name.Contains("环穿"))
                {
                    var device = DeviceConverter.ToDevice<ConveyorDevice>(conveyor.Name);
                    var rgvInfo = device.ReadStatus<VehicleNetTransferObject>();
                    foreach (var item in rgvInfo.Cast<VehicleNetTransferObject>())
                    {
                        var alarm = device.ReadStatus<VehicleAlarmNetTransferObject>().Where(x => x.VehicleNo == item.VehicleNo);
                        int alarmInfo = 1;
                        if (alarm.Any())
                        {
                            alarmInfo = (int)Wcs.Framework.DeviceStatus.Error;
                        }
                        var rgvName = "R0" + item.VehicleNo.ToString().PadLeft(2, '0');
                        if (conveyor.Name == "4库1层环穿")
                        {
                            rgvName= "R0" + (item.VehicleNo+7).ToString().PadLeft(2, '0');
                        }
                        if (conveyor.Name == "变轨环穿")
                        {
                            rgvName = "R0" + (item.VehicleNo + 14).ToString().PadLeft(2, '0');
                        }
                        string start = "", end = "", tasktype = "", taskcode = "", containercode = "", fromStation="", toStation="";
                        try
                        {
                            var tasks3 = device.ReadStatus<VehicleNetTransferObject>().Where(x => x.TaskNo >= 100 && x.TaskNo != 999999 && x.VehicleNo== item.VehicleNo).FirstOrDefault();
                            var taskno = Convert.ToInt32(tasks3.TaskNo);
                            var vehicleno = tasks3.VehicleNo;
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                            {
                                EquipmentAction currentAction = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == taskno).FirstOrDefault();
                                if (currentAction != null)
                                {
                                    LogicMovement logicMovement = unitOfWork.session.Query<LogicMovement>().Where(x => x.Id == currentAction.Id).FirstOrDefault();
                                    var tasks2 = unitOfWork
                                        .session
                                        .Get<Task>(logicMovement.Task.Id);
                                    start = tasks2.StartLocation.DeviceCode;
                                    end = tasks2.EndLocation.UserCode;
                                    tasktype = tasks2.TaskType;
                                    taskcode = tasks2.TaskCode;
                                    fromStation = currentAction.Movement.StartLocation.UserCode.ToString();
                                    toStation = currentAction.Movement.EndLocation.UserCode.ToString();
                                    if (tasks2.ContainerCodes.Count > 0)
                                    {
                                        containercode = tasks2.ContainerCodes.FirstOrDefault().ToString();
                                    }

                                    unitOfWork.Commit();
                                }
                            }


                        }
                        catch
                        {

                        }
                        var info2 = new
                        {
                            Name = rgvName,
                            No = item.VehicleNo,
                            Status = item.VehicleState.GetDescription(),                            
                            AlarmInfo = alarmInfo,
                            Position=item.Position,
                            IsConnected=true,
                            StateName= (int)item.VehicleState,
                            CommandId=item.TaskNo,
                            TaskCode=taskcode,
                            From=start,
                            To=end,
                            TaskType=tasktype,
                            ContainerCode=containercode,
                            FromStation=fromStation,
                            ToStation=toStation,
                            //TaskInfo = tasks2 == null ? tasks2 : null
                        };
                        result.Add(info2);
                    }
                }                
                }
                return result;
        }

        private List<dynamic> BuildConveyorImage()
        {
            var result = new List<dynamic>();

            var ConveyorDevices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection.SelectMany(x => x.DeviceElements)
                                .Where(x => x.Device is Wcs.DefaultImpls.Conveyor.ConveyorDevice);

            foreach (var conveyor in ConveyorDevices)
            {
                if (!conveyor.Device.IsConnected)
                {
                    continue;
                }
                Wcs.Framework.Task[] tasks;
               
                if (conveyor.Name.Contains("环穿"))
                {
                   
                }
                else
                {
                    var device = DeviceConverter.ToDevice<ConveyorDevice>(conveyor.Name);
                    var locationItem = device.ReadStatus<LocationTaskNetTransferObject>();
                    foreach (var item in locationItem.Cast<LocationTaskNetTransferObject>())
                    {
                        //if (item.TaskNo == 0)
                        //{
                        //    continue;
                        //}
                        var status = device.ReadStatus<LocationNetTransferObject>().Where(x => x.PosNo == item.PosNo);
                        Wcs.Framework.Task tasks2 = null;
                        try
                        {
                            if (item.TaskNo > 99)
                            {
                                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                                {
                                    EquipmentAction currentAction = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == item.TaskNo).FirstOrDefault();
                                    if (currentAction != null)
                                    {
                                        LogicMovement logicMovement = unitOfWork.session.Query<LogicMovement>().Where(x => x.Id == currentAction.Id).FirstOrDefault();
                                        tasks2 = unitOfWork
                                            .session
                                            .Get<Wcs.Framework.Task>(logicMovement.Task.Id);
                                        unitOfWork.Commit();
                                    }

                                }
                            }
                        }
                        catch
                        {

                        }
                        var alarm = device.MachineAlarms.FirstOrDefault(x => x.PosNo == item.PosNo);
                        int alarmInfo = 1;
                        if (alarm.Manual ||
                                            alarm.Isolator ||
                                            alarm.Breaker ||
                                            alarm.Photocell ||
                                            alarm.RunOvertime ||
                                            alarm.OccupyOvertime ||
                                            alarm.TaskNoGoods ||
                                            alarm.X_MotorVAF ||
                                            alarm.Y_MotorVAF ||
                                            alarm.X_MotorContactor ||
                                            alarm.X_MotorBraker ||
                                            alarm.Y_MotorContactor ||
                                            alarm.Y_MotorBraker ||
                                            alarm.Lift_MotorContactor ||
                                            alarm.Lift_MotorBraker)
                        {
                            alarmInfo = (int)Wcs.Framework.DeviceStatus.Error;
                        }
                        if(tasks2 != null)
                        {
                            var info2 = new
                            {
                                Device = conveyor.Name,
                                No = item.PosNo,
                                Status = status.FirstOrDefault().Status,
                                TaskNo = item.TaskNo,
                                SensorInfo = item.TaskNo == 0 ? 0 : 1,
                                AlarmInfo = alarmInfo,
                                TaskCode=tasks2.TaskCode,
                                Type=tasks2.TaskType,
                                From=tasks2.StartLocation.UserCode,
                                To=tasks2.EndLocation.UserCode,
                                ContainerCodes=tasks2.ContainerCodes.FirstOrDefault()
                            };
                            result.Add(info2);
                        }
                        else
                        {
                            var info2 = new
                            {
                                Device = conveyor.Name,
                                No = item.PosNo,
                                Status = status.FirstOrDefault().Status,
                                TaskNo = item.TaskNo,
                                SensorInfo = item.TaskNo == 0 ? 0 : 1,
                                AlarmInfo = alarmInfo,
                                TaskInfo = tasks2 == null ? null : tasks2
                            };
                            result.Add(info2);
                        }
                        
                        
                    }
                }
               

            }

            return result;
        }

        private List<dynamic> BuildWarningImage()
        {
            //var warnings = AppStartup.App.Ioc.Resolve<IWarningRepository>()
            //    .Query(out var totalItemsCount, 100, 0, null, null, null, false, null, null);

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                var warnings = unitOfWork.session.Query<WarningRecord>().OrderBy(x=>x.Id).Take(100);
                return warnings.Cast<dynamic>().ToList();
            }

            
        }
        public void SendMsg(object obj)
        {
            while (true)
            {
                Thread.Sleep(1000);
                var dto1 = BuildCraneImage();
                var dto2 = BuildConveyorImage();
                var dto3 = BuildRGVImage();
                //var dto4 = BuildWarningImage();
                var dto = new
                {
                    Crane = dto1,
                    Conveyor = dto2,
                    RailGuidedVehicle=dto3
                    
                };
                var value = JsonConvert.SerializeObject(dto);
                var dataBytes = System.Text.Encoding.UTF8.GetBytes(value);
                var compressed = false;
                if (dataBytes.Length > 1024)
                {
                    dataBytes = CompressHelper.ZipCompress(dataBytes);

                    compressed = true;

                    var base64 = Convert.ToBase64String(dataBytes);

                    var base64Bytes = System.Text.Encoding.UTF8.GetBytes(base64);

                    dataBytes = base64Bytes;
                }

                var package = new List<byte>();
                //添加压缩标记
                package.Add(compressed ? (byte)0x2 : (byte)0x1);
                //添加命令类型
                package.Add((byte)'A');
                //添加数据
                package.AddRange(dataBytes);
                //以0结束（字符串读0为结束符）
                package.Add(0x0);
                var bytes = package.ToArray();

                try
                {
                    foreach (Socket s in socketList)
                    {
                        try
                        {
                            s.Send(bytes);
                        }
                        catch (Exception)
                        {
                            s.Close();
                        }
                    }
                }
                catch
                {

                }

            }


        }

        private void check(object obj)
        {
            
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint IEP = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(IEP);
                socket.Listen(10);
                Thread thread = new Thread(new ParameterizedThreadStart(StartServer));
                thread.IsBackground = true;
                thread.Start(socket);
            }
            catch
            {

            }
           

        }
    }
}
