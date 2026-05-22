using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线位置
    /// </summary>
    public class ConveyorLocation:Location
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deviceCode">           该位置在设备中的编码形式. </param>
        /// <param name="userCode">             用户编码格式： 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）. </param>
        /// <param name="acceptRequestSignal">  是否接受占位请求信号 如果为 false，底层的占位信号将被忽略（完全不处理）. </param>
        /// <param name="device">               位置所属设备. </param>
        public ConveyorLocation(Int32 deviceCode, String userCode, Device device, Boolean acceptRequestSignal, Boolean isEntrance, Boolean isExit, Boolean hasFictitousLocation, Boolean isFictitiousLocation, String fictitiousConvertibleCode,string region)
            :base(deviceCode.ToString(),userCode,device)
        {
            this.AcceptRequestSignal = acceptRequestSignal;
            this.IsEntrance = isEntrance;
            this.IsExit = isExit;
            this.HasFictitousLocation = hasFictitousLocation;
            this.IsFictitiousLocation = isFictitiousLocation;
            this.FictitiousConvertibleCode = fictitiousConvertibleCode;
            this.Region = region;
        }
        /// <summary>
        /// 用户编码格式：
        /// 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）
        /// </summary>
        public override string UserCode
        {
            get 
            {
                if (!string.IsNullOrWhiteSpace(base.UserCode))
                {
                    return base.UserCode;
                }
                else
                {
                    return String.Format("00-{0:000}-{1:000}", Device.No, base.UserCode);
                }
            }
        }
        /// <summary>
        /// 是否存在虚拟位置
        /// </summary>
        public Boolean HasFictitousLocation { get; set; }
        /// <summary>
        /// 是否是虚拟位置
        /// </summary>
        public Boolean IsFictitiousLocation { get; set; }
        /// <summary>
        /// 虚拟位置
        /// </summary>
        public String FictitiousConvertibleCode { get; set; } 
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
        /// <summary>
        /// <para>区域</para>
        /// 目前仅手动配置标志分区未知
        /// </summary>
        public string Region { get; private set; }
    }
}
