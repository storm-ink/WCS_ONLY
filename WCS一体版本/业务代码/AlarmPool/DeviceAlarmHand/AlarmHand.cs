using ZHQXC.WebAPI;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;

namespace ZHQXC.AlarmPool
{
    /// <summary>
    /// 报警处理
    /// </summary>
    public class AlarmHand
    {
        /// <summary>
        /// 构造函数 - 设备报警处理
        /// </summary>
        /// <param name="deviceName"></param>
        public AlarmHand(string ownerDevice)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                OldList = unitOfWork.session.Query<AlarmRecord>().Where(x => x.OwnerDevice == ownerDevice).ToList();
                unitOfWork.Commit();
            }
        }

        public List<AlarmRecord> OldList { get; set; }

        public void Handler(List<AlarmRecord> newList)
        {
            var oldListIds = OldList.Select(x => x.WcsAlarmCode).ToList();
            var newListIds = newList.Select(x => x.WcsAlarmCode).ToList();

            //结束历史故障
            var _oldList = OldList.Where(x => !newListIds.Contains(x.WcsAlarmCode)).ToList();
            if (_oldList.Count()>0)
            {
                foreach (var item in _oldList)
                {
                    item.EndingAt = DateTime.Now;
                    item.TotalMilliseconds = (long)((DateTime)item.EndingAt - item.BeginingAt).TotalMilliseconds;
                    AlarmRecordHelper.UpdateAlarmRecord(item);
                    AlarmRecordHelper.PushArchivedList(item);
                }
                var _oldListIds = _oldList.Select(x => x.Id).ToList();
                OldList = OldList.Where(x => !_oldListIds.Contains(x.Id)).ToList();
            }

            //登记新的故障
            var _newList = newList.Where(x => !oldListIds.Contains(x.WcsAlarmCode)).ToList();
            if (_newList.Count() > 0)
            {
                foreach (var item in _newList)
                {
                    AlarmRecordHelper.AddAlarmRecord(item);
                    try
                    {
                        //故障发生时只上报一次，不论成功与否，但是故障归档时一定会上报成功才删除，所以该故障记录一定会上报到WMS
                        //var result = RequestWMSHelper.AlarmReport(item.Id.ToString(), item.GetAlarmHost(), item.WcsAlarmCode.ToString(), item.AlarmName, AlarmStaus.happen, out string msg);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                OldList.AddRange(_newList);
            }
        }
    }
}
