using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

namespace Wcs.Framework.Devices
{
    public class NewConveyor:TaskableDevice
    {
        TcpClient _tcpClient;

        #region Properties
        /// <summary>
        /// 作为接收方使用的数据包解码器
        /// </summary>
        public DefaultConveyorNetPackageDecoder ReceiverDecoder { get; set; }
        /// <summary>
        /// 作为发送方使用的数据包解码器
        /// </summary>
        public DefaultConveyorNetPackageDecoder SenderDecoder { get; set; }

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
        #endregion

        #region Events
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
        #endregion

        #region 基类实现

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
            throw new NotImplementedException();
        }

        public override void Write<TData>(TData data, Func<TaskableDevice, TData, bool> isSuccess)
        {
            throw new NotImplementedException();
        }

        public override DeviceType DeviceType
        {
            get 
            { 
                return Devices.DeviceType.Conveyor; 
            }
        }

        public override bool IsConnected
        {
            get
            {
                return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Connected;
            }
        }

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

        public override bool Connect()
        {
            try
            {
                lock (this)
                {
                    if (IsConnected)
                    {
                        return true;
                    }

                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(this.Ip, Port, this.ConnectTimeout);
                    FireConnectedEvent();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _tcpClient = null;
                FireDisconnectedEvent(new DeviceDisconnectEventArgs(DeviceDisconnectReason.Error,ex));
                return false;
            }
        }

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

            FireDisconnectedEvent(new DeviceDisconnectEventArgs(DeviceDisconnectReason.User));
        }
        #endregion
    }
}
