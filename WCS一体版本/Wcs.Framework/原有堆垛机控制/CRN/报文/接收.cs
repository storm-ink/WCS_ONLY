
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>接收回复报文</summary>
    [Serializable]
    [DataContract]
    public class ResponseTelex
    {
        /// <summary>报文头</summary>
        [DataMember]
        public string Head { get; protected set; }
        /// <summary>报文信息</summary>
        [DataMember]
        public string Body { get; protected set; }

        /// <summary>报文头 + 报文信息</summary>
        public string Text
        {
            get
            {
                return Head + Body;
            }
        }

        public static LA ParseLA(string CName, string sTelex)
        {
            try
            {
                sTelex = sTelex.Substring(2);

                LA la = new LA(
                     Util.PEnum<ECraneState>(sTelex.Substring(0, 2)),
                     new Position(CName, int.Parse(sTelex.Substring(2, 3)), int.Parse(sTelex.Substring(5, 3))),
                     Util.PEnum<ECraneLR>(sTelex.Substring(8, 1)),
                     Util.PEnum<ECraneTL>(sTelex.Substring(9, 1)),
                     sTelex.Substring(10, 1) == "0",
                     sTelex.Substring(11, 4),
                     Util.PEnum<ECraneEvent>(sTelex.Substring(15, 1)),
                     sTelex.Substring(16, 8));

                return la;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(string.Format("解析 {0} LA 报文 {1} 时出现异常", CName,sTelex));

                return null;
            }
        }
        internal static LB ParseLB(string sTelex)
        {
            if (sTelex.Length != 11) return null;
            sTelex = sTelex.Substring(2);

            return new LB((ETaskState)Enum.Parse(typeof(ETaskState), sTelex.Substring(0, 1)), sTelex.Substring(1));
        }
    }
}
