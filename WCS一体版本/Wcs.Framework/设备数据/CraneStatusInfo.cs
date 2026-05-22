using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using System.Reflection;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机状态数据类型
    /// </summary>
    public class CraneStatusInfo:Comparable<CraneStatusInfo>
    {
        internal CraneControl.LA _CraneProxyStatus { get; private set; }
        public CraneStatusInfo(CraneControl.LA craneLastStatus)
        {
            _CraneProxyStatus = craneLastStatus;

            this.AtStation = craneLastStatus.AtPosition;
#warning 此处为了测试直接给 Connected 属性赋值，正式使用时用第二行代码
            this.Connected = true;
            //this.Connected = craneLastStatus.Connected;
            this.CraneForkHorizontalLocation = (CraneForkHorizontalLocation)craneLastStatus.ForkLR;
            this.CraneForkVerticalLocation = (CraneForkVerticalLocation)craneLastStatus.ForkTL;
            this.CraneWorkModeKeyInfo = craneLastStatus.CraneWorkModeKeyInfo;
            this.CurrentTaskId = craneLastStatus.TaskId;
            this.ErrorCode = craneLastStatus.ErrorCode;
            this.ErrorDescription = craneLastStatus.ErrorInfo;
            this.Event = (CraneEvent)craneLastStatus.Event;
            this.LockId = craneLastStatus.LockGuid;
            this.LockUser = craneLastStatus.LockUser;
            this.Status = convertToStatus((int)craneLastStatus.State);

            CraneCurrentLocation currentLocation = new CraneCurrentLocation();
            currentLocation.ColumnUserCode = craneLastStatus.Position.UCol;
            currentLocation.LevelUserCode = craneLastStatus.Position.URow;
            if (craneLastStatus.Position.UCol.Contains("?"))
            {
                currentLocation.ColumnDeviceCode = null;
            }
            else
            {
                currentLocation.ColumnDeviceCode = craneLastStatus.Position.MCol;
            }

            if (craneLastStatus.Position.URow.Contains("?"))
            {
                currentLocation.LevelDeviceCode = null;
            }
            else
            {
                currentLocation.LevelDeviceCode = craneLastStatus.Position.MRow;
            }

            this.CurrentPosition = currentLocation;
        }

        private CraneStatus convertToStatus(Int32 state)
        {
            switch (state)
	        {
                case 0://初始化
                    return  CraneStatus.Initialized;
                case 1://回原点
                    return CraneStatus.MovingToOrigin;
                case 2://无货待命
                case 3://有货待命
                    return  CraneStatus.Waiting;
                    
                case 4://无货运行
                case 5://有货运行
                    return  CraneStatus.Running;

                case 6://取货
                    return CraneStatus.Loading;

                case 7://放货
                    return CraneStatus.Unloading;
                    
                case 8://报警停机
                case 9://报警复位
                    return CraneStatus.Error;

                case 10://回原点
                    return CraneStatus.MovingToOrigin;

                case 11://未连接
#warning 此处应该处理或询问是否会有数据返回
                    return CraneStatus.Manual;

                case 12://手动操作
                    return CraneStatus.Manual;
		        default:
                    throw new NotSupportedException(String.Format("状态值 {0}",state));
	        }
        }
        /// <summary>
        /// 是否已连接
        /// </summary>
        [System.ComponentModel.DisplayName("已连接")]
        public bool Connected { get; set; }
        
        /// <summary>
        /// 手动操作终端锁 GUID
        /// </summary>
        [System.ComponentModel.DisplayName("锁")]
        public string LockId { get; set; }

        /// <summary>
        /// 手动操作终端锁的客户机
        /// </summary>
        [System.ComponentModel.DisplayName("锁用户")]
        public string LockUser { get; set; }

       /// <summary>
        /// 堆垛机状态
        /// </summary>
        [System.ComponentModel.DisplayName("状态")]
        public CraneStatus Status { get; private set; }

        /// <summary>
        /// 堆垛机事件
        /// </summary>
        [System.ComponentModel.DisplayName("事件")]
        public CraneEvent Event { get; private set; }

        /// <summary>
        /// 货叉左右位
        /// </summary>
        [System.ComponentModel.DisplayName("货叉左右位置")]
        public CraneForkHorizontalLocation CraneForkHorizontalLocation { get; private set; }

        /// <summary>
        /// 货叉高低位
        /// </summary>
        [System.ComponentModel.DisplayName("货叉高低位置")]
        public CraneForkVerticalLocation CraneForkVerticalLocation { get; private set; }
                
        /// <summary>
        /// 堆垛机当前所在货位
        /// </summary>
        [System.ComponentModel.DisplayName("当前位置")]
        public CraneCurrentLocation CurrentPosition { get; private set; }

        /// <summary>
        /// 是否在站点位置
        /// </summary>
        [System.ComponentModel.DisplayName("是否在站点")]
        public bool AtStation { get; private set; }

       /// <summary>
        /// 错误码
        /// </summary>
        [System.ComponentModel.DisplayName("错误码")]
        public string ErrorCode { get; private set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        [System.ComponentModel.DisplayName("错误描述")]
        public string ErrorDescription { get; private set; }

        /// <summary>
        /// 任务号
        /// </summary>
        [System.ComponentModel.DisplayName("任务号")]
        public string CurrentTaskId { get; private set; }

        /// <summary>
        /// 指示堆垛机上是否有货物
        /// </summary>
        [System.ComponentModel.DisplayName("是否有货")]
        public bool Loaded { get; set; }

        /// <summary>
        /// 指示堆垛机是否繁忙
        /// <para>繁忙状态的相关成立条件（任意一个）：</para>
        /// <para>1、堆垛机未连接</para>
        /// <para>2、已切换至手动模式</para>
        /// <para>3、被其它用户锁定</para>
        /// <para>4、错误码不为 String.Empty 并且不为 0000</para>
        /// <para>5、事件不等于 完成、出错完成、已初始化</para>
        /// </summary>
        [System.ComponentModel.DisplayName("是否繁忙")]
        public bool IsBusy
        {
            get
            {
                return !(
                    (this.Event == CraneEvent.Initialized || this.Event == CraneEvent.Completed || this.Event == CraneEvent.CompletedWithError)
                    && (string.IsNullOrEmpty(this.ErrorCode) || this.ErrorCode == "0000")
                    && this.Connected
                    && !this.IsManual
                    && !this.IsBrake
                    && string.IsNullOrEmpty(LockId)
                    && !(this.Event==CraneEvent.Initialized && this.Status==CraneStatus.Initialized) //状态和事件都为初始化时说明堆垛机才上电未回原点
                    );
            }
        }
        /// <summary>
        /// 是否处于静止状态
        /// </summary>
        public bool IsMotionless
        {
            get
            {
                return this.Status == CraneStatus.Error
                    || this.Status == CraneStatus.Initialized
                    || this.Status == CraneStatus.Manual
                    || this.Status == CraneStatus.Waiting;
            }
        }

        /// <summary>
        /// 指示堆垛机是否处于手动模式
        /// </summary>
        [System.ComponentModel.DisplayName("是否手动")]
        public bool IsManual
        {
            get
            {
                //未获取到远程信息，默认为手动
                return this.Status == CraneStatus.Manual || CraneWorkModeKeyInfo == null || CraneWorkModeKeyInfo.IsManual;
            }
        }

        /// <summary>
        /// 远程急停按下
        /// </summary>
        [System.ComponentModel.DisplayName("远程急停按下")]
        public bool IsBrake
        {
            get
            {
                //必须有远程信息该标志位才有效
                return CraneWorkModeKeyInfo == null || CraneWorkModeKeyInfo.IsBrake;
            }
        }
        /// <summary>
        ///堆垛机相关工作模式切换信号
        /// </summary>
        [System.ComponentModel.DisplayName("远程控制信息")]
        public CraneWorkModeKeyInfo CraneWorkModeKeyInfo { get; internal set; }

        public object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Connected":
                        return Connected;
                    case "LockId":
                        return LockId;
                    case "LockUser":
                        return LockUser;
                    case "Status":
                        return Status;
                    case "Event":
                        return Event;
                    case "CraneForkHorizontalLocation":
                        return CraneForkHorizontalLocation;
                    case "CraneForkVerticalLocation":
                        return CraneForkVerticalLocation;
                    case "CurrentPosition":
                        return CurrentPosition;
                    case "AtStation":
                        return AtStation;
                    case "ErrorCode":
                        return ErrorCode;
                    case "ErrorDescription":
                        return ErrorDescription;
                    case "CurrentTaskId":
                        return CurrentTaskId;
                    case "Loaded":
                        return Loaded;
                    case "IsBusy":
                        return IsBusy;
                    case "IsManual":
                        return IsManual;
                    case "IsBrake":
                        return IsBrake;
                    case "CraneWorkModeKeyInfo":
                        return CraneWorkModeKeyInfo;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }
    }
}
