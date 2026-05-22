using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot位置
    /// </summary>
    public class RobotLocation : Location
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deviceCode">           该位置在设备中的编码形式. </param>
        /// <param name="userCode">             用户编码格式： 00-设备编号（不足三位时补0）-设备货位号（不足三位时补0）. </param>
        /// <param name="device">               位置所属设备. </param>
        public RobotLocation(Int32 deviceCode, String userCode, Device device)
            :base(deviceCode.ToString(),userCode,device)
        {
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
    }
}
