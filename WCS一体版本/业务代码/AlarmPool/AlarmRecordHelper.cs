using ZHQXC.WebAPI;
using NHibernate.Linq;
using NLog;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 故障记录操作类
    /// </summary>
    public static class AlarmRecordHelper
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static object objLock = new object();

        /// <summary>
        /// 获取故障ID
        /// </summary>
        /// <returns></returns>
        public static long GetWaringRecordId()
        {
            lock (objLock)
            {
                Thread.Sleep(1);
                return Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            }
        }

        /// <summary>
        /// 新增故障记录
        /// </summary>
        /// <param name="alarmRecord"></param>
        public static void AddAlarmRecord(AlarmRecord alarmRecord)
        {
            lock (objLock)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var _alarmRecord = unitOfWork.session.Get<AlarmRecord>(alarmRecord.Id);
                    if (_alarmRecord == null)
                    {
                        unitOfWork.session.Save(alarmRecord);
                        unitOfWork.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// 更新当前故障记录
        /// </summary>
        /// <param name="alarmRecord"></param>
        public static void UpdateAlarmRecord(AlarmRecord alarmRecord)
        {
            lock (objLock)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var record = unitOfWork.session.Get<AlarmRecord>(alarmRecord.Id);
                    if (record != null)
                    {
                        if (record.EndingAt != null)
                            return;

                        record.EndingAt = alarmRecord.EndingAt;
                        record.TotalMilliseconds = (long)((DateTime)alarmRecord.EndingAt - alarmRecord.BeginingAt).TotalMilliseconds;
                        unitOfWork.session.SaveOrUpdate(record);
                    }
                    else
                        unitOfWork.session.SaveOrUpdate(alarmRecord);

                    unitOfWork.Commit();
                }
            }
        }

        /// <summary>
        /// 删除当前故障记录
        /// </summary>
        /// <param name="alarmRecord"></param>
        public static void DeleteAlarmRecord(AlarmRecord alarmRecord)
        {
            lock (objLock)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    var record = unitOfWork.session.Get<AlarmRecord>(alarmRecord.Id);
                    if (record != null)
                    {
                        unitOfWork.session.Delete(record);
                        unitOfWork.Commit();
                    }
                }
            }
        }

        #region 归档故障记录
        static object archivedObjLock = new object();
        static List<AlarmRecord> ArchivedList;
        public static void PushArchivedList(AlarmRecord alarmRecord)
        {
            lock (archivedObjLock)
            {
                if (ArchivedList == null)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        ArchivedList = unitOfWork.session.Query<AlarmRecord>().Where(x => x.EndingAt != null).ToList();
                        unitOfWork.Commit();
                    }
                }
                if (!ArchivedList.Any(x => x.Id == alarmRecord.Id))
                    ArchivedList.Add(alarmRecord);
            }
        }
        static void PopArchivedList(long id)
        {
            lock (archivedObjLock)
            {
                ArchivedList = ArchivedList.Where(x => x.Id != id).ToList();
            }
        }

        /// <summary>
        /// 设备告警信息上报
        /// </summary>
        /// <param name="args"></param>
        public static void Proc(object args)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(100);

                    if (ArchivedList == null)
                    {
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            ArchivedList = unitOfWork.session.Query<AlarmRecord>().Where(x => x.EndingAt != null).ToList();
                            unitOfWork.Commit();
                        }
                    }

                    AlarmRecord[] alarmRecords;
                    lock (archivedObjLock)
                    {
                        alarmRecords = ArchivedList.ToArray();
                    }

                    if (alarmRecords.Count() > 0)
                    {
                        foreach (var item in alarmRecords)
                        {
                            //var result = RequestWMSHelper.AlarmReport(item.Id.ToString(), item.GetAlarmHost(), item.WcsAlarmCode.ToString(), item.AlarmName, AlarmStaus.resolve, out string msg);//WCS--->WMS上报告警信息

                            //if (result)
                            //{
                            //    DeleteAlarmRecord(item);
                            //    PopArchivedList(item.Id);
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, typeof(AlarmRecordHelper));
                }
            }
        }
        #endregion
    }
}
