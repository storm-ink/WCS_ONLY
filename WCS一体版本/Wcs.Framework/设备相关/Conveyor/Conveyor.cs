using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using Wcs.Framework.EquipmentActions;
using Wcs.Framework.Impl;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 表示一个输送线设备
    /// </summary>
    /// <example>
    ///<h1>如何配置一个输送线设备</h1>
    ///<h5>首先我们需要了解输送线设备和 Wcs 的通讯协议，这里我们提供了输送线设备和 Wcs 的通讯协议的简要叙述：</h5>
    ///<ul>
    ///<li>1、Wcs 如何得知设备的运行状态（包括任务、货位、报警等等）？<br />
    ///	   输送线设备会一直向与之建立 Tcp 连接的 Wcs 系统发送 db2 数据块。该数据块包括设备中各种类型的数据，并以约定好的形式编码发送。<br />
    ///	   Wcs 只要在和输送线设备建立连接后不断解析收到的数据就能获取到输送线设备当前的实时状态数据。
    ///</li>
    ///<li>2、Wcs 如何向设备发送指令（包括添加删除任务、删除占位等等）？<br />
    ///	   输送线设备中有一个名为 db1 的数据块，这里存储了所有 Wcs 发送给输送线设备的指令。<br />
    ///	   所以 wcs 只要以约定好的形式编码发送 db1 给输送线设备即可。<br />
    ///</li>
    ///</ul>
    ///<b><i>通过上面的介绍我们可以得知 Wcs 需要知道如何解码输送线设备发过来的 db2 数据，以及如何编码发送给输送线设备的 db1 数据。</i></b>
    ///<para>为了达到这样的目的， Conveyor 提供了两个属性：ReceiverDecoder、SenderDecoder，它们被用来处理接入及发送数据的处理。</para>
	///<para>从命名上我们就可以看出：<br />
	///ReceiverDecoder 用于接收解码，也就是 Wcs 对接收到的 db2 二进制数据进行解、编码，转换为 Wcs 系统内状态数据对象;<br />
    ///SenderDecoder   用于发送编码，也就是 Wcs 向输送线设备发送数据时将 Wcs系统内状态数据对象转换为输送线设备可识别的二进制数据;
    ///</para>
    ///<para>
    ///ReceiverDecoder、SenderDecoder 的类型是框架提供的一个 <see cref="T:Wcs.Framework.Devices.NetPackageDecoder"/> 的默认实现子类 <see cref="T:Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder"/>。
    ///</para>
    ///<h5>经过了解，我们现在开始配置工作：</h5>
    ///<ul>
    ///<li>1、添加新的 db1 及 db2 编码解码器配置。具体内容可点击 <see cref="T:Wcs.Framework.Impl.DefaultConveyorNetPackageDecoder"/> 查看"如何配置一个新的输送线网络数据包解码器"。</li>
    ///<li>2、添加新的 conveyor 设备配置节点。具体内容可点击 <see cref="T:Wcs.Framework.Cfg.ConveyorElement"/> 查看"如何配置一个新的输送线设备"。</li>
    ///<li>3、添加新的 route 路径配置节点。具体内容可点击 <see cref="T:Wcs.Framework.Cfg.RouteElement"/> 查看"如何配置设备中的路径集合"</li>
    /// </ul>
    /// </example>
    public class Conveyor:TaskableDevice 
    {
        /// <summary>
        /// 作为接收方使用的数据包解码器
        /// </summary>
        public DefaultConveyorNetPackageDecoder ReceiverDecoder { get; set; }
        /// <summary>
        /// 作为发送方使用的数据包解码器
        /// </summary>
        public DefaultConveyorNetPackageDecoder SenderDecoder { get; set; }

        /// <summary>
        /// 在任务需要二次握手时发生
        /// </summary>
        public event ConveyorNeedSecondHandShakeEventHandler NeedSecondHandShake;
        /// <summary>
        /// 在任务当前位置发生改变时发生
        /// </summary>
        public event ConveyorTaskCurrentLocationChangedEventHandler TaskCurrentLocationChanged;
        /// <summary>
        /// 当占位信号改变时发生
        /// </summary>
        public event ConveyorOccpiedSignalStatusChangedEventHandler OccpiedSignalStatusChanged;

        /// <summary>   
        ///  构造函数.
        /// </summary>
        /// <param name="deviceName">       设备名称. </param>
        /// <param name="ip">               远程地址. </param>
        /// <param name="port">             端口. </param>
        /// <param name="receiveTimeout">   接收超时（在指定的毫秒内如果未接收到来自设备的数据，连接将断开）. </param>
        /// <param name="connectTimeout">   连接超时. </param>
        /// <param name="sendTimeout">      数据发送超时. </param>
        /// <param name="deviceNo">         设备编号. </param>
        /// <param name="receiverDecoder">  作为接收方使用的数据包解码器. </param>
        /// <param name="senderDecoder">    作为发送方使用的数据包解码器. </param>
        /// <param name="logTarget">        日志输出对象. </param>
        public Conveyor(string name, String ip, Int32 port, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, Boolean allowConcurrency, DefaultConveyorNetPackageDecoder receiverDecoder, DefaultConveyorNetPackageDecoder senderDecoder, LogTarget logTarget)
            : base(name, ip, port, receiveTimeout, connectTimeout, sendTimeout,allowConcurrency)
        {
            this.ReceiverDecoder = receiverDecoder;
            this.SenderDecoder = senderDecoder;
            this.StatusDataComparer = new DefaultConveyorStatusDataComparer(this.ReceiverDecoder);
            this.lastReceiveBytesFromConveyor = null;
            this.StatusDataComparer.DataChanged += (compareResult) =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in compareResult.differences)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                        sb.Append(item.ToString());
                    }
                    else
                    {
                        sb.Append(item.ToString());
                    }
                }

                this.Logger.Info(sb.ToString(), this, compareResult);
                sb = null;
            };

            this.Disconnected += (obj, e) =>
            {
                this.updateStatus(null);

                this.lastReceiveBytesFromConveyor = null;
                ExecuteUpdateStatusActions(null);
            };
        }
        /// <summary>
        /// 获取设备组包对象
        /// </summary>
        public ConveyorNetPackage NetPackage
        {
            get
            {
                return _receivedNetPackage ;
            }
        }
        
        /// <summary>
        /// 判断指定的对象是否和本对象相等。
        /// </summary>
        /// <param name="obj">指定的要比较的对象</param>
        /// <returns>如果指定的 Object 等于当前的对象，则为 true；否则为 false。 </returns>
        public override bool Equals(object obj)
        {
            NewConveyor conveyor = obj as NewConveyor;
            if (conveyor == null) return false;

            return string.Equals(conveyor.DeviceName, this.DeviceName, StringComparison.CurrentCultureIgnoreCase);
        }
        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>32 位有符号整数哈希代码。</returns>
        public override int GetHashCode()
        {
            return string.Concat(this.DeviceType, this.DeviceName).GetHashCode();
        }

        /// <summary>
        /// 从最后收到的状态数据中读取指定类型的状态数据
        /// </summary>
        /// <typeparam name="T">    泛型参数. </typeparam>
        public virtual T[] GetStatusFromLastReceivedPackage<T>() where T : NetTransferObject, new()
        {
            if (this.lastReceiveBytesFromConveyor == null
                || this.lastReceiveBytesFromConveyor == null
                || this.lastReceiveBytesFromConveyor.Length == 0)
            {
                return null;
            }

            return this.ReceiverDecoder.Get<T>(this.lastReceiveBytesFromConveyor);
        }
        /// <summary>
        /// 指示设备当前是否处于空闲状态（可接受任务）.
        /// </summary>
        public override bool IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    return false;
                }

                if (Tasks != null
                    && _tcpClient != null
                    && _tcpClient.Client != null
                    && _tcpClient.Client.Connected
                    && _tcpClient.GetStream() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的货位当前任务列表
        /// </summary>
        public LocationCurrentTask[] LocationCurrentTasks { get; private set; }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的任务列表
        /// </summary>
        public TaskBlock[] Tasks { get; protected set; }

        /// <summary>
        /// 获取当前输送线设备中包含的所有位置
        /// </summary>
        public ConveyorLocation[] Locations { get; internal set; }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的设备报警数据
        /// </summary>
        public MachineAlarms[] MachineAlarms { get; protected set; }

        /// <summary>   
        /// 获取从设备状态数据缓存中读取到的占位信号. 
        /// </summary>
        public OccupiedSignal[] OccupiedSignals { get; protected set; }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的光电状态.
        /// </summary>
        public OccupyStatus[] OccupyStatus { get; protected set; }
        /// <summary>
        /// 获取从设备状态数据缓存中读取到的货位状态数据.
        /// </summary>
        public ConveyorLocationState[] ConveyorLocationStates { get; protected set; }
        
        /// <summary>
        /// 获取该输送线使用的状态数据对比器
        /// </summary>
        public DefaultConveyorStatusDataComparer StatusDataComparer { get; private set; }

        /// <summary>
        /// 上一次接收到的数据缓存对象
        /// </summary>
        byte[] lastReceiveBytesFromConveyor { get; set; }
        /// <summary>
        /// 将状态数据更新到当前对象
        /// </summary>
        /// <param name="receivedBytes">接收到的状态数据</param>
        private void updateStatus(byte[] receivedBytes)
        {
            if (receivedBytes == null || receivedBytes.Length == 0)
            {
                this.LocationCurrentTasks = null;
                this.Tasks = null;
                this.MachineAlarms = null;
                this.OccupiedSignals = null;
                this.OccupyStatus = null;
                this.ConveyorLocationStates = null;
            }
            else
            {
                this.LocationCurrentTasks = this.ReceiverDecoder.Get<LocationCurrentTask>(receivedBytes);
                this.Tasks = this.ReceiverDecoder.Get<TaskBlock>(receivedBytes);
                this.MachineAlarms = this.ReceiverDecoder.Get<MachineAlarms>(receivedBytes);
                this.OccupiedSignals = this.ReceiverDecoder.Get<OccupiedSignal>(receivedBytes);
                this.OccupyStatus = this.ReceiverDecoder.Get<OccupyStatus>(receivedBytes);
                this.ConveyorLocationStates = this.ReceiverDecoder.Get<ConveyorLocationState>(receivedBytes);
            }
        }

        Boolean isFirstLoad = false;
        /// <summary>
        /// 更新设备当前状态信息.
        /// </summary>
        /// <param name="lastReceivedStatus">最后收到的网络数据包结构</param>
        protected override void UpdateStatus(NetPackage lastReceivedStatus)
        {
            var datagram = lastReceivedStatus.FetchPackage() as ConveyorDatagram;
            if (datagram == null || datagram.GetBytes().Length == 0)
            {
                return;
            }

            ExecuteUpdateStatusActions(datagram.Data);

            CompareResult<NetTransferObject>[] _compareResults = null;

            if (lastReceiveBytesFromConveyor == null)
            {
                isFirstLoad = true;
            }

            if (lastReceiveBytesFromConveyor == null || !lastReceiveBytesFromConveyor.SequenceEqual(datagram.Data))
            {
                if (Wcs.Framework.Cfg.Configuration.GetSetting<Boolean>("对接收到的输送线数据包进行效验", true))
                {
                    if (!_receivedNetPackage.CheckPackage(datagram))
                    {
                        lastReceiveBytesFromConveyor = null;
                        String msg = String.Format("{0} 接收到了数据，但校验失败。", this);
                        this.Logger.Warning(msg, this, datagram.GetBytes());
                        updateStatus(null);

#warning 是否记录数据效验失败的包
                        if (Cfg.Configuration.GetSetting<Boolean>("记录数据效验失败的包", false))
                        {
                            string dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(NewConveyor).Assembly.Location), "无效的数据包");
                            dir = System.IO.Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd"));
                            dir = System.IO.Path.Combine(dir, this.DeviceName);
                            if (!System.IO.Directory.Exists(dir))
                            {
                                System.IO.Directory.CreateDirectory(dir);
                            }
                            string fileName = System.IO.Path.Combine(dir, System.IO.Path.GetRandomFileName());
                            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.FileStream(fileName, System.IO.FileMode.Create)))
                            {
                                bw.Write(datagram.GetBytes());
                                bw.Flush();
                                bw.Close();
                            }
                        }
                        return;
                    }
                }

                updateStatus(datagram.Data);

                _compareResults = this.StatusDataComparer.Compare(datagram.Data);
                lastReceiveBytesFromConveyor = datagram.Data;
            }

            #region 执行上一次未处理的数据
            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_ERROR))
            {
                OnTaskError(Convert.ToUInt32(item.Args[0]), Convert.ToString(item.Args[1]), Convert.ToString(item.Args[2]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_COMPLETED))
            {
                OnTaskCompleted(Convert.ToUInt32(item.Args[0]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get(DelayedDeviceStatusChangedNotify.TASK_RUNNING))
            {
                OnTaskRunning(Convert.ToUInt32(item.Args[0]));
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get("NeedSecondHandShake"))
            {
                OnNeedSecondHandShake((TaskBlock)item.Args[0]);
            }

            foreach (var item in this.DelayedDeviceStatusChangedNotify.Get("TaskCurrentLocationChanged"))
            {
                OnTaskCurrentLocationChanged((LocationCurrentTask)item.Args[0]);
            }
            #endregion

            #region 如果有新的变化数据
            if (_compareResults != null && this.Tasks != null)
            {
                var changedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(TaskBlock)));
                var statusChangedTasks = changedTasks
                    .Where(x => x.differences.Any(differece => differece.propertyName == "HandShake" || differece.propertyName == "TaskStatus"));
                foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Error))
                {
                    OnTaskError(((TaskBlock)task.newObject).AssignmentID, ((TaskBlock)task.newObject).TaskStatus.ToString(), "任务不能执行");
                }

                foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Finished))
                {
                    OnTaskCompleted(((TaskBlock)task.newObject).AssignmentID);
                }

                foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).TaskStatus == TaskStatus.Running))
                {
                    OnTaskRunning(((TaskBlock)task.newObject).AssignmentID);
                }

                foreach (var task in statusChangedTasks.Where(x => ((TaskBlock)x.newObject).HandShake == HandShake.Readed))
                {
                    OnNeedSecondHandShake(((TaskBlock)task.newObject));
                }

                var locationChangedTasks = _compareResults.Where(x => x.IsTypeOf(typeof(LocationCurrentTask)))
                                        .Where(x => x.differences.Any(differece => differece.propertyName == "TaskNo"));

                foreach (var task in locationChangedTasks.Where(x => ((LocationCurrentTask)x.newObject).TaskNo != 0))
                {
                    OnTaskCurrentLocationChanged(((LocationCurrentTask)task.newObject));
                }

                var changedSignals = _compareResults
                        .Where(x => x.IsTypeOf(typeof(OccupiedSignal)))
                        .Where(x => ((OccupiedSignal)x.newObject).HandShake == OccupiedSignalHandShake.New);
                foreach (var signal in changedSignals)
                {
                    var signalObj = (OccupiedSignal)signal.newObject;
                    var location = this.Locations.SingleOrDefault(x => x.DeviceCode.Equals(signalObj.PosNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    if (location != null && location.AcceptRequestSignal)
                    {
                        OnOccpiedSignalStatusChanged((OccupiedSignal)signal.newObject);
                    }
                }
            }
            #endregion

            #region 如果是第一次加载数据
            if (isFirstLoad)
            {
                if (this.Tasks != null)
                {
                    foreach (var task in this.Tasks.Where(x => x.TaskStatus == TaskStatus.Error))
                    {
                        OnTaskError(task.AssignmentID, task.TaskStatus.ToString(), "任务不能执行");
                    }

                    foreach (var task in this.Tasks.Where(x => x.TaskStatus == TaskStatus.Finished))
                    {
                        OnTaskCompleted(task.AssignmentID);
                    }

                    foreach (var task in this.Tasks.Where(x => x.TaskStatus == TaskStatus.Running))
                    {
                        OnTaskRunning(task.AssignmentID);
                    }

                    foreach (var task in this.Tasks.Where(x => x.HandShake == HandShake.Readed))
                    {
                        OnNeedSecondHandShake(task);
                    }

                    foreach (var task in this.LocationCurrentTasks.Where(x => x.TaskNo != 0))
                    {
                        OnTaskCurrentLocationChanged(task);
                    }
                }

                if (this.OccupiedSignals != null)
                {
                    foreach (var signal in this.OccupiedSignals.Where(x => x.HandShake == OccupiedSignalHandShake.New))
                    {
                        var location = this.Locations.SingleOrDefault(x => x.DeviceCode.Equals(signal.PosNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                        if (location != null && location.AcceptRequestSignal)
                        {
                            OnOccpiedSignalStatusChanged(signal);
                        }
                    }
                }

                isFirstLoad = false;
            }
            #endregion
        }

        /// <summary>
        /// 向当前设备通迅缓冲区发送一个任务信息
        /// </summary>
        /// <param name="action">要发送给设备的物理动作</param>
        public override void SendTask(EquipmentAction action)
        {
            SendTaskBlock task = (SendTaskBlock)action.ToEquipmentTask();

            var fullMap = this.SenderDecoder.CreateFullMap();
            fullMap[typeof(SendTaskBlock)][0] = task;

            byte[] dataBytes = this.SenderDecoder.Encode(fullMap);
            NetPackage package = new ConveyorNetPackage();
            var datagram = package.CreateSendPackage(dataBytes);

            _tcpClient.GetStream().Write(datagram.GetBytes(), 0, datagram.GetBytes().Length);

            String msg = String.Format("给 {0} 发送了 {1}", this, action);
            this.Logger.Info(msg, this, action);
        }

        /// <summary>
        /// 向当前设备通迅缓冲区发送一个任务的二次握手信息
        /// </summary>
        /// <param name="action">需要二次握手的物理动作</param>
        public void SencodConfirm(EquipmentAction action)
        {
            SendTaskBlock task = (SendTaskBlock)action.ToEquipmentTask();
            task.HandShake = HandShake.SecondConfirm;

            var fullMap = this.SenderDecoder.CreateFullMap();
            fullMap[typeof(SendTaskBlock)][0] = task;

            byte[] dataBytes = this.SenderDecoder.Encode(fullMap);
            NetPackage package = new ConveyorNetPackage();
            //byte[] packageBytes = package.CreateSendPackage(dataBytes);
            var datagram = package.CreateSendPackage(dataBytes);
            _tcpClient.GetStream().Write(datagram.GetBytes(), 0, datagram.GetBytes().Length);

            String msg = String.Format("给 {0} 发送了二次握手信号", action);
            this.Logger.Info(msg, this, action);
        }

        /// <summary>
        /// 向当前设备通迅缓冲区发送一个清空指定任务块的请求
        /// </summary>
        /// <param name="action">需要清空任务块的设备动作对象</param>
        public void SetTaskBlockToEmpty(EquipmentAction action, Action<EquipmentAction, Boolean> callback)
        {
            SendTaskBlock task = (SendTaskBlock)action.ToEquipmentTask();
            task.HandShake = HandShake.ApplyForClear;

            String msg = String.Format("开始重置 {0} 的 {1} 占用的任务地址", this, action);
            this.Logger.Info(msg, this, action);

            SendStatusToDevice(task, (conveyor, tsk) =>
            {
                if (conveyor.Tasks == null) return false;

                var tasks = conveyor.Tasks.Where(x => x!=null && x.AssignmentID == tsk.AssignmentID);
                if (tasks.Count() > 1)
                {
                    conveyor.Logger.Warning(String.Format("{0} 在 {1} 找到了多个，请检查设备内任务是否正确", tsk.AssignmentID, conveyor), conveyor, tsk);
                    return false;
                }
                else
                {
                    return tasks.Count() == 0 || tasks.Single().TaskStatus == TaskStatus.Empty;
                }
            }, (tsk, success) =>
            {
                if (success)
                {
                    this.Logger.Info(String.Format("重置 {0} 的 {1} 占用的任务地址成功", this, tsk), this, tsk);
                }
                else
                {
                    this.Logger.Warning(String.Format("重置 {0} 的 {1} 占用的任务地址失败", this, tsk), this, tsk);
                }
                callback(action, success);
            });
        }

        /// <summary>
        /// 向当前设备通迅缓冲区发送一个清空指定任务块的请求.
        /// </summary>
        /// <exception cref="Exception">    如果调用该方法时当前任务未处于连接状态，将抛出异常. </exception>
        /// <param name="taskBlock">    要清空的任务块 </param>
        /// <returns></returns>
        public virtual Boolean SetTaskBlockToEmpty(TaskBlock taskBlock)
        {
            if (!this.IsConnected)
            {
                this.Logger.Warning(String.Format("{0} 未连接，无法清除 {1}", this, taskBlock), this, taskBlock);
                throw new Exception(String.Format("{0} 未连接，无法清除 {1}", this, taskBlock));
            }

            if (this.Tasks == null || this.Tasks.Length == 0)
            {
                string msg = String.Format("{0} 任务缓冲区未同步", this);
                this.Logger.Warning(msg, this);
                throw new Exception(msg);
            }

            var taskIndex = this.Tasks.ToList().FindIndex(x => x.AssignmentID == taskBlock.AssignmentID);
            if (taskIndex < 0)
            {
                string msg = String.Format("任务缓冲区未找到 {0}", taskBlock);
                this.Logger.Warning(msg, this, taskBlock);
                throw new Exception(msg);
            }
            taskIndex = taskIndex + 1;
            SendTaskBlock task = new SendTaskBlock
            {
                AssignmentID = taskBlock.AssignmentID,
                DestinationNo = taskBlock.DestinationNo,
                HandShake = HandShake.ApplyForClear,
                Index = Convert.ToUInt16(taskIndex),
                IO_Data = taskBlock.IO_Data,
                RotingNo = taskBlock.RotingNo,
                Spare = taskBlock.Spare,
                StartMotorNo = taskBlock.StartMotorNo,
                TU_ID = taskBlock.TU_ID,
                TU_Type = taskBlock.TU_Type
            };
            task.HandShake = HandShake.ApplyForDelete;

            this.Logger.Info(String.Format("尝试清除 {0} 上的 {1}", this, task), this, task);

            var result = false;
            SendStatusToDevice(task, (conveyor, tsk) =>
            {
                if (this.Tasks == null) return false;

                var tasks = this.Tasks.Where(x => x!=null && x.AssignmentID == tsk.AssignmentID);
                if (tasks.Count() > 1)
                {
                    this.Logger.Warning(String.Format("{0} 在 {1} 找到了多个，请检查设备内任务是否正确", tsk, this), this, tsk);
                    return false;
                }
                else
                {
                    return tasks.Count() == 0 || tasks.Single().TaskStatus == TaskStatus.Empty;
                }
            }, (tsk, success) =>
            {
                result = success;
                if (success)
                {
                    this.Logger.Info(String.Format("清除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
                else
                {
                    this.Logger.Warning(String.Format("清除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
            });

            return result;
        }

        TcpClient _tcpClient;
        ConveyorNetPackage _receivedNetPackage = new ConveyorNetPackage();
        /// <summary>
        /// 立即连接到设备
        /// </summary>
        /// <returns>成功后返回 true，失败 返回 false</returns>
        public override Boolean Connect()
        {
            try
            {
                lock (this)
                {
                    if (_tcpClient != null)
                    {
                        String msg = String.Format("{0} 已连接，请勿重复操作", this);

                        Debug.WriteLine(msg);

                        this.Logger.Warning(msg, this);

                        return true;
                    }

                    this.Logger.Info(String.Format("{0} 开始尝试连接到 {1}:{2}...", this, Ip, Port), this);

                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(Ip, Port, this.ConnectTimeout);
                    _receivedNetPackage.Clear();

                    OnConnected();
                    this.ConnectionRetries = 0;

                    this.Logger.Info(String.Format("{0} 连接成功", this), this);

                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        Receive();
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.ConnectionRetries += 1;
                String msg = String.Format("{0} 连接失败，原因：{1}", this, ex.Message, ex);
                this.Logger.Warning(msg, this);

                _tcpClient = null;
                _receivedNetPackage.Clear();

                OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.Error));
                return false;
            }
        }
        /// <summary>
        /// 连接后接收从设备发送过来的缓冲区数据
        /// </summary>
        private void Receive()
        {
            try
            {
                byte[] readBuffer = new byte[1024];
                int numberOfBytesRead;
                NetworkStream networkStream = _tcpClient.GetStream();
                _tcpClient.ReceiveTimeout = this.ReceiveTimeout;
                while (_tcpClient != null)
                {
                    numberOfBytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);

                    if (numberOfBytesRead <= 0)
                    {
                        throw new Exception(String.Format("{0} 已断开连接", this));
                    }
                    _receivedNetPackage.AddBytes(readBuffer.Take(numberOfBytesRead));

                    UpdateStatus(_receivedNetPackage);
                }
            }
            catch (Exception ex)
            {
                //如果_tcpClient为null，说明是强制断开的连接，无需再处理错误
                if (_tcpClient != null)
                {
                    _receivedNetPackage.Clear();
                    String msg = String.Format("{0} 在接收数据时发生错误，原因：{1}", this, ex.Message, ex);
                    Debug.WriteLine(msg);

                    this.Logger.Error(msg, this, ex);
                    OnDeviceError("OnReceive", ex.ToString());
                    if (_tcpClient != null)
                    {
                        if (_tcpClient.Client != null)
                        {
                            _tcpClient.Client.Close();
                        }
                        _tcpClient.Close();
                    }

                    _tcpClient = null;

                    OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.Error));

                }
            }
        }
        /// <summary>
        /// 断开与设备的连接.
        /// </summary>
        public override void Disconnect()
        {
            if (_tcpClient != null)
            {
                if (_tcpClient.Client != null)
                {
                    _tcpClient.Client.Close();
                }
                _tcpClient.Close();
            }

            _tcpClient = null;

            this.Logger.Warning(String.Format("{0} 被强制断开连接", this), this);

            OnDisconnected(new DeviceDisconnectEventArgs(DeviceDisconnectReason.User));
        }

        /// <summary>
        /// 引发 <see cref="T:Wcs.Framework.Devices.Conveyor"/> 的 <see cref="E:Wcs.Framework.Devices.Conveyor.NeedSecondHandShake"/> 事件.
        /// </summary>
        /// <param name="taskBlock">任务块</param>
        protected void OnNeedSecondHandShake(TaskBlock taskBlock)
        {
            Location startLocation = Location.TryParse(taskBlock.StartMotorNo.ToString(), null, this);
            Location endLocation = Location.TryParse(taskBlock.DestinationNo.ToString(), null, this);

            if (startLocation == null)
            {
                String msg = String.Format("{0} 的 {1} 收到了二次握手信号，但未找到位置 {2} 的本地定义，本次信号被忽略", this, taskBlock, taskBlock.StartMotorNo);
                this.Logger.Warning(msg, this, taskBlock);
                return;
            }

            if (endLocation == null)
            {
                String msg = String.Format("{0} 的 {1} 收到了二次握手信号，但未找到位置 {2} 的本地定义，本次信号被忽略", this, taskBlock, taskBlock.DestinationNo);
                this.Logger.Warning(msg, this, taskBlock);
                return;
            }

            this.Logger.Info(String.Format("{0} 的 {1} 收到了二次握手信号", this, taskBlock), this, taskBlock);

            if (NeedSecondHandShake != null)
            {
                Boolean handled = false;
                try
                {
                    NeedSecondHandShake.Invoke(this, taskBlock, ref handled);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message, this, ex);
                    this.Logger.Warning(string.Format("在处理 {0} 的二次握手信号时发生错误:\r\n{1}", taskBlock, ex.Message), this, taskBlock);
                }
                finally
                {
                    if (!handled)
                    {
                        this.DelayedDeviceStatusChangedNotify.Add("NeedSecondHandShake", null, taskBlock);
                    }
                    else
                    {
                        this.DelayedDeviceStatusChangedNotify.Remove("NeedSecondHandShake", null, taskBlock);
                    }
                }
            }
        }

        /// <summary>
        /// 引发 <see cref="T:Wcs.Framework.Devices.Conveyor"/> 的 <see cref="E:Wcs.Framework.Devices.Conveyor.TaskCurrentLocationChanged"/> 事件.
        /// </summary>
        /// <param name="locationCurrentTask">货位当前任务</param>
        protected void OnTaskCurrentLocationChanged(LocationCurrentTask locationCurrentTask)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                LocationCurrentTask locationCurrentTaskState = (LocationCurrentTask)state;
                Location currentLocation = Location.TryParse(locationCurrentTaskState.PosNo.ToString(), null, this);
                if (currentLocation == null)
                {
                    String msg = String.Format("{0} 的 {1} 位置发生了改变，但未找到位置 {2} 的本地定义，本次信号被忽略", this, locationCurrentTaskState, locationCurrentTaskState.PosNo);
                    this.Logger.Warning(msg, this, locationCurrentTaskState);
                    return;
                }

                this.Logger.Info(String.Format("{0} 的 {1} 位置发生了改变，当前处于 {2}", this, locationCurrentTaskState, currentLocation), this, locationCurrentTaskState);

                if (TaskCurrentLocationChanged != null)
                {
                    Boolean handled = false;
                    try
                    {
                        TaskCurrentLocationChanged.Invoke(this, locationCurrentTaskState, (ConveyorLocation)currentLocation, ref handled);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex.Message, this, ex);
                        this.Logger.Warning(string.Format("在处理 {0} 时发生错误:\r\n{1}", locationCurrentTaskState, ex.Message), this, locationCurrentTaskState);
                    }
                    finally
                    {
                        if (!handled)
                        {
                            this.DelayedDeviceStatusChangedNotify.Add("TaskCurrentLocationChanged", null, locationCurrentTaskState);
                        }
                        else
                        {
                            this.DelayedDeviceStatusChangedNotify.Remove("TaskCurrentLocationChanged", null, locationCurrentTaskState);
                        }
                    }
                }
            }, locationCurrentTask);
        }
        /// <summary>
        /// 引发 <see cref="T:Wcs.Framework.Devices.Conveyor"/> 的 <see cref="E:Wcs.Framework.Devices.Conveyor.OccpiedSignalStatusChanged"/> 事件.
        /// </summary>
        /// <param name="signal">占位信号</param>
        protected void OnOccpiedSignalStatusChanged(OccupiedSignal signal)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                OccupiedSignal signalState = (OccupiedSignal)state;
                Location location = Location.TryParse(signalState.PosNo.ToString(), null, this);
                if (location == null)
                {
                    String msg = String.Format("{0} 的 {1} 状态发生了改变，但未找到位置 {2} 的本地定义，本次信号被忽略", this, signalState, signalState.PosNo);
                    this.Logger.Warning(msg, this, signalState);
                    return;
                }

                this.Logger.Info(String.Format("{0} 的 {1} 状态发生了改变，当前处于 {2}", this, signalState, signalState.HandShake), this, signalState);
                if (OccpiedSignalStatusChanged != null)
                {
                    Boolean handled = false;
                    try
                    {
                        OccpiedSignalStatusChanged.Invoke(this, location, signalState, ref handled);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex.Message, this, ex);
                        this.Logger.Warning(string.Format("在处理 {0} 时发生错误:\r\n{1}", signalState, ex.Message), this, signalState);
                    }
                    finally
                    {
                        if (!handled)
                        {
                            this.DelayedDeviceStatusChangedNotify.Add("OccpiedSignalStatusChanged", null, signalState);
                        }
                        else
                        {
                            this.DelayedDeviceStatusChangedNotify.Remove("OccpiedSignalStatusChanged", null, signalState);
                        }
                    }
                }
            }, signal);
        }
        /// <summary>
        /// 指示当前设备是否已连机.
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Connected;
            }
        }
        /// <summary>
        /// 取消一个设备正在执行的任务.
        /// </summary>
        /// <param name="action">       要取消的物理动作. </param>
        public override void CancelTask(EquipmentAction action)
        {
            CancelTask(action, null);
        }
        /// <summary>
        /// 取消一个设备正在执行的任务.
        /// </summary>
        /// <param name="action">   要取消的物理动作. </param>
        /// <param name="callback"> 方法执行完成后的回调方法. </param>
        public virtual void CancelTask(EquipmentAction action, Action<EquipmentAction, Boolean> callback)
        {
            if (!this.IsConnected)
            {
                this.Logger.Warning(String.Format("{0} 未连接，无法取消 {1}", this, action), this, action);
                return;
            }

            SendTaskBlock task = (SendTaskBlock)action.ToEquipmentTask();
            task.HandShake = HandShake.ApplyForDelete;


            SendStatusToDevice(task, (conveyor, tsk) =>
            {
                if (conveyor.Tasks == null) return false;

                var tasks = conveyor.Tasks.Where(x => x.AssignmentID == tsk.AssignmentID);
                if (tasks.Count() > 1)
                {
                    this.Logger.Warning(String.Format("{0} 在 {1} 找到了多个，请检查设备内任务是否正确", tsk, this), this, tsk);
                    return false;
                }
                else
                {
                    return tasks.Count() == 0 || tasks.Single().TaskStatus == TaskStatus.Empty;
                }
            }, (tsk, success) =>
            {
                if (success)
                {
                    this.Logger.Info(String.Format("删除 {0} 的 {1} 占用的任务地址成功", this, action), this, action);
                }
                else
                {
                    this.Logger.Warning(String.Format("删除 {0} 的 {1} 占用的任务地址失败", this, action), this, action);
                }

                if (callback != null)
                {
                    callback(action, success);
                }
            });
        }
        /// <summary>
        /// 取消一个设备正在执行的任务.
        /// </summary>
        /// <exception cref="Exception">如果尝试在设备未连接时调用此方法将引发异常. </exception>
        /// <param name="taskBlock">    要清空的任务块. </param>
        public virtual Boolean CancelTask(TaskBlock taskBlock)
        {
            if (!this.IsConnected)
            {
                this.Logger.Warning(String.Format("{0} 未连接，无法取消 {1}", this, taskBlock), this, taskBlock);
                throw new Exception(String.Format("{0} 未连接，无法取消 {1}", this, taskBlock));
            }

            if (this.Tasks == null || this.Tasks.Length == 0)
            {
                string msg = String.Format("{0} 任务缓冲区未同步", this);
                this.Logger.Warning(msg, this);
                throw new Exception(msg);
            }

            var taskIndex = this.Tasks.ToList().FindIndex(x => x.AssignmentID == taskBlock.AssignmentID);
            if (taskIndex < 0)
            {
                string msg = String.Format("任务缓冲区未找到 {0}", taskBlock);
                this.Logger.Warning(msg, this, taskBlock);
                throw new Exception(msg);
            }
            taskIndex = taskIndex + 1;
            SendTaskBlock task = new SendTaskBlock
            {
                AssignmentID = taskBlock.AssignmentID,
                DestinationNo = taskBlock.DestinationNo,
                HandShake = HandShake.ApplyForDelete,
                Index = Convert.ToUInt16(taskIndex),
                IO_Data = taskBlock.IO_Data,
                RotingNo = taskBlock.RotingNo,
                Spare = taskBlock.Spare,
                StartMotorNo = taskBlock.StartMotorNo,
                TU_ID = taskBlock.TU_ID,
                TU_Type = taskBlock.TU_Type
            };
            task.HandShake = HandShake.ApplyForDelete;


            this.Logger.Info(String.Format("尝试取消 {0} 上的 {1}", this, task), this, task);

            var result = false;
            SendStatusToDevice(task, (conveyor, tsk) =>
            {
                if (this.Tasks == null) return false;

                var tasks = this.Tasks.Where(x => x.AssignmentID == tsk.AssignmentID);
                if (tasks.Count() > 1)
                {
                    this.Logger.Warning(String.Format("{0} 在 {1} 找到了多个，请检查设备内任务是否正确", tsk, this), this, tsk);
                    return false;
                }
                else
                {
                    return tasks.Count() == 0 || tasks.Single().TaskStatus == TaskStatus.Empty;
                }
            }, (tsk, success) =>
            {
                result = success;
                if (success)
                {
                    this.Logger.Info(String.Format("删除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
                else
                {
                    this.Logger.Warning(String.Format("删除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
            });

            return result;
        }

        /// <summary>
        /// 设置占位信号握手状态
        /// </summary>
        /// <param name="location">位置</param>
        /// <param name="handShake">握手变量</param>
        /// <returns></returns>
        public virtual Boolean SetOccupiedSignalHandshake(Location location, OccupiedSignalHandShake handShake)
        {
            if (!this.IsConnected)
            {
                string msg = String.Format("{0} 未连接，无法设置占位信息", this);
                this.Logger.Warning(msg, this);
                throw new Exception(msg);
            }

            int index = this.OccupiedSignals.ToList().FindIndex(x => x.PosNo == int.Parse(location.DeviceCode));
            if (index < 0)
            {
                string msg = String.Format("{0} 在占位信号块内未定义", location);
                this.Logger.Warning(msg, this);
                throw new Exception(msg);
            }

            //plc的索引下标从 1 开始，所以这里加1
            index = index + 1;

            SendOccupiedSignal sendOccupiedSignal = new SendOccupiedSignal
            {
                HandShake = handShake,
                PosNo = UInt16.Parse(location.DeviceCode),
                Index = Convert.ToUInt16(index)
            };

            this.Logger.Info(String.Format("尝试设置 {0} 上的占位信号 {1}", this, sendOccupiedSignal), this, sendOccupiedSignal);

            var result = false;
            SendStatusToDevice(sendOccupiedSignal, (conveyor, status) =>
            {
                if (this.OccupiedSignals == null) return false;

                OccupiedSignalHandShake targetHandShake = status.HandShake;
                if (status.HandShake == OccupiedSignalHandShake.ApplyToDelete)
                {
                    targetHandShake = OccupiedSignalHandShake.Empty;
                }

                return this.OccupiedSignals.Any(x => x.PosNo == status.PosNo && x.HandShake == targetHandShake);
            }, (status, success) =>
            {
                result = success;
                if (success)
                {
                    this.Logger.Info(String.Format("设备 {0} 的 {1} 上的占位信号成功", this, status), this, status);

#warning 此处代码以后需要删除，此步骤多余
                    var 帮电气删东西 = (SendOccupiedSignal)status;
                    帮电气删东西.HandShake = OccupiedSignalHandShake.Empty;
                    SendStatusToDevice(帮电气删东西, null, null);
                }
                else
                {
                    this.Logger.Warning(String.Format("重置 {0} 的 {1} 上的占位信号失败", this, status), this, status);
                }
            });

            return result;
        }
        /// <summary>
        /// 判断一个物理动作是否已成功从设备任务列表中取消.
        /// </summary>
        /// <param name="action">   被取消的物理动作. </param>
        public override bool TaskCancelSuccessfully(EquipmentAction action)
        {
            if (this.Tasks == null) return false;

            var tasks = this.Tasks.Where(x => x.AssignmentID == action.EquipmentTaskId);
            if (tasks.Count() > 1)
            {
                this.Logger.Warning(String.Format("{0} 在 {1} 找到了多个，请检查设备内任务是否正确", action, this), this, action);
                return false;
            }
            else
            {
                return tasks.Count() == 0 || tasks.Single().TaskStatus == TaskStatus.Empty;
            }
        }
        /// <summary>
        /// 判断一个物理动作是否已成功发送给了设备.
        /// </summary>
        /// <param name="action">   被发送的物理动作. </param>
        /// <returns></returns>
        public override bool TaskSendSuccessfully(EquipmentAction action)
        {
            if (this.Tasks == null) return false;

            return this.Tasks.Any(x => x.AssignmentID == action.EquipmentTaskId);
        }

        /// <summary>
        /// 向设备通信缓存区发送一个清除货位当前任务的指令
        /// </summary>
        /// <exception cref="Exception">    如果在设备未连接时调用此方法将抛出异常. </exception>
        /// <param name="currentTask">  要申请清除的货位任务. </param>
        /// <returns>
        /// 成功返回 true,失败返回 false
        /// </returns>
        public virtual Boolean ClearLocationCurrentTask(LocationCurrentTask currentTask)
        {
            if (!this.IsConnected)
            {
                this.Logger.Warning(String.Format("{0} 未连接，无法取消 {1}", this, currentTask), this, currentTask);
                throw new Exception(String.Format("{0} 未连接，无法取消 {1}", this, currentTask));
            }

            if (this.LocationCurrentTasks == null || this.LocationCurrentTasks.Length == 0)
            {
                string msg = String.Format("{0} 货位当前任务未同步", this);
                this.Logger.Warning(msg, this);
                throw new Exception(msg);
            }

            var taskIndex = this.LocationCurrentTasks.ToList().FindIndex(x => x.TaskNo == currentTask.TaskNo && x.PosNo == currentTask.PosNo);
            if (taskIndex < 0)
            {
                string msg = String.Format("货位当前任务缓冲区未找到 {0}", currentTask);
                this.Logger.Warning(msg, this, currentTask);
                throw new Exception(msg);
            }
            taskIndex = taskIndex + 1;
            SendClearLocationCurrentTask task = new SendClearLocationCurrentTask
            {
                Index = Convert.ToUInt16(taskIndex),
                PosNo = currentTask.PosNo,
                TaskNo = currentTask.TaskNo,
                TUID = currentTask.TUID
            };


            this.Logger.Info(String.Format("尝试取消 {0} 上的 {1}", this, task), this, task);

            var result = false;

            SendStatusToDevice(task, (conveyor, tsk) =>
            {
                if (conveyor.LocationCurrentTasks == null) return false;

                return !conveyor.LocationCurrentTasks.Any(x => x.PosNo == tsk.PosNo && x.TaskNo == tsk.TaskNo);
            }, (tsk, success) =>
            {
                result = success;
                if (success)
                {
                    this.Logger.Info(String.Format("删除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
                else
                {
                    this.Logger.Warning(String.Format("删除 {0} 的 {1} 成功", this, tsk), this, tsk);
                }
            });

            return result;
        }
        /// <summary>
        /// 当前设备的警告信息.
        /// </summary>
        /// <value>
        /// 警告信息的文本表现形式.
        /// </value>
        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                if (this.Tasks == null)
                {
                    result.Add("任务缓冲块未同步");
                }

                if (LocationCurrentTasks == null)
                {
                    result.Add("货位当前任务块未同步");
                }

                if (MachineAlarms == null)
                {
                    result.Add("报警信息块未同步");
                }

                if (OccupiedSignals == null)
                {
                    result.Add("占位信号块未同步");
                }
                if (OccupyStatus == null)
                {
                    result.Add("光电状态块未同步");
                }
                if (ConveyorLocationStates == null)
                {
                    result.Add("货位状态块未同步");
                }

                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    result.Add(string.Format("设备被 {0} 远程锁定", this.Locker));
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// 向设备通迅缓存冲发送一个状态信息
        /// </summary>
        /// <typeparam name="TState">  状态信息类型. </typeparam>
        /// <param name="state">            状态信息. </param>
        /// <param name="isSuccessfully">   发送是否成功的判断方法. </param>
        /// <param name="callback">         方法执行完成后的回调方法. </param>
        public virtual void SendStatusToDevice<TState>(TState state, Func<NewConveyor, TState, Boolean> isSuccessfully, Action<TState, Boolean> callback) where TState : NetTransferObject
        {
            var fullMap = this.SenderDecoder.CreateFullMap();
            fullMap[typeof(TState)][0] = state;

            SendStatusToDevice(fullMap, state, isSuccessfully, callback);
        }

        /// <summary>
        /// 向设备通迅缓存冲发送一个状态信息
        /// </summary>
        /// <typeparam name="TState">  状态信息类型. </typeparam>
        /// <param name="state">            状态信息. </param>
        /// <param name="isSuccessfully">   发送是否成功的判断方法. 如果为 null，将始终标示为发送成功</param>
        /// <param name="callback">         方法执行完成后的回调方法. </param>
        /// <remarks>
        /// state 为 null，但 callback 不为 null 时，或 state 为 null，但 isSuccessfully 不为 null 时 将抛出异常
        /// </remarks>
        public virtual void SendStatusToDevice<TState>(Dictionary<Type, NetTransferObject[]> fullMap, TState state, Func<NewConveyor, TState, Boolean> isSuccessfully, Action<TState, Boolean> callback) where TState : NetTransferObject
        {
            if (state == null && callback != null)
            {
                throw new ArgumentException("state 为 null，但 callback 不为 null");
            }

            if (state == null && isSuccessfully != null)
            {
                throw new ArgumentException("state 为 null，但 isSuccessfully 不为 null");
            }

            if (!this.IsConnected)
            {
                this.Logger.Warning(String.Format("{0} 未连接，无法发送状态数据 {0} 到设备", this, state), this, state);
                if (callback != null)
                {
                    callback(state, false);
                }
                return;
            }

            byte[] dataBytes = this.SenderDecoder.Encode(fullMap);
            NetPackage package = new ConveyorNetPackage();
            //byte[] packageBytes = package.CreateSendPackage(dataBytes);

            //_tcpClient.GetStream().Write(packageBytes, 0, packageBytes.Length);
            var datagram = package.CreateSendPackage(dataBytes);

            _tcpClient.GetStream().Write(datagram.GetBytes(), 0, datagram.GetBytes().Length);

            String msg = String.Format("尝试向 {0} 发送状态数据 {1}", this, state);
            this.Logger.Info(msg, this, state);

            bool success;
            if (isSuccessfully != null)
            {
                DateTime start = DateTime.Now;
                while (DateTime.Now.Subtract(start).TotalMilliseconds < this.SendTimeout && !isSuccessfully(this, state))
                {
                    System.Threading.Thread.Sleep(10);
                }

                success = isSuccessfully(this, state as TState);

                if (success)
                {
                    this.Logger.Info(String.Format("向 {0} 发送状态数据 {1} 成功", this, state), this, state);
                }
                else
                {
                    this.Logger.Warning(String.Format("向 {0} 发送状态数据 {1} 成功", this, state), this, state);
                }
            }
            else
            {
                success = true;
            }

            if (callback != null)
            {
                callback(state, success);
            }
        }

        /// <summary>
        /// 获取设备类型.
        /// </summary>
        /// <value>
        /// DeviceType.Conveyor
        /// </value>
        public override DeviceType DeviceType
        {
            get { return Devices.DeviceType.Conveyor; }
        }
    }
}
