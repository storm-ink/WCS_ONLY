using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework.Cfg;
using NHibernate.Linq;

namespace Wcs.Framework
{
    public class SerialNumberFactory
    {
        static Random _rnd = new Random();

        static object generateEquipmentTaskIdSync = new object();
        /// <summary>
        /// 生成一个设备任务号
        /// </summary>
        /// <returns></returns>
        public static Int32 GenerateEquipmentTaskId()
        {
            lock (generateEquipmentTaskIdSync)
            {
                using (System.Transactions.TransactionScope tx = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        DateTime today = DateTime.Now.Date;
                        EquipmentTaskIdSerialNumber snObject = unitOfWork.session.Get<EquipmentTaskIdSerialNumber>(today, NHibernate.LockMode.Upgrade);

                        Int32 nextSn = 0;
                    getnextsn:
                        if (snObject == null)
                        {
                            snObject = new EquipmentTaskIdSerialNumber();
                            snObject.DateValue = today;
                            snObject.NextSn = 100;
                            nextSn = snObject.NextSn;
                            snObject.NextSn++;
                            unitOfWork.session.Save(snObject);
                        }
                        else
                        {
                            //2,147,483,647
                            nextSn = snObject.NextSn;
                            snObject.NextSn++;

                            //因为堆垛机和其它设备的任务号最长为8位，所以我们此处必须将任务号限制在7位内，也就是7位的最大数值(8位是环穿的让道任务)
                            if (nextSn > 9999999 || nextSn < 100)
                            {
                                snObject.NextSn = 100;
                                nextSn = snObject.NextSn;
                                snObject.NextSn++;
                            }
                            unitOfWork.session.Update(snObject);
                        }

                        //验证任务否是否已被设备占用
                        if (WcsConfiguration
                            .Instance
                            .DeviceCollection
                            .ParticularDeviceCollection
                            .SelectMany(x => x.DeviceElements)
                            .Where(x => x.Device is TaskableDevice)
                            .Select(x => (TaskableDevice)x.Device)
                            .Where(x => x.OccupiedEquipmentTasks != null)
                            .SelectMany(x => x.OccupiedEquipmentTasks)
                            .Any(x => x == nextSn))
                        {
                            goto getnextsn;
                        }

                        //验证任务是否被数据库内的对象占用
                        bool exists;
                        using (NHUnitOfWork queryUnitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            exists = queryUnitOfWork
                                .session
                                .Query<EquipmentAction>()
                                .Any(x => x.EquipmentTaskId == nextSn);

                            queryUnitOfWork.Commit();
                        }

                        if (exists)
                        {
                            goto getnextsn;
                        }


                        unitOfWork.Commit();

                        tx.Complete();

                        return nextSn;
                    }
                }
            }
        }

        /// <summary>
        /// 生成一个手工任务
        /// </summary>
        /// <returns></returns>
        public static String GenerateManualTaskCode()
        {
            return GenerateManualTaskCode("SGRW");
        }

        /// <summary>
        /// 生成一个手工任务
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static String GenerateManualTaskCode(String prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException("prefix");
            }

            prefix = prefix.Trim();
            if (prefix.Length > 5)
            {
                throw new ArgumentOutOfRangeException("前缀的长度必须大于0小于等于5");
            }

            DateTime today = DateTime.Now.Date;
            Int32 nextSn = 0;
            using (System.Transactions.TransactionScope tx = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    ManualTaskCodeSerialNumber snObject = unitOfWork.session.Get<ManualTaskCodeSerialNumber>(today, NHibernate.LockMode.Upgrade);
                    if (snObject == null)
                    {
                        snObject = new ManualTaskCodeSerialNumber();
                        snObject.DateValue = today;
                        snObject.NextSn = 1;
                        nextSn = snObject.NextSn++;
                        unitOfWork.session.Save(snObject);
                    }
                    else
                    {
                        nextSn = snObject.NextSn++;
                        unitOfWork.session.Update(snObject);
                    }

                    unitOfWork.Commit();

                    tx.Complete();
                }
            }

            return string.Format("{0}{1:yyyyMMdd}{2:000000}", prefix, today, nextSn);
        }

        /// <summary>
        /// 获取一个指定范围内的随机 int 值
        /// </summary>
        /// <param name="minValue">最小</param>
        /// <param name="maxValue"></param>
        /// <returns>一个大于等于 minValue 且小于 maxValue 的 32 位带符号整数，即：返回的值范围包括 minValue 但不包括 maxValue。
        /// 如果 minValue 等于 maxValue，则返回 minValue。
        /// </returns>
        public static int GenerateRandomValue(int minValue, int maxValue)
        {
            return _rnd.Next(minValue, maxValue);
        }
    }
}
