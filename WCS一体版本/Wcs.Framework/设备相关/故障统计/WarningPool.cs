using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
namespace Wcs.Framework
{
    /// <summary>
    /// 警告池<br />
    /// 负责将报警统计数据记录到数据库
    /// </summary>
    internal class WarningPool
    {
        static List<WarningRecord> _warningRecords;
        static WarningPool()
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                _warningRecords = unitOfWork
                    .session
                    .Query<WarningRecord>()
                    .Where(x => x.EndingAt == null)
                    .ToList();
            }
        }
        /// <summary>
        /// 添加一个警告
        /// </summary>
        /// <param name="deviceName">设备名称</param>
        /// <param name="deviceType">设备类型</param>
        /// <param name="warning"></param>
        public static void AddWarning(String deviceName, String deviceType, DeviceWarning warning)
        {
            lock (_warningRecords)
            {
                var oldWarning = _warningRecords.FirstOrDefault(x => x.Device == deviceName && x.DeviceType == deviceType);
                if (oldWarning == null)
                {
                    var warningRecord = new WarningRecord
                    {
                        Category = warning.Category,
                        Code = warning.Code,
                        Device = deviceName,
                        Name = warning.Description,
                        IsFault=warning.IsFault,
                        DeviceType = deviceType                            
                    };
                    Begining(warningRecord);
                }
                else
                {
                    //故障重复引发时不做任务操作
                    if (oldWarning.Code == warning.Code)
                    {
                        return;
                    }
                    else
                    {
                        Ending(oldWarning);
                    }
                }
            }
        }
        /// <summary>
        /// 结束一个警告
        /// </summary>
        /// <param name="deviceName">设备名称</param>
        /// <param name="deviceType">设备类型</param>
        public static void EndingWarning(String deviceName,String deviceType)
        {
            lock (_warningRecords)
            {
                var oldWarning = _warningRecords.FirstOrDefault(x => x.Device == deviceName && x.DeviceType == deviceType);
                if (oldWarning == null)
                {
                    return;
                }
                else
                {
                    Ending(oldWarning);
                }
            }
        }

        static void Ending(WarningRecord warningRecord)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                var obj = unitOfWork.session.Query<WarningRecord>()
                    .Where(x => x.Device == warningRecord.Device
                        && x.DeviceType==warningRecord.DeviceType
                        && x.EndingAt == null
                        && x.Code == warningRecord.Code)
                        .FirstOrDefault();

                //如果数据库中已登记了该故障，并且未消失，则更新消失时间
                if (obj != null)
                {
                    obj.EndingAt = DateTime.Now;
                    unitOfWork.session.Update(obj);
                }
                else
                {
                    //如果数据库中未登录该故障，则登记一个新数据
                    warningRecord.EndingAt = DateTime.Now;
                    unitOfWork.session.Save(warningRecord);
                }

                unitOfWork.Commit();
            }

            _warningRecords.Remove(warningRecord);
        }

        static void Begining(WarningRecord warningRecord)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {

                //登记一个新数据
                unitOfWork.session.Save(warningRecord);

                unitOfWork.Commit();
            }

            _warningRecords.Add(warningRecord);
        }
    }
}
