using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Wcs;
using Wcs.Framework;

namespace BOE.设备相关.AGV.GSAGV
{
    [DisplayName("GSAGV接口")]
    public class GSAGVDevice : TcpProtocolTaskableDevice
    {
        Boolean WebServiceState = true;
        Boolean _任务状态检查启动标志 = false;

        public GSAGVDevice(string name, int no, int receiveTimeout, int connectTimeout, int sendTimeout, bool allowConcurrency, System.Net.IPEndPoint ipEndPoint, System.Net.IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, allowConcurrency, ipEndPoint, bindEndPoint, dataReceiver)
        {

        }

        public Dictionary<int, string> _完成处理中的任务 = new Dictionary<int, string>();

        private void 任务状态检查(object state)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    List<Task> list;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        var q = unitOfWork.session.Query<Task>();
                        list = q.ToList();
                        var key = string.Empty;

                        Int32 keyIntValue;
                        Int32.TryParse(key, out keyIntValue);

                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            list = list.Where(x =>
                                            x.ContainerCodes.Any(containerCode => containerCode.Contains(key)) //条码
                                            || (x.Description != null && x.Description.Contains(key))
                                            || (x.TaskType != null && x.TaskType.Contains(key))
                                            || x.CurrentLocation.UserCode.Contains(key)
                                            || x.CurrentLocation.DeviceCode.Contains(key)
                                            || x.TaskCode.Contains(key) //任务号
                                            || x.Id == keyIntValue      //任务id
                                            || x.StartLocation.UserCode.Contains(key) //起点
                                            || x.EndLocation.UserCode.Contains(key)//终点
                                            || (x.MasterTaskCode + "").Contains(key)//父任务号
                                            || x.Movements
                                                .Any(movement => movement.Id == keyIntValue       //逻辑动作id
                                                  || movement.RouteId == keyIntValue              //路径id
                                                  || movement.DeviceName.Contains(key) //设备名称
                                                  || movement.StartLocation.UserCode.Contains(key) //起点
                                                  || movement.EndLocation.UserCode.Contains(key)//终点
                                                  || movement.EquipmentActions.Any(action => action.Id == keyIntValue
                                                                                          || action.EquipmentTaskId == keyIntValue
                                                                                          || action.DeviceName.Contains(key) //设备名称
                                                                                   )
                                                    )
                                            ).ToList();
                        }

