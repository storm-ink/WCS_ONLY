
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>发送报文</summary>
    [Serializable]
    [DataContract]
    public class RequestTelex
    {
        /// <summary>报文头</summary>
        [DataMember]
        public string Head { get; set; }
        /// <summary>报文信息</summary>
        [DataMember]
        public string Body { get; set; }

        /// <summary>报文头 + 报文信息</summary>
        public string Text
        {
            get
            {
                return Head + Body;
            }
        }

        ///// <summary>请求发送报文转换</summary>
        //public static RequestTelex aParse(string sTelex)
        //{
        //    if (sTelex.Length < 2) return null;

        //    switch (sTelex.Substring(0, 2))
        //    {
        //        case "HB": return ParseHB(sTelex);
        //        case "HA": return HA.Instance;
        //        case "HP": return HP.Instance;
        //        case "HE": return HE.Instance;
        //        case "HC": return HC.Instance;

        //        default: return null;
        //    }
        //}

        //internal static HB ParseHB(string sTelex)
        //{
        //    string s = sTelex.Substring(2);

        //    Position pS = new Position(int.Parse(s.Substring(2, 3)), int.Parse(s.Substring(5, 3)));
        //    Position pE = new Position(int.Parse(s.Substring(10, 3)), int.Parse(s.Substring(13, 3)));

        //    //return new HB(
        //    //    Util.PEnum<EForkLR>(s.Substring(0, 2)),
        //    //    pS,
        //    //    Util.PEnum<EForkLR>(s.Substring(8, 2)),
        //    //    pE,
        //    //    Util.PEnum<HBCommand>(s.Substring(16, 2)),
        //    //    s.Substring(18, 8));

        //    return null;
        //}
    }
}
