using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备故障帮助类
    /// </summary>
    public static class DeviceErrorHelper
    {
        static DeviceErrorType[] _deviceErrorTypes;
        public static DeviceErrorType[] DeviceErrorTypes
        {
            get
            {
                if (_deviceErrorTypes == null || _deviceErrorTypes.Length == 0)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        _deviceErrorTypes = unitOfWork.session.Query<DeviceErrorType>().ToArray();
                        unitOfWork.Commit();
                    }
                }
                return _deviceErrorTypes;
            }
        }

        /// <summary>
        /// 通过设备类型和错误码确认错误
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static DeviceErrorType GetDeviceErrorFromErrorCode(String typeName, string errorCode)
        {
            return DeviceErrorTypes.FirstOrDefault(x => x.DeviceType == typeName && x.DeviceErrorCode == errorCode);
        }
        /// <summary>
        /// 通过设备类型和故障名称确认错误
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="ErrorName"></param>
        /// <returns></returns>
        public static DeviceErrorType GetDeviceErrorFromErrorName(String typeName, String errorName)
        {
            return DeviceErrorTypes.FirstOrDefault(x => x.DeviceType == typeName && x.ErrorName == errorName);
        }

        static object objLocker = new object();
        public static DeviceErrorType AddDeviceErrorType(DeviceErrorType deviceErrorType)
        {
            lock (objLocker)
            {
                var _deviceErrorType = DeviceErrorTypes.FirstOrDefault(x => x.DeviceType == deviceErrorType.DeviceType && x.DeviceErrorCode == deviceErrorType.DeviceErrorCode);
                if (_deviceErrorType != null)
                    return _deviceErrorType;

                if (_deviceErrorTypes.Length == 0)
                    deviceErrorType.Id = 1;
                else
                    deviceErrorType.Id = _deviceErrorTypes.Max(x => x.Id) + 1;

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    unitOfWork.session.Save(deviceErrorType);
                    unitOfWork.Commit();
                }
                _deviceErrorTypes = null;
                return deviceErrorType;
            }
        }
    }
}
