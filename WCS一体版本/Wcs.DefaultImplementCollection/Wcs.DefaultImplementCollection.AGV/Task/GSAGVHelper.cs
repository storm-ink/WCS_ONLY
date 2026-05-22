using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.AGV
{
    public static class SinevaAGVHelper
    {
        static object _lockObj = new object();
        /// <summary>
        /// 创建一个AGV任务号
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string GetSendAGVTaskId(String device)
        {
            lock (_lockObj)
            {
                String sendAGVTaskId = "";
            getnextsn:
                //sendAGVTaskId = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                sendAGVTaskId = System.Guid.NewGuid().ToString("N");

                //验证任务是否被数据库内的对象占用
                bool exists = SinevaAGVDatabaseHand.CheckAgvTaskIdIsExict(sendAGVTaskId, device);

                if (exists)
                    goto getnextsn;

                return sendAGVTaskId;
            }


        }
    }
}
