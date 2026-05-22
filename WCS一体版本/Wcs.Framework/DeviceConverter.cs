using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    public static class DeviceConverter
    {
        ///// <summary>
        ///// 将指定的名称转换为设备对象
        ///// </summary>
        ///// <param name="deviceName">要转换的设备名称</param>
        ///// <exception cref="System.ArgumentNullException">deviceName 为 null、空或是仅由空白字符组成</exception>
        ///// <exception cref="System.InvalidOperationException">未找到名称与 deviceName 值匹配的设备</exception>
        ///// <returns>名称为 deviceName 值的设备对象</returns>
        //public static Device ToDeivce(this String deviceName)
        //{
        //    if (String.IsNullOrWhiteSpace(deviceName))
        //    {
        //        throw new ArgumentNullException("deviceName");
        //    }

        //    var deviceElement = Wcs.Framework.Cfg.WcsConfiguration
        //        .Instance
        //        .DeviceCollection
        //        .ParticularDeviceCollection
        //        .SelectMany(x => x.DeviceElements)
        //        .SingleOrDefault(x => string.Equals(x.Device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase));

        //    if (deviceElement == null)
        //    {
        //        throw new InvalidOperationException(string.Format("未找到名称为 {0} 的设备对象", deviceName));
        //    }

        //    return deviceElement.Device;
        //}

        /// <summary>
        /// 将指定的名称转换为 TDevice 类型的设备对象
        /// </summary>
        /// <param name="deviceName">要转换的设备名称</param>
        /// <typeparam name="TDevice">返回的设备类型</typeparam>
        /// <exception cref="System.ArgumentNullException">deviceName 为 null、空或是仅由空白字符组成</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     <param>1、未找到名称与 deviceName 值匹配的设备</param>
        ///     <param>2、名称与 deviceName 值匹配的设备无法转换为 TDevice 类型</param>
        /// </exception>"
        /// <returns>名称为 deviceName 值的设备对象</returns>
        public static TDevice ToDevice<TDevice>(String deviceName)
            where TDevice : Device
        {
            if (String.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("deviceName");
            }

            var deviceElement = Wcs.Framework.Cfg.WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .SingleOrDefault(x => string.Equals(x.Device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase));

            if (deviceElement == null)
            {
                throw new InvalidOperationException(string.Format("未找到名称为 {0} 的设备对象", deviceName));
            }

            Device device = deviceElement.Device;
            if (!(device is TDevice))
            {
                throw new InvalidOperationException(String.Format("{0} 无法转换为 {1} 类型", device, typeof(TDevice)));
            }

            return (TDevice)device;
        }
    }
}
