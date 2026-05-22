using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Wcs.Framework.Devices.Conveyor
{
    public sealed class ConveyorDevice:TcpProtocolTaskableDevice
    {
        NetPacket lastReceivedNetPacket = null;
        _DB1 lastReceivedDB1 = null;

        List<ConveyorLocation> _locations = new List<ConveyorLocation>();

        #region Properties
        public ConveyorLocation[] Locations
        {
            get
            {
                return _locations.ToArray();
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的货位当前任务列表
        /// </summary>
        public LocationCurrentTask[] LocationCurrentTasks
        {
            get
            {
                return ReadStatus<LocationCurrentTask>();
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的任务列表
        /// </summary>
        public TaskBlock[] Tasks
        {
            get
            {
                return ReadStatus<TaskBlock>();
            }
        }

        /// <summary>
        /// 获取从设备状态数据缓存中读取到的设备报警数据
        /// </summary>
        public MachineAlarms[] MachineAlarms
        {
            get
            {
                return ReadStatus<MachineAlarms>();
            }
        }

        /// <summary>   
        /// 获取从设备状态数据缓存中读取到的占位信号. 
        /// </summary>
        public OccupiedSignal[] OccupiedSignals
        {
            get
            {
                return ReadStatus<OccupiedSignal>();
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的光电状态.
        /// </summary>
        public OccupyStatus[] OccupyStatus
        {
            get
            {
                return ReadStatus<OccupyStatus>();
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的货位状态数据.
        /// </summary>
        public ConveyorLocationState[] ConveyorLocationStates
        {
            get
            {
                return ReadStatus<ConveyorLocationState>();
            }
        }
        /// <summary>
        /// 数据对比器<br />
        /// 该对象用于对接收到的数据进行对比。在外部可用处理该对象的关事件已达到跟踪数据变化的目的。
        /// </summary>
        public IDataComparer<NetTransferObject, _DB1> DataComparer { get; private set; }
        #endregion

        #region Events

        /// <summary>
        /// 在任务需要二次握手时发生
        /// </summary>
        public event EventHandler<NeedSecondHandShakeEventArgs> NeedSecondHandShake;
        /// <summary>
        /// 在任务当前位置发生改变时发生
        /// </summary>
        public event EventHandler<TaskCurrentLocationChangedEventArgs> TaskCurrentLocationChanged;
        /// <summary>
        /// 当占位信号改变时发生
        /// </summary>
        public event EventHandler<OccpiedSignalStatusChangedEventArgs> OccpiedSignalStatusChanged;
        #endregion,

        public ConveyorDevice(string name, Int32 no, IPEndPoint ipEndPoint, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, Boolean allowConcurrency, ITcpProtocolDataReceiver dataReceiver)
            :base(name,no,ipEndPoint,receiveTimeout,connectTimeout,sendTimeout,allowConcurrency,dataReceiver)
        {
            this.DataComparer = new DefaultTcpConveyorDataComparer();
        }

        #region Implement TcpProtocolTaskableDevice
        public override void SendTask(EquipmentAction action)
        {
            throw new NotImplementedException();
        }

        public override void CancelTask(EquipmentAction action)
        {
            throw new NotImplementedException();
        }

        public override TState Read<TState>()
        {
            throw new NotSupportedException();
        }

        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (!(netTransferObject is _DB1))
            {
                throw new Exception(string.Format("接收到未处理的数据类型 {0}",netTransferObject.GetType()));
            }

            _DB1 db1 = (_DB1)netTransferObject;

            if (netPacket == lastReceivedNetPacket)
            {
                lastReceivedDB1 = db1;
                return;
            }

            CompareResult<NetTransferObject>[] compareResult = this.DataComparer.Compare(lastReceivedDB1, db1);
            if (compareResult != null && compareResult.Length>0)
            {
                FireEvents(compareResult);
            }
            lastReceivedNetPacket = netPacket;
            lastReceivedDB1 = db1;
        }

        public override DeviceType DeviceType
        {
            get { return Devices.DeviceType.Conveyor; }
        }

        public override bool IsIdle
        {
            get { throw new NotImplementedException(); }
        }

        public override string[] Warnings
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Public Methods
        public TState[] ReadStatus<TState>()
            where TState : NetTransferObject
        {
            if (lastReceivedDB1 == null)
            {
                return new TState[0];
            }
            else
            {
                return lastReceivedDB1.Get<TState>();
            }
        }

        public void AcceptLocation(params ConveyorLocation[] locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            if (locations.Any(x => x.Device != null))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            foreach (var loc in locations)
            {
                loc.Device = this;
            }

            _locations.AddRange(locations);
        }
        #endregion

        #region Private Methos
        void FireNeedSecondHandShakeEvent(NeedSecondHandShakeEventArgs args)
        {
            this.ReceivedEvents.Push(this.NeedSecondHandShake, this, args);
        }

        void FireTaskCurrentLocationChangedEvent(TaskCurrentLocationChangedEventArgs args)
        {
            this.ReceivedEvents.Push(this.TaskCurrentLocationChanged, this, args);
        }

        void FireOccpiedSignalStatusChangedEvent(OccpiedSignalStatusChangedEventArgs args)
        {
            this.ReceivedEvents.Push(this.OccpiedSignalStatusChanged, this, args);
        }

        void FireEvents(CompareResult<NetTransferObject>[] _compareResults)
        {
            var changedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(TaskBlock)));
            var statusChangedTasks = changedTasks
                .Where(x => x.differences.Any(differece => differece.propertyName == "HandShake" || differece.propertyName == "TaskStatus"));
            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Error))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskErrorEvent(new TaskErrorEventArgs(Convert.ToInt32(newObject.AssignmentID), newObject.TaskStatus.ToString(), "任务不能执行"));
            }

            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Finished))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskCompletedEvent(new TaskCompletedEventArgs(Convert.ToInt32(newObject.AssignmentID)));
            }

            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Running))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskRunningEvent(new TaskRunningEventArgs(Convert.ToInt32(newObject.AssignmentID)));
            }

            foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).HandShake == HandShake.Readed))
            {
                TaskBlock newObject = (TaskBlock)task.newObject;
                FireTaskRunningEvent(new TaskRunningEventArgs(Convert.ToInt32(newObject.AssignmentID)));
            }

            var locationChangedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(LocationCurrentTask)))
                                    .Where(x => x.differences.Any(differece => differece.propertyName == "TaskNo"));

            foreach (var task in locationChangedTasks.Where(x => ((LocationCurrentTask)x.newObject).TaskNo != 0))
            {
                LocationCurrentTask state = (LocationCurrentTask)task.newObject;
                var location = this.Locations.SingleOrDefault(x => x.DeviceCode == state.PosNo.ToString());
                if (location == null)
                {
#warning 输出警告
                }
                else
                {
                    FireTaskCurrentLocationChangedEvent(new TaskCurrentLocationChangedEventArgs(state,location));
                }
            }

            var changedSignals = _compareResults
                    .Where(x => x.IsTypeOf(typeof(OccupiedSignal)))
                    .Where(x => ((OccupiedSignal)x.newObject).HandShake == OccupiedSignalHandShake.New);
            foreach (var signal in changedSignals)
            {
                var signalObj = (OccupiedSignal)signal.newObject;

                var location = this.Locations.SingleOrDefault(x => x.DeviceCode == signalObj.PosNo.ToString());
                if (location == null)
                {
#warning 输出警告
                }
                else
                {
                    if (location.AcceptRequestSignal)
                    {
                        FireOccpiedSignalStatusChangedEvent(new OccpiedSignalStatusChangedEventArgs(location, signalObj));
                    }
                }
            }
        }
        #endregion
    }
}
