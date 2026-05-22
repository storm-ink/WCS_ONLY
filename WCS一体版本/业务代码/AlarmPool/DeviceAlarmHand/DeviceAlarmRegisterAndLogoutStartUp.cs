using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.Framework;
using NLog;
using System.Threading;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 设备数据接收停复机事件启动程序
    /// </summary>
    public class DeviceAlarmRegisterAndLogoutStartUp : IApplicationStartup
    {
        AlarmHand _alarmHand;
        string _deviceType;
        String _deviceName;
        Logger _logger = LogManager.CreateNullLogger();
        Thread _thread;
        TcpProtocolTaskableDevice device;

        Dictionary<String, DeviceCommand> _lastDeviceCommands;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="element"></param>
        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _deviceName = element.GetAttributeOrDefault<String>("device");
            _deviceType = element.GetAttributeOrDefault<String>("deviceType");
        }

        /// <summary>
        /// 自动加载
        /// </summary>
        /// <param name="application"></param>
        public void Run(IWcsApplication application)
        {
            _lastDeviceCommands = new Dictionary<string, DeviceCommand>();
            if (String.IsNullOrWhiteSpace(_deviceName))
            {
                return;
            }

            device = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection
                          .SelectMany(x => x.DeviceElements)
                          .Where(x => x.Device is TcpProtocolTaskableDevice)
                          .Select(x => x.Device as TcpProtocolTaskableDevice)
                          .SingleOrDefault(x => x.Name == _deviceName);

            if (device == null)
                return;

            _alarmHand = new AlarmHand(_deviceName);

            ParameterizedThreadStart Start = new ParameterizedThreadStart(proc);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);

            device.DataReceiver.DataReceived += new EventHandler<DataReceiverReceivedEventArgs>((sender, args) =>
            {
                OnDataReceivedWarningHandler(args.NetPacket, args.NetTransferObject);
            });
        }

        private void OnDataReceivedWarningHandler(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            try
            {
                pushList(netTransferObject);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        static object objLock = new object();
        List<Object> list = new List<object>();
        void pushList(Object obj)
        {
            lock (objLock)
            {
                list.Add(obj);
            }
        }
        /// <summary>
        /// 线程处理告警上报
        /// </summary>
        /// <param name="args"></param>
        public void proc(object args)
        {
            while (true)
            {
                Thread.Sleep(1000);
                //CL:todo
                //try
                //{
                //    if (list == null || list.Count() == 0)
                //        continue;

                //    object obj;
                //    lock (objLock)
                //    {
                //        //一般来说处理最新的报文即可
                //        obj = list.LastOrDefault();
                //        list.Clear();
                //    }
                //    if (obj == null)
                //        continue;

                //    if (device is CraneDevice)
                //        CraneWarningHand(obj);

                //    if (device is ConveyorDevice)
                //        ConveyorWarningHand(obj);


                //}
                //catch (Exception ex)
                //{
                //    _logger.Error1(ex, this);
                //}
            }
        }


        /// <summary>
        /// 输送线告警
        /// </summary>
        /// <param name="obj"></param>
        /// //CL:todo
        //private void ConveyorWarningHand(object obj)
        //{
        //    var _device = (ConveyorDevice)device;
        //    if (obj is Wcs.DefaultImplementCollection.Conveyor._DB2)
        //    {
        //        var state = (Wcs.DefaultImplementCollection.Conveyor._DB2)obj;
        //        var conveyorAlarms = state.Get<LocationInfoBlock>();
        //        if (conveyorAlarms != null && conveyorAlarms.Length > 0)
        //        {
        //            List<AlarmRecord> list = new List<AlarmRecord>();
        //            foreach (var item in conveyorAlarms)
        //            {
        //                if (item.PosNo == 0)
        //                    continue;

        //                var _alarms = item.ToAlarms();
        //                if (_alarms == null || _alarms.Length == 0)
        //                    continue;

        //                List<AlarmRecord> _list = new List<AlarmRecord>();
        //                foreach (var _item in _alarms)
        //                {
        //                    var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorName(_deviceType, _item);
        //                    if (alarm == null)
        //                    {
        //                        alarm = new DeviceErrorType();
        //                        if (DeviceErrorHelper.DeviceErrorTypes.Length == 0)
        //                            alarm.Id = 1;
        //                        else
        //                            alarm.Id = DeviceErrorHelper.DeviceErrorTypes.Max(x => x.Id) + 1;
        //                        alarm.AlarmCategory = 1;
        //                        alarm.DeviceType = _deviceType;
        //                        alarm.DeviceErrorCode = "";
        //                        alarm.ErrorName = "未登记故障";
        //                        alarm.Description = "";
        //                        alarm.Levle = 0;
        //                        alarm.Solution = "";
        //                        alarm.IsFault = true;
        //                        alarm = DeviceErrorHelper.AddDeviceErrorType(alarm);
        //                    }

        //                    AlarmRecord alarmRecord = new AlarmRecord();
        //                    alarmRecord.Id = AlarmRecordHelper.GetWaringRecordId();
        //                    alarmRecord.WcsAlarmCode = alarm.Id.ToString();
        //                    alarmRecord.AlarmCategory = AlarmCategorys.DeviceError;
        //                    alarmRecord.AlarmLevel = (AlarmLevels)alarm.Levle;
        //                    alarmRecord.AlarmName = alarm.ErrorName;
        //                    alarmRecord.DeviceErrorCode = alarm.DeviceErrorCode.ToString();
        //                    alarmRecord.Device = item.PosNo.ToString();
        //                    alarmRecord.OwnerDevice = _deviceName;
        //                    alarmRecord.DeviceType = _deviceType;
        //                    alarmRecord.BeginingAt = DateTime.Now;
        //                    alarmRecord.EndingAt = null;
        //                    alarmRecord.TotalMilliseconds = 0;

        //                    _list.Add(alarmRecord);
        //                }
        //                list.AddRange(_list);
        //            }
        //            _alarmHand.Handler(list);
        //        }

        //        var generalAlarms = state.Get<GeneralAlarmNetTransferObject>();
        //        if (generalAlarms != null && generalAlarms.Length > 0)
        //        {
        //            List<AlarmRecord> list = new List<AlarmRecord>();
        //            var _generalAlarms = generalAlarms.Where(x => x.PosNo != 0);
        //            foreach (var item in _generalAlarms)
        //            {
        //                if (item.PosNo == 0)
        //                    continue;

        //                var _alarms = item.GetAlarm();
        //                if (_alarms == null || _alarms.Length == 0)
        //                    continue;

        //                List<AlarmRecord> _list = new List<AlarmRecord>();
        //                foreach (var _item in _alarms)
        //                {
        //                    var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorName(_deviceType, _item);
        //                    if (alarm == null)
        //                    {
        //                        alarm = new DeviceErrorType();
        //                        if (DeviceErrorHelper.DeviceErrorTypes.Length == 0)
        //                            alarm.Id = 1;
        //                        else
        //                            alarm.Id = DeviceErrorHelper.DeviceErrorTypes.Max(x => x.Id) + 1;
        //                        alarm.AlarmCategory = 1;
        //                        alarm.DeviceType = _deviceType;
        //                        alarm.DeviceErrorCode = "";
        //                        alarm.ErrorName = _item;
        //                        alarm.Description = "";
        //                        alarm.Levle = 0;
        //                        alarm.Solution = "";
        //                        alarm.IsFault = true;
        //                        alarm = DeviceErrorHelper.AddDeviceErrorType(alarm);
        //                    }

        //                    AlarmRecord alarmRecord = new AlarmRecord();
        //                    alarmRecord.Id = AlarmRecordHelper.GetWaringRecordId();
        //                    alarmRecord.WcsAlarmCode = alarm.Id.ToString();
        //                    alarmRecord.AlarmCategory = AlarmCategorys.DeviceError;
        //                    alarmRecord.AlarmLevel = (AlarmLevels)alarm.Levle;
        //                    alarmRecord.AlarmName = alarm.ErrorName;
        //                    alarmRecord.DeviceErrorCode = alarm.DeviceErrorCode.ToString();
        //                    alarmRecord.Device = item.PosNo.ToString();
        //                    alarmRecord.OwnerDevice = _deviceName;
        //                    alarmRecord.DeviceType = _deviceType;
        //                    alarmRecord.BeginingAt = DateTime.Now;
        //                    alarmRecord.EndingAt = null;
        //                    alarmRecord.TotalMilliseconds = 0;

        //                    _list.Add(alarmRecord);
        //                }
        //                list.AddRange(_list);
        //            }
        //            _alarmHand.Handler(list);
        //        }
        //    }
        //}

        /// <summary>
        /// 堆垛机告警
        /// </summary>
        /// <param name="obj"></param>
        private void CraneWarningHand(object obj)
        {
            var _device = (CraneDevice)device;
            if (obj is Wcs.DefaultImplementCollection.Crane.CraneReportInfo)
            {
                var state = (Wcs.DefaultImplementCollection.Crane.CraneReportInfo)obj;
                List<AlarmRecord> list = new List<AlarmRecord>();
                if (state.ErrorCodeList != null && state.ErrorCodeList.Count() > 0)
                {
                    foreach (var item in state.ErrorCodeList)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorCode(_deviceType, item);
                        if (alarm == null)
                        {
                            alarm = new DeviceErrorType();
                            if (DeviceErrorHelper.DeviceErrorTypes.Length == 0)
                                alarm.Id = 1;
                            else
                                alarm.Id = DeviceErrorHelper.DeviceErrorTypes.Max(x => x.Id) + 1;
                            alarm.AlarmCategory = 1;
                            alarm.DeviceType = _deviceType;
                            alarm.DeviceErrorCode = item;
                            alarm.ErrorName = "未登记故障";
                            alarm.Description = "";
                            alarm.Levle = 0;
                            alarm.Solution = "";
                            alarm.IsFault = true;
                            alarm = DeviceErrorHelper.AddDeviceErrorType(alarm);
                            //continue;
                        }
                        if (alarm == null)
                            continue;

                        AlarmRecord alarmRecord = new AlarmRecord();
                        alarmRecord.Id = AlarmRecordHelper.GetWaringRecordId();
                        alarmRecord.WcsAlarmCode = alarm.Id.ToString();
                        alarmRecord.AlarmCategory = AlarmCategorys.DeviceError;
                        alarmRecord.AlarmLevel = (AlarmLevels)alarm.Levle;
                        alarmRecord.AlarmName = alarm.ErrorName;
                        alarmRecord.DeviceErrorCode = alarm.DeviceErrorCode.ToString();
                        alarmRecord.Device = _deviceName;
                        alarmRecord.OwnerDevice = _deviceName;
                        alarmRecord.DeviceType = _deviceType;
                        alarmRecord.BeginingAt = DateTime.Now;
                        alarmRecord.EndingAt = null;
                        alarmRecord.TotalMilliseconds = 0;
                        alarmRecord.Remarks = null;

                        list.Add(alarmRecord);
                    }
                }
                _alarmHand.Handler(list);
            }
        }
    }
}
