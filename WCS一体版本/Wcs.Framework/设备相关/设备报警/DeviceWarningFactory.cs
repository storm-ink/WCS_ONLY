using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备报警工厂类
    /// <para>用于解析来自不同设备的报警，并将其转换为带有等级描述的设备错误对象</para>
    /// </summary>
    public class DefaultDeviceWarningFactory
    {
        /// <summary>
        /// 无法连接错误码
        /// </summary>
        public const String UNABLE_TO_CONNECT_ERROR_CODE = "unable to connect";
        /// <summary>
        /// 无法连接错误描述
        /// </summary>
        public const String UNABLE_TO_CONNECT_ERROR_DESCRIPTION = "无法连接到设备";

        public DefaultDeviceWarningFactory()
        {

        }
        /// <summary>
        /// 创建一个无法连接的报警
        /// </summary>
        /// <param name="device">设备</param>
        /// <returns></returns>
        public virtual DeviceWarning CreateUnableToConnectError(Device device)
        {
            var error = new DeviceWarning(device, null, DeviceWarningLevel.Warning, UNABLE_TO_CONNECT_ERROR_CODE, UNABLE_TO_CONNECT_ERROR_DESCRIPTION,false);
            return error;
        }
        /// <summary>
        /// 创建一个错误
        /// </summary>
        /// <param name="device">设备</param>
        /// <param name="code">错误码</param>
        /// <param name="source">错误源</param>
        /// <returns></returns>
        public virtual DeviceWarning Create(Device device, String code,Object source)
        {
            var error = new DeviceWarning(device, source, DeviceWarningLevel.Warning, code,null,false);
            return error;
        }
    }
}
