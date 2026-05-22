
/*
 * 开 发 者: 朱庆丰
 * 开发时间: 2011/07/04
 * 模块名称: Alarm
 * 模块说明: 错误码配置处理
 * 备    注: 
 */

using System;
using System.Data;
using System.IO;

namespace Wcs.Framework.CraneControl
{
    /// <summary>报警信息</summary>
    public class Alarm
    {
        /// <summary>错误码 配置数据</summary>
        public static DataSet dsAlarm;

        /// <summary>读错误码数据</summary>
        public static void ReadConfig()
        {
            try
            {
                if (dsAlarm == null) dsAlarm = new DataSet();
                dsAlarm.ReadXml(Directory.GetDirectories(Directory.GetCurrentDirectory(), "P_*")[0] + "/Alarm.xml");
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("读取错误码配置异常\r\n{0}", ex.ToString()));
            }
        }
        /// <summary>读错误码相关信息</summary>
        public static string ReadErrorInfo(string sECode)
        {
            int icode;
            string sCode1=sECode;
            if(int.TryParse(sECode,out icode))
            {
                sCode1=icode.ToString();
            }
            DataRow[] arr = dsAlarm.Tables["ErrorCode"].Select(string.Format("{0}='{1}' or {0}='{2}'", "EID", sECode, sCode1));

            return arr.Length > 0 ? Convert.ToString(arr[0]["EName"]) : sECode;
        }
        /// <summary>读错误码相关信息</summary>
        private static string ReadErrorInfo(EError eError, string sValue)
        {
            try
            {
                if (eError == EError.Description)
                    return Convert.ToString(dsAlarm.Tables["Description"].Select(string.Format("{0}='{1}'", "DID", sValue))[0]["DName"]);
                else if (eError == EError.Solution)
                    return Convert.ToString(dsAlarm.Tables["Solution"].Select(string.Format("{0}='{1}'", "SID", sValue))[0]["SName"]);
                else
                    return "";
            }
            catch //(Exception ex)
            {
                return "";
            }
        }
        
        /// <summary> 错误信息</summary>
        enum EError
        {
            /// <summary>错误码 对应的错误名</summary>
            Name,
            /// <summary>错误码 对应的错误描述</summary>
            Description,
            /// <summary>错误码 对应的错误处理</summary>
            Solution
        }
    }
}
