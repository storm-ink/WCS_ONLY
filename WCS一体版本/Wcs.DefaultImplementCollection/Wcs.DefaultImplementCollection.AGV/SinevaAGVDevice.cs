using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.AGV
{
    [DisplayName("子系统接口")]
    public class SinevaAGVDevice : TcpProtocolTaskableDevice
    {
        Boolean WebServiceState = true;
        Boolean _任务状态检查启动标志 = false;

        public SinevaAGVDevice(string name, int no, int receiveTimeout, int connectTimeout, int sendTimeout, bool allowConcurrency, System.Net.IPEndPoint ipEndPoint, System.Net.IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, allowConcurrency, ipEndPoint, bindEndPoint, dataReceiver)
        {
           // _locations = new List<AGVSubSystemLocation>();
        }

        public Dictionary<int, string> _完成处理中的任务 = new Dictionary<int, string>();
        public Dictionary<int, string> _取消的任务 = new Dictionary<int, string>();

        private void 任务状态检查(object state)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    //完成检查
                    var equipmentTaskIds = SinevaAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.已完成,this.Name);
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
                                //var _action = unitOfWork.session.Query<SinevaAGVSubSystemAction>().FirstOrDefault(x => x.SendAGVTaskId == item);
                                var _action = unitOfWork.session.Query<SinevaAGVSubSystemAction>().FirstOrDefault(x => item.Contains(x.SendAGVTaskId));
                                if (_action != null)
                                    equipmentTaskId = _action.EquipmentTaskId;
                                else
                                    equipmentTaskId = -1;

                                unitOfWork.Commit();
                            }
                            if (equipmentTaskId > 0)
                            {
                                FireTaskCompletedEvent(new TaskCompletedEventArgs(equipmentTaskId));
                               // _完成处理中的任务.Add(equipmentTaskId, item);
                            }
                            else
                                SinevaAGVDatabaseHand.DeleteCompeletedTask(item);
                        }
                    }
                    //任务取消检查
                    //var canclequipmentTaskIds = SinevaAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.AGV系统主动取消, this.Name);
                    //if (canclequipmentTaskIds == null)
                    //{
                    //    continue;
                    //}

                    //foreach (var item in canclequipmentTaskIds)
                    //{
                    //    Int32 canclequipmentTaskId = 0;
                    //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    //    {
                    //        var _action = unitOfWork.session.Query<SinevaAGVSubSystemAction>().FirstOrDefault(x => x.SendAGVTaskId == item);
                    //        if (_action != null)
                    //            canclequipmentTaskId = _action.EquipmentTaskId;
                    //        else
                    //            canclequipmentTaskId = -1;

                    //        unitOfWork.Commit();
                    //    }
                    //    if (canclequipmentTaskId > 0)
                    //    {

                    //        FireTaskErrorEvent(new TaskErrorEventArgs(canclequipmentTaskId, "取消", "AGV取消"));
                         
                    //    }
                    //    else
                    //        SinevaAGVDatabaseHand.DeleteCompeletedTask(item);
                    //}
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

        //List<AGVSubSystemLocation> _locations;
        //public virtual AGVSubSystemLocation[] Locations
        //{
        //    get
        //    {
        //        return _locations.ToArray();
        //    }
        //}

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                //var equipmentTaskIds = SinevaAGVDatabaseHand.QueryEquipmentTaskIDs(this.Name);
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
            return new SinevaAGVUserInterfaceFrm();
        }

        public override IsIdleResult IsIdle
        {
            get
            {
                if (!SinevaAGVDatabaseHand.MiddleDatabaseState)
                {
                    return new IsIdleResult(false, "");
                }
                //if (!WebServiceState)
                //{
                //    return false;
                //}

                return new IsIdleResult(true, "");
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<String> warnings = new List<string>();

                if (!SinevaAGVDatabaseHand.MiddleDatabaseState)
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

        public override void SendTask(EquipmentAction action, params string[] args)
        {
           // String[] location = new String[] { "2019", "2004", "3016", "3003", "4020", "4015", "4008", "4002", "5017", "5003", "6020", "6003", "5034", "6036", "6042", "6075", "9022", "9030", "9037", };
           
            SinevaAGVSubSystemAction _action = (SinevaAGVSubSystemAction)action;
            AGVSubSystemLocation fromLocation = (AGVSubSystemLocation)LocationConverter.ToLocation(_action.LoadLocation);
            AGVSubSystemLocation toLocation = (AGVSubSystemLocation)LocationConverter.ToLocation(_action.UnloadLocation);
                 
            AGV_T_interface model = new AGV_T_interface();
            model.interCode = _action.SendAGVTaskId;
            
            //model.interCode = _action.Movement.Task.TaskCode;
            model.begionLoc = Convert.ToString(fromLocation.StationNo);
            model.endLoc = Convert.ToString(toLocation.StationNo);
            model.createDatetime = DateTime.Now;

            model.sort = "0";
            if (action.Movement.Task.AdditionalInfo.ContainsKey("priority"))
            {
                model.sort = Convert.ToString(action.Movement.Task.AdditionalInfo["priority"]);
            }
            model.interType = "3";  
            model.begionLevel = fromLocation.Level;
            model.endLevel = toLocation.Level;          
            model.state = "1";
            model.category = "1";
            model.SalverType = "1";
            model.TrayCode = action.ContainerCode.ToString();         
           if (!SinevaAGVDatabaseHand.InterTaskToAGV(model,action.DeviceName))
            {
                throw new Exception();
            }
           
        }

        public override void SendTaskPre(EquipmentAction action)
        {
            throw new NotImplementedException();
        }


       
        public override void CancelTask(EquipmentAction action)
        {
            var _action = (SinevaAGVSubSystemAction)action;
            var agvDevice = DeviceConverter.ToDevice<SinevaAGVDevice>(Name);
            var equipmentTaskIds = SinevaAGVDatabaseHand.QueryEquipmentTaskState(EquipmentTaskStatus.错误, _action.DeviceName);
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
            SinevaAGVResultCheck(result);
        }

        private void SinevaAGVResultCheck(string result)
        {
            if (result.Contains("<ErrCode>"))
            {
                var errorCode = result;
            }
        }

        //public override void stateUpdateProc(object obj)
        //{
        //    return;
        //}
    }
}
