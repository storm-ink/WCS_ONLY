using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线位置
    /// </summary>
    public class ConveyorLocation:Location
    {
        Int32 _deviceCode;
        String _userCode;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deviceCode">           该位置在设备中的编码形式. </param>
        /// <param name="userCode">             用户编码格式： 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）. </param>
        /// <param name="acceptRequestSignal">  是否接受占位请求信号 如果为 false，底层的占位信号将被忽略（完全不处理）. </param>
        /// <param name="device">               位置所属设备. </param>
        public ConveyorLocation(Int32 deviceCode, String userCode, Boolean acceptRequestSignal,Boolean isEntrance,Boolean isExit, Device device)
        {
            this.Device = device;
            _deviceCode = deviceCode;
            this.AcceptRequestSignal = acceptRequestSignal;
            this.IsEntrance = isEntrance;
            this.IsExit = isExit;
            if (!string.IsNullOrWhiteSpace(userCode))
            {
                _userCode = userCode;
            }
            else
            {
                _userCode = String.Format("00-{0:000}-{1:000}", Device.DeviceNo, _deviceCode);
            }
        }
        /// <summary>
        /// 用户编码格式：
        /// 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）
        /// </summary>
        public override string UserCode
        {
            get 
            {
                return _userCode;
            }
        }
        /// <summary>
        /// 该位置在设备中的编码形式.
        /// </summary>
        public override string DeviceCode
        {
            get 
            { 
                return _deviceCode.ToString(); 
            }
        }
        /// <summary>
        /// 类型.
        /// </summary>
        public override LocationType Type
        {
            get { return LocationType.ConveyorLocation; }
        }

        /// <summary>
        /// 是否接受占位请求信号<br />
        /// 如果为 false，底层的占位信号将被忽略（完全不处理）
        /// </summary>
        public Boolean AcceptRequestSignal { get; private set; }
        /// <summary>
        /// 指示该位置是否是入库口<br />
        /// 用于明确指出该位置的一些特殊的作用
        /// </summary>
        public Boolean IsEntrance { get; private set; }
        /// <summary>
        /// 指示该位置是否是出库口<br />
        /// 用于明确指出该位置的一些特殊的作用
        /// </summary>
        public Boolean IsExit { get; private set; }
    }
}
