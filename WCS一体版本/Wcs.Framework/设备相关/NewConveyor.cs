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
    public class NewConveyor:TaskableDevice
    {
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

        public override bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsIdle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string[] Warnings
        {
            get
            {
                throw new NotImplementedException();
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override bool Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
