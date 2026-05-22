using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个可以执行任务的设备
    /// </summary>
    public abstract class TaskableDevice : Device
    {
        #region Properties
        /// <summary>
        /// 是否允许并发任务（同时接受多个任务）
        /// </summary>
        public Boolean AllowConcurrency { get; protected set; }
        /// <summary>
        /// 任务发送超时时间
        /// </summary>
        public Int32 SendTimeout { get; protected set; }
        /// <summary>
        /// 物理动作序列
        /// </summary>
#warning 要考虑先有鸡还是先有蛋的问题
        public EquipmentActionScheduler EquipmentActionScheduler { get; set; }
        /// <summary>
        /// 设备路径转换为逻辑动作时使用的逻辑动作类型选择器
        /// </summary>
        public LogicMovementSelector LogicMovementSelector { get; set; }

        /// <summary>
        /// 对应的动作逻辑动作类型
        /// </summary>
        public Type[] LogicMovementTypes { get; set; }
        /// <summary>
        /// 获取被占用的设备任务号集合
        /// </summary>
        public abstract Int32[] OccupiedEquipmentTasks { get; }
        #endregion

        #region Events

        /// <summary>
        /// 当设备接收的任务开始运行时发生。
        /// </summary>
        public event DeviceEventHandler<TaskableDevice, TaskRunningEventArgs> TaskRunning;

        /// <summary>
        /// 当设备接收的任务发生错误时发生。
        /// </summary>
        public event DeviceEventHandler<TaskableDevice, TaskErrorEventArgs> TaskError;

        /// <summary>
        /// 当设备接收的任务完成时发生
        /// </summary>
        public event DeviceEventHandler<TaskableDevice, TaskCompletedEventArgs> TaskCompleted;

        /// <summary>
        /// 当设备接收的任务请求时发生
        /// </summary>
        public event DeviceEventHandler<TaskableDevice, TaskRequestEventArgs> TaskRequest;

        /// <summary>
        /// 当设备接收的任务当前位置改变时发生
        /// </summary>
        public event DeviceEventHandler<TaskableDevice, TaskCurrentLocationChangedEventArgs> TaskCurrentLocationChanged;
        #endregion

        #region Abstract Methods
        /// <summary>
        /// 向设备发送一个任务,失败时抛出异常
        /// </summary>
        /// <param name="action">要发送给设备的物理动作</param>
        /// <param name="args">发送给设备物理动作时所带参数</param>
        /// <remarks>失败时抛出异常</remarks>
        public abstract void SendTask(EquipmentAction action, params string[] args);

        /// <summary>
        /// 向设备发送一个任务之前,比如说需要做锁定等操作
        /// </summary>
        /// <param name="action">要发送给设备的物理动作</param>
        /// <param name="args">发送给设备物理动作时所带参数</param>
        /// <remarks>失败时抛出异常</remarks>
        public abstract void SendTaskPre(EquipmentAction action);

        /// <summary>
        /// 取消一个设备正在执行的任务
        /// </summary>
        /// <param name="action">要取消的物理动作</param>
        /// <remarks>失败时抛出异常</remarks>
        public abstract void CancelTask(EquipmentAction action);

        /// <summary>
        /// 从设备中读取指定类型的状态数据
        /// </summary>
        /// <typeparam name="TState">状态数据类型</typeparam>
        /// <returns>设备的状态数据</returns>
        public abstract TState Read<TState>() where TState : NetTransferObject, new();

        /// <summary>
        /// 向设备中同步写入数据
        /// </summary>
        /// <typeparam name="TData">泛型参数，要写入的数据类型</typeparam>
        /// <param name="data">要写入的具体数据</param>
        /// <param name="isSuccess">是否成功的判定表示式</param>
        /// <remarks>如果失败，则抛出异常。</remarks>
        public abstract void Write<TCommand>(TCommand data, Func<TaskableDevice, TCommand, Boolean> isSuccess) where TCommand : DeviceCommand;
        #endregion

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="name">设备名称</param>
        /// <param name="ipEndPoint">表示网络端点的IP 地址和端口号</param>
        /// <param name="receiveTimeout">接收超时时间（毫秒）</param>
        /// <param name="connectTimeout">连接超时时间（毫秒）</param>
        /// <param name="sendTimeout">发送超时时间（毫秒）</param>
        /// <param name="allowConcurrency">是否允许并行任务</param>
        /// <param name="dataReceiver">数据接收器</param>
        public TaskableDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, Boolean allowConcurrency)
            : base(name, no, receiveTimeout, connectTimeout)
        {
            _locations = new List<Location>();
            if (sendTimeout <= 0)
            {
                throw new ArgumentOutOfRangeException("receiveTimeout", "必须大于 0");
            }
            this.SendTimeout = sendTimeout;
            this.AllowConcurrency = allowConcurrency;
        }

        #region Protected Methods
        protected virtual void FireTaskRunningEvent(TaskRunningEventArgs args)
        {
            this.DeviceEventQueue.Enqueue(this.TaskRunning, this, args);
        }

        protected virtual void FireTaskErrorEvent(TaskErrorEventArgs args)
        {
            this.DeviceEventQueue.Enqueue(this.TaskError, this, args);
        }

        protected virtual void FireTaskCompletedEvent(TaskCompletedEventArgs args)
        {
            //if (AbstractStateManager.Contexts.Any(x => x.EquipmentAction.EquipmentTaskId == args.EquipmentTaskId))
            //{
            //    _logger.Warn1(string.Format("{0} 已被状态机接管，不再引发此次完成事件。", args.EquipmentTaskId), this, null, args.EquipmentTaskId);
            //}
            //else
            //{
            //    this.DeviceEventQueue.Enqueue(this.TaskCompleted, this, args);
            //}

            //this.DeviceEventQueue.Enqueue(this.TaskCompleted, this, args);
            var _arg = new TaskCompletedEventHandlerArgs() { Device = this, Args = args };
            if (!TaskCompletedEventHandler.ExistArchiveProcTaskList(_arg))
                TaskCompletedEventHandler.PushCompeltedTaskList(_arg);
        }

        protected virtual void FireTaskRequestEvent(TaskRequestEventArgs args)
        {
            this.DeviceEventQueue.Enqueue(this.TaskRequest, this, args);
        }

        protected virtual void FireTaskCurrentLocationChangedEvent(TaskCurrentLocationChangedEventArgs args)
        {
            this.DeviceEventQueue.Enqueue(this.TaskCurrentLocationChanged, this, args);
        }
        #endregion

        public Location[] Locations
        {
            get
            {
                return _locations.ToArray();
            }
        }
        protected List<Location> _locations;
    }
}