                        unitOfWork.Commit();
                    }
                    var equipmentTaskIds = GSAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.已完成);
                    if (equipmentTaskIds == null)
                    {
                        continue;
                    }

                    foreach (var item in equipmentTaskIds)
                    {
                        Int32 equipmentTaskId = 0;
                        if (!_完成处理中的任务.Values.Contains(item))
                        {
                            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                            {
                                var _action = unitOfWork.session.Query<GSAGVSubSystemAction>().FirstOrDefault(x => x.SendAGVTaskId == item);
                                if (_action != null)
                                    equipmentTaskId = _action.EquipmentTaskId;
                                else
                                    equipmentTaskId = -1;

                                unitOfWork.Commit();
                            }
                            if (equipmentTaskId > 0)
                            {
                                FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
                                _完成处理中的任务.Add(equipmentTaskId, item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, this);
                }
            }
        }

        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            throw new NotImplementedException();
        }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                //var equipmentTaskIds = GSAGVDatabaseHand.QueryEquipmentTaskIDs();
                //if (equipmentTaskIds == null)
                //    throw new Exception("查询中间表失败获取已使用任务号失败");
                return new int[] { };
            }
        }

        public override TState Read<TState>()
        {
            throw new NotImplementedException();
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new GSAGVUserInterfaceFrm();
        }

        public override bool IsIdle
        {
            get
            {
                if (!GSAGVDatabaseHand.MiddleDatabaseState)
                {
                    return false;
                }
                //if (!WebServiceState)
                //{
                //    return false;
                //}

                return true;
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<String> warnings = new List<string>();

                if (!GSAGVDatabaseHand.MiddleDatabaseState)
                {
                    warnings.Add("中间库连接失败");
                }
                //if (!WebServiceState)
                //{
                //    warnings.Add("WebService服务调用失败");
                //}
                return warnings.ToArray();
            }
        }

        public override bool Connect()
        {
            if (!_任务状态检查启动标志)
            {
                _logger.Info(String.Format("开始启动 {0} 的任务状态检查线程", this.Name));
                System.Threading.ThreadPool.QueueUserWorkItem(任务状态检查);
                _logger.Info(String.Format("启动 {0} 的任务状态检查线程成功", this.Name));
                _任务状态检查启动标志 = true;
            }

            return true;
        }

        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }
        public override void Disconnect()
        {
        }

        public virtual void AcceptLocation(params AGVSubSystemLocation[] locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            if (locations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            var invalidLocations = locations.Where(x => !(x is ILocationWildcard)).Intersect(this.Locations.Where(x => !(x is ILocationWildcard)));
            if (invalidLocations.Any())
            {
                throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            }

            invalidLocations = locations.Where(x => x is ILocationWildcard).Intersect(this.Locations.Where(x => x is ILocationWildcard));
            if (invalidLocations.Any())
            {
                throw new InvalidOperationException(String.Format("通配符位置 {0} 在 {1} 中已存在", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray()), this));
            }
            _locations.AddRange(locations);
        }

        public override void Write<TData>(TData data, Func<TaskableDevice, TData, bool> isSuccess)
        {

        }

        public override void SendTask(EquipmentAction action)
        {
            String[] location = new String[] { "00-001-2100", "00-001-3100" };

            GSAGVSubSystemAction _action = (GSAGVSubSystemAction)action;
            AGVSubSystemLocation fromLocation = (AGVSubSystemLocation)LocationConverter.ToLocation(_action.LoadLocation);
            AGVSubSystemLocation toLocation = (AGVSubSystemLocation)LocationConverter.ToLocation(_action.UnloadLocation);

            AGV_T_interface model = new AGV_T_interface();
            model.interCode = _action.SendAGVTaskId;
            model.beginLoc = fromLocation.DeviceCode;
            //model.beginLevel = 1;
            model.endLoc = toLocation.DeviceCode;
           // model.endLevel = 1;
            model.createDatetime = DateTime.Now;
            model.interType = "3";
            model.state = "1";
            model.sort = "0";
            model.SalverType = "1";

            if (location.Contains(fromLocation.UserCode))
            {
                model.beginLevel = 1;
                model.endLevel = toLocation.Level;
            }
            else if (location.Contains(toLocation.UserCode))
            {
                if (_action.Movement.Task.AdditionalInfo.Keys.Contains("数量"))
                {
                    model.beginLevel = fromLocation.Level;

                    var priviod = _action.Movement.Task.AdditionalInfo["数量"];
                    model.endLevel = Convert.ToInt32(priviod);
                }
            }
            else
            {
                model.beginLevel = fromLocation.Level;
                model.endLevel = toLocation.Level;
            }

            model.TrayCode = _action.ContainerCode.ToString();
            if (!GSAGVDatabaseHand.InterTaskToAGV(model))
            {
                throw new Exception();
            }
            //using (NDCService.HostToAGVPortTypeClient client = new NDCService.HostToAGVPortTypeClient())
            //{
            //    try
            //    {
            //        client.AddNewOrder(cmd.ToXml());
            //        client.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        if (client != null)
            //            client.Abort();

            //        throw ex;
            //    }
            //}
        }

        public override void CancelTask(EquipmentAction action)
        {
            var _action = (GSAGVSubSystemAction)action;
            var agvDevice = DeviceConverter.ToDevice<GSAGVDevice>(Name);
            var equipmentTaskIds = GSAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.错误);
            if (equipmentTaskIds == null)
            {
                return;
            }
            else if (!equipmentTaskIds.Any(x => x.Contains(_action.SendAGVTaskId)))
            {
                return;
            }
            CancleTaskCommand cmd = new CancleTaskCommand(_action.SendAGVTaskId, "");
            string result = "";
            //using (NDCService.HostToAGVPortTypeClient client = new NDCService.HostToAGVPortTypeClient())
            //{
            //    try
            //    {
            //        result = client.AddNewOrder(cmd.ToXml());
            //        client.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        if (client != null)
            //            client.Abort();

            //        throw ex;
            //    }
            //}
            GSAGVResultCheck(result);
        }

        private void GSAGVResultCheck(string result)
        {
            if (result.Contains("<ErrCode>"))
            {
                var errorCode = result;
            }
        }

        public override void stateUpdateProc(object obj)
        {
            while (true)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }
    }
}
