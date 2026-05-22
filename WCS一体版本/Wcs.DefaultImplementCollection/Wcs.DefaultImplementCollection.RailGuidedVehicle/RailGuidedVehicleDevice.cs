using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    /// <summary>
    /// 穿梭车设备
    /// </summary>
    [System.ComponentModel.Description("穿梭车")]
    public class RailGuidedVehicleDevice : TcpProtocolTaskableDevice
    {
        StateTelexTransferObject lastStateTelexTransferObject = null;
        //string _空闲等待站点;
        public RailGuidedVehicleDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, false, ipEndPoint, bindEndPoint, dataReceiver)
        {
            //this.DeviceWarningFactory = new RailGuidedVehicleWarningFactory();

            this.Disconnected += RailGuidedVehicleDevice_Disconnected;
            this.Connected += RailGuidedVehicleDevice_Connected;
            //_空闲等待站点 = String.Format("{0}空闲待命站点", name);
        }

        Thread _freeWaitStationThreadProcThread;
        Boolean _freeWaitStationThreadIsRunning;
        public bool CanManual
        {
            get
            {
                var newObject = this.LastStatus;
                if (newObject == null)
                {
                    return false;
                }

                //设备处于手动模式
                if (newObject.State == RailGuidedVehicleStatus.手动模式)
                {
                    return false;
                }

                if (newObject.State == RailGuidedVehicleStatus.报警停机)
                {
                    return false;
                }


                #region
                //防止 二次下发穿梭车任务
                if (newObject.Event == RailGuidedVehicleEvent.TaskCompletionByManual
                    || newObject.Event == RailGuidedVehicleEvent.AutomaticTaskCompletion)
                {
                    return false;
                }
                #endregion
                Int32 equipmentTaskId = 0;
                if (!Int32.TryParse(newObject.TaskId, out equipmentTaskId))
                {
                    return false;
                }
                else
                {
                    if (equipmentTaskId != 0)
                    {
                        return false;
                    }
                }

                if (newObject.ErrorList.Any(x => !string.IsNullOrWhiteSpace(x)))
                    return false;
                //if (newObject.ErrorCode != 0)
                //{
                //    return false;
                //}

                if (!this.IsConnected)
                {
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// 只要返回的任务号为0都可以下任务。
        /// </summary>
        public override IsIdleResult IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                    return new IsIdleResult(false,"锁不为空");

                var newObject = this.LastStatus;
                if (newObject == null)
                    return new IsIdleResult(false, "未获取数据（LastStatus = null）");

                if (newObject.State != RailGuidedVehicleStatus.无货待命 && newObject.State != RailGuidedVehicleStatus.有货待命)
                    return new IsIdleResult(false, "穿梭车状态不为 无货待命/有货待命");

                //设备处于手动模式
                if (newObject.State == RailGuidedVehicleStatus.手动模式)
                    return new IsIdleResult(false, "穿梭车状态为 手动模式");

                if (newObject.State == RailGuidedVehicleStatus.报警停机)
                    return new IsIdleResult(false, "穿梭车状态为 报警停机");

                #region
                //防止 二次下发穿梭车任务
                if (newObject.Event == RailGuidedVehicleEvent.TaskCompletionByManual
                    || newObject.Event == RailGuidedVehicleEvent.AutomaticTaskCompletion)
                    return new IsIdleResult(false, "穿梭车事件为 手动完成/自动完成（等待程序自动清理）");

                #endregion
                Int32 equipmentTaskId = 0;
                if (!Int32.TryParse(newObject.TaskId, out equipmentTaskId))
                    return new IsIdleResult(false, "穿梭车上报任务号无法转换成Int32类型");
                else
                {
                    if (equipmentTaskId != 0)
                        return new IsIdleResult(false, "穿梭车上报任务号不为0");
                }

                if (newObject.ErrorList.Any(x => !string.IsNullOrWhiteSpace(x)))
                    return new IsIdleResult(false, $"穿梭车报警信息不为空（{string.Join("/",newObject.ErrorList)}）");

                if (!this.IsConnected)
                    return new IsIdleResult(false, "设备未连接");

                return new IsIdleResult(true, "");
            }
        }

        public StateTelexTransferObject LastStatus
        {

            get
            {
                return lastStateTelexTransferObject;
            }
            private set
            {
                lastStateTelexTransferObject = value;
            }

        }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                if (this.LastStatus == null)
                {
                    return new int[0];
                }

                Int32 equipmentTaskId = 0;
                if (int.TryParse(this.LastStatus.TaskId, out equipmentTaskId))
                {
                    return new int[] { equipmentTaskId };
                }
                else
                {
                    return new int[0];
                }
            }
        }

        //public RailGuidedVehicleStation[] Stations
        //{
        //    get
        //    {
        //        return _stations.ToArray();
        //    }
        //}
        //List<string> ErrorListDescription = new List<string> {
        //                                                        "38-地上盘急停按下",
        //                                                        "39-车体突出光电报警",
        //                                                        "40-输送机突出光电报警",
        //                                                        "41-安全门打开",
        //                                                        "42-行走变频器异常",
        //                                                        "43-移栽变频器异常",
        //                                                        "44-安全触边异常",
        //                                                        "45-条码数据错误",
        //                                                        "46-防火门不在上限",
        //                                                        "47-火警报警",
        //                                                        "48-上行或者下行极限位置",
        //                                                        "49-站点二次确认传感器异常",
        //                                                        "50-移栽搬入超时",
        //                                                        "51-移栽搬出超时",
        //                                                        "52-地上盘通讯异常",
        //                                                        "53-行走速度异常",
        //                                                        "54-车体未上电",
        //                                                        "55-车体急停按下",
        //                                                        "56-WCS命令下发错误",
        //                                                        "57-WCS急停命令",
        //                                                        "58-行走抱闸未打开",
        //                                                        "59-与输送机PLC通讯异常",
        //                                                        "60-输送机急停按下",
        //                                                        "61-行走变频器通讯异常",
        //                                                        "62-警告-RGV编号未设置",
        //                                                        "63-警告-Spare",
        //                                                        "64-警告-Spare",
        //                                                        "65-警告-Spare",
        //                                                        "66-警告-Spare",
        //                                                        "67-警告-可能相撞",
        //                                                        "68-警告-输送机交握信号异常",
        //                                                        "69-警告-交握时输送机未自动"};
        List<string> ErrorListDescription = new List<string> {
                                                                "1-站台货物越位报警",
                                                                "2-行走变频器故障",
                                                                "3-进出货变频器故障",
                                                                "4-进货超时报警",
                                                                "5-出货超时报警",
                                                                "6-测距仪损坏报警",
                                                                "7-测距仪数据丢失",
                                                                "8-电柜急停按下",
                                                                "9-HMI急停按下",
                                                                "10-小车货物越位报警",
                                                                "11-断电限位报警",
                                                                "12-对位光电1未检测到报警",
                                                                "13-对位光电2未检测到报警",
                                                                "14-输送线安全门开启",
                                                                "15-上位机急停",
                                                                "16-距离过近",
                                                                "17-前车未离开安全区域报警"};
        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                var newObject = this.LastStatus;
                if (newObject == null)
                {
                    result.Add("设备状态未获取");
                }
                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));
                }

                if (newObject != null)
                {
                    //if (newObject.ErrorCode != 0)
                    //{
                    //    var alarm = this.DeviceWarningFactory.Create(this, this.LastStatus.ErrorCode.ToString(), null);
                    //    if (alarm == null)
                    //    {
                    //        result.Add(string.Format("处于报警状态 {0},{1}", newObject.ErrorCode, ""));
                    //    }
                    //    else
                    //    {
                    //        result.Add(string.Format("处于报警状态 {0},{1}", newObject.ErrorCode, alarm.Description));
                    //    }
                    //}
                    var errorList = newObject.ErrorList.Where(x => !string.IsNullOrWhiteSpace(x) && x != "0");
                    foreach (var item in errorList)
                    {
                        var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorCode("穿梭车", item);
                        if (alarm == null)
                            result.Add($"报警码{item}，未知报警");
                        else
                            result.Add($"报警码{item}，{alarm.ErrorName}");
                    }
                    if (newObject.State == RailGuidedVehicleStatus.手动模式)
                    {
                        result.Add(string.Format("设备被切换为手动模式"));
                    }

                    if (newObject.State == RailGuidedVehicleStatus.报警停机)
                    {
                        result.Add("设备正处于报警停机状态");
                    }


                    if (newObject.Event == RailGuidedVehicleEvent.TaskCompletionByManual
                        || newObject.Event == RailGuidedVehicleEvent.AutomaticTaskCompletion)
                    {
                        result.Add("设备事件正处于" + newObject.Event.GetDescription() + ",无法下发新任务");
                    }
                }

                return result.ToArray();
            }
        }

        public virtual void AcceptStation(params RailGuidedVehicleStation[] stations)
        {
            if (stations == null)
            {
                throw new ArgumentNullException("stations");
            }

            if (stations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            foreach (var loc in stations)
            {
                if (this._locations.Any(x => x.ToConvertibleCode() == loc.ToConvertibleCode()))
                {
                    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", loc, this));
                }
            }

            //var invalidLocations = stations.Where(x => !(x is ILocationWildcard)).Intersect(this.Stations.Where(x => !(x is ILocationWildcard)));
            //if (invalidLocations.Any())
            //{
            //    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            //}

            //invalidLocations = stations.Where(x => x is ILocationWildcard).Intersect(this.Stations.Where(x => x is ILocationWildcard));
            //if (invalidLocations.Any())
            //{
            //    throw new InvalidOperationException(String.Format("通配符位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            //}
            _locations.AddRange(stations);
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new RailGuidedVehicleDeviceUserInterface();
        }

        /// <summary>
        /// 艾芬达项目，穿梭车调度设计：小车不在站点时返回的站点号为0
        /// </summary>
        /// <returns></returns>
        public RailGuidedVehicleStation GetCurrentStation()
        {
            if (lastStateTelexTransferObject == null)
            {
                return null;
            }

            if (!lastStateTelexTransferObject.AtStation)
            {
                return null;
            }

            return _locations.Select(x => (RailGuidedVehicleStation)x).FirstOrDefault(x => x.StationNo == lastStateTelexTransferObject.CurrentStation);
        }

        public override TState Read<TState>()
        {
            throw new NotImplementedException();
        }

        public override void SendTask(EquipmentAction action, params string[] args)
        {
            var cmd = action.ToAddCommand();
        }

        public override void SendTaskPre(EquipmentAction action)
        {
            throw new NotImplementedException();
        }

        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (netTransferObject is StateTelexTransferObject)
            {
                //if (lastStateTelexTransferObject != null)
                //{
                //    var compareResult = lastStateTelexTransferObject.Compare((StateTelexTransferObject)netTransferObject);
                //    lastStateTelexTransferObject = (StateTelexTransferObject)netTransferObject;
                //    FireEvents(compareResult);
                //    if (compareResult != null && compareResult.differences != null && compareResult.differences.Where(x => x.propertyName != "Position").Count() > 0)
                //    {
                //        ReceivedDataLogHelper.Log(new StateTelexTransferObjectDataLog(this, (StateTelexTransferObject)netTransferObject));
                //        string msg = string.Join("，", compareResult.differences.Select(x => string.Format("{0}：{1}->{2}", x.propertyName, x.oldValue, x.newValue)).ToArray());
                //        this._logger.Trace1(msg, this, compareResult);
                //    }
                //}
                //else
                //{
                //    //ReceivedDataLogHelper.Log(new StateTelexTransferObjectDataLog(this, (StateTelexTransferObject)netTransferObject));
                //    lastStateTelexTransferObject = (StateTelexTransferObject)netTransferObject;
                //    FireEvents((StateTelexTransferObject)netTransferObject);
                //}
                lastStateTelexTransferObject = (StateTelexTransferObject)netTransferObject;
                FireEvents((StateTelexTransferObject)netTransferObject);
            }
        }

        private void FireEvents(StateTelexTransferObject stateTelexTransferObject)
        {
            if (stateTelexTransferObject == null)
            {
                return;
            }

            Int32 equipmentTaskId = 0;
            Int32.TryParse(stateTelexTransferObject.TaskId, out equipmentTaskId);

            //完成信号
            if (equipmentTaskId != 0 &&
                (stateTelexTransferObject.Event == RailGuidedVehicleEvent.AutomaticTaskCompletion
                || stateTelexTransferObject.Event == RailGuidedVehicleEvent.TaskCompletionByManual))
            {
                FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
            }

            //手动清缓存
            if (stateTelexTransferObject.State == RailGuidedVehicleStatus.B)
            {
                ThreadPool.QueueUserWorkItem(n =>
                {
                    try
                    {
                        ClearTaskCommand cmd = new ClearTaskCommand();
                        this.Write<ClearTaskCommand>(cmd, cmd.SendSuccess);
                        _logger.Info($"收到 {this.Name} 上报 {RailGuidedVehicleStatus.B.GetDescription()} 状态，清除命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                    }
                });
            }

            //报警信号
            if (!string.IsNullOrWhiteSpace(stateTelexTransferObject.ErrorCode))
            {
                //var warning = this.DeviceWarningFactory.Create(this, stateTelexTransferObject.ErrorCode.ToString(), null);

                ////报警停机并且有任务
                //if (stateTelexTransferObject.State == RailGuidedVehicleStatus.报警停机 && equipmentTaskId != 0)
                //{
                //    FireTaskErrorEvent(new TaskErrorEventArgs(equipmentTaskId, warning.Code, warning.Description));
                //}

                //AddWarning(warning);

                //FireDeviceErrorEvent(new DeviceWarningEventArgs(warning));
            }

            //报警信号恢复
            if (string.IsNullOrWhiteSpace(stateTelexTransferObject.ErrorCode))
            {
                EndingWarning();
            }
        }

        private void FireEvents(CompareResult<NetTransferObject> compareResult)
        {
            if (compareResult == null || compareResult.differences == null || compareResult.differences.Length == 0)
            {
                return;
            }

            //if (!compareResult.IsTypeOf(typeof(StateTelexTransferObject)))
            //{
            //    return;
            //}

            StateTelexTransferObject newObject = (StateTelexTransferObject)compareResult.newObject;

            Int32 equipmentTaskId = 0;
            Int32.TryParse(newObject.TaskId, out equipmentTaskId);

            //完成信号
            if (compareResult.differences.Any(x => x.propertyName == "Event"
                && (x.newValue == RailGuidedVehicleEvent.AutomaticTaskCompletion
                        || x.newValue == RailGuidedVehicleEvent.TaskCompletionByManual)))
            {
                FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
            }

            //手动清缓存
            if (compareResult.differences.Any(x => x.propertyName == "State" && x.newValue == RailGuidedVehicleStatus.B))
            {
                ThreadPool.QueueUserWorkItem(n =>
                {
                    try
                    {
                        ClearTaskCommand cmd = new ClearTaskCommand();
                        this.Write<ClearTaskCommand>(cmd, cmd.SendSuccess);
                        _logger.Info($"收到 {this.Name} 上报 {RailGuidedVehicleStatus.B.GetDescription()} 状态，清除命令发送成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                    }
                });
            }

            //报警信号
            if (compareResult.differences.Any(x => x.propertyName == "ErrorCode" && x.newValue != 0))
            {
                //var warning = this.DeviceWarningFactory.Create(this, newObject.ErrorCode.ToString(), null);

                ////报警停机并且有任务
                //if (newObject.State == RailGuidedVehicleStatus.报警停机 && equipmentTaskId != 0)
                //{
                //    FireTaskErrorEvent(new TaskErrorEventArgs(equipmentTaskId, warning.Code, warning.Description));
                //}


                //AddWarning(warning);

                //FireDeviceErrorEvent(new DeviceWarningEventArgs(warning));
            }

            //报警信号恢复
            if (compareResult.differences
                .Any(x => x.propertyName == "ErrorCode"
                    && x.newValue == 0
                    )
                )
            {
                EndingWarning();
            }
        }

        void RailGuidedVehicleDevice_Connected(Device device, ConnectedEventArgs args)
        {
            args.Handled = true;
            lastStateTelexTransferObject = null;
            if (_freeWaitStationThreadIsRunning)
            {
                this._logger.Debug1(string.Format("{0} 请求回报状态线程已运行，不再启动。", this), this);
                return;
            }

            //_freeWaitStationThreadProcThread = new Thread(freeWaitStationThreadProc);
            //_freeWaitStationThreadProcThread.IsBackground = true;
            //_freeWaitStationThreadProcThread.Name = String.Format("{0}设备空闲发送到指定站点等待线程", this);
            //_freeWaitStationThreadProcThread.StartAndManaged();
        }

        void RailGuidedVehicleDevice_Disconnected(Device device, DisconnectEventArgs args)
        {
            args.Handled = true;
            lastStateTelexTransferObject = null;
            if (_freeWaitStationThreadIsRunning)
            {
                _freeWaitStationThreadIsRunning = false;
                this._logger.Debug1(string.Format("{0} 已断开连接，写入停止请求回报状态线程标志。", this), this);
            }
        }

        protected override int ReceiveBufferSize
        {
            get
            {
                return 38;
            }
        }

        //public override void stateUpdateProc(object obj)
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            Thread.Sleep(1000);
        //            DeviceState deviceState;
        //            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
        //            {
        //                deviceState = unitOfWork.session.Query<DeviceState>().SingleOrDefault(x => x.DeviceName == this.Name);
        //                unitOfWork.Commit();
        //            }
        //            if (deviceState == null)
        //            {
        //                deviceState = new DeviceState() { DeviceName = this.Name, State = DeviceStatus.UnKnow, Device = this.Name, Description = "初始化" };
        //                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
        //                {
        //                    unitOfWork.session.Save(deviceState);
        //                    unitOfWork.Commit();
        //                }
        //                continue;
        //            }
        //            if (!this.IsConnected)
        //            {
        //                deviceState.State = DeviceStatus.Error;
        //                deviceState.Description = "未连接";
        //            }
        //            else if (this.Warnings != null && this.Warnings.Count() != 0)
        //            {
        //                deviceState.State = DeviceStatus.Error;
        //                deviceState.Description = String.Join("/", this.Warnings);
        //            }
        //            else
        //            {
        //                deviceState.State = DeviceStatus.OK;
        //                deviceState.Description = "";
        //            }

        //            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
        //            {
        //                unitOfWork.session.SaveOrUpdate(deviceState);
        //                unitOfWork.Commit();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error1(ex, this);
        //        }
        //    }
        //}
    }
}
