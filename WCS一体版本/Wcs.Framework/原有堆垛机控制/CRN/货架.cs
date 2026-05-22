
using System;
using System.Collections.Generic;
using System.Data;

namespace Wcs.Framework.CraneControl
{
    /// <summary>货架 信息</summary>
    public partial class Shelf
    {
        internal static Shelf ReadConfig(string sCraneName)
        {
            Shelf sf = new Shelf();

            string sShelfFile = string.Format("{0}/{1}.xml", Config.sPath, sCraneName);
            DataSet dts = new DataSet(); dts.ReadXml(sShelfFile);

            sf.dtbLBit = dts.Tables["LBit"];
            sf.dtbRBit = dts.Tables["RBit"];
            sf.dtbCBit = dts.Tables["CBit"]; sf.dtbCBit.Columns.Add(F_LRL); sf.dtbCBit.Columns.Add(F_LRR);
            sf.dtbPBit = dts.Tables["PBit"];
            sf.dtbXBit = dts.Tables["XBit"];

            dLBit = new Dictionary<string, string>(); dLBit.Add("", "");
            foreach (DataRow dtr in sf.dtbLBit.Rows) dLBit.Add(ToStr(dtr[F_L]), ToStr(dtr[F_LR]));

            sf.ParseShelf(); 
            sf.sCraneName = sCraneName;

            return sf;
        }
        private void ParseShelf()
        {
            int i; string L, LL, LR, C1, R1;

            foreach (DataRow dtr in dtbLBit.Rows)
            {
                i = ToInt(dtr[F_L]); if (i > iMaxL) iMaxL = i; if (i < iMinL) iMinL = i;
            }
            foreach (DataRow dtr in dtbRBit.Rows)
            {
                i = ToInt(dtr[F_R1]); if (i > iMaxR) iMaxR = i; if (i < iMinR) iMinR = i;
                i = ToInt(dtr[F_R2]); if (i > iCMaxR) iCMaxR = i; if (i < iCMinR) iCMinR = i;
            }
            foreach (DataRow dtr in dtbPBit.Rows)
            {
                i = ToInt(dtr[F_C1]); if (i > iMaxC) iMaxC = i; if (i < iMinC) iMinC = i;
                i = ToInt(dtr[F_C2]); if (i > iCMaxC) iCMaxC = i; if (i < iCMinC) iCMinC = i;
            }

            foreach (DataRow dtrC in dtbCBit.Rows)
            {
                i = ToInt(dtrC[F_C1]); if (i > iMaxC) iMaxC = i; if (i < iMinC) iMinC = i;
                i = ToInt(dtrC[F_C2]); if (i > iCMaxC) iCMaxC = i; if (i < iCMinC) iCMinC = i;

                LL = ToStr(dtrC[F_LL]); LR = ToStr(dtrC[F_LR]);

                // 填充 左右伸叉
                dtrC[F_LRL] = dLBit[LL];
                dtrC[F_LRR] = dLBit[LR];

                foreach (DataRow dtrR in dtbRBit.Rows)
                {
                    // 生成货位信息
                    DataRow dtr = dtbPBit.NewRow(); dtbPBit.Rows.Add(dtr);

                    dtr[F_C1] = dtrC[F_C1]; dtr[F_R1] = dtrR[F_R1];
                    dtr[F_C2] = dtrC[F_C2]; dtr[F_R2] = dtrR[F_R2];

                    dtr[F_LL] = dtrC[F_LL]; dtr[F_LR] = dtrC[F_LR];
                    dtr[F_LRL] = dtrC[F_LRL]; dtr[F_LRR] = dtrC[F_LRR];
                }
            }

            // 屏蔽货位伸叉
            foreach (DataRow dtrX in dtbXBit.Rows)
            {
                L = ToStr(dtrX[F_L]); if (L == "") continue;
                C1 = ToStr(dtrX[F_C1]); R1 = ToStr(dtrX[F_R1]);

                foreach (DataRow dtr in dtbPBit.Select(string.Format(
                    "{0}='{3}' AND {1}='{4}' AND {2}='{5}'", F_LL, F_C1, F_R1, L, C1, R1)))
                    dtr[F_LRL] = "";

                foreach (DataRow dtr in dtbPBit.Select(string.Format(
                    "{0}='{3}' AND {1}='{4}' AND {2}='{5}'", F_LR, F_C1, F_R1, L, C1, R1)))
                    dtr[F_LRR] = "";
            }

            dtbLBit = dtbXBit = dtbCBit = dtbRBit = null; dLBit = null;
        }

        private string sCraneName;
        // 货架
        public int iMaxL, iMinL;
        public int iMaxC, iMinC;
        public int iMaxR, iMinR;

        // 堆垛机
        public int iCMaxC, iCMinC;
        public int iCMaxR, iCMinR;

        // 排, 列, 层, 屏蔽位置, 站点位置  配置
        private static Dictionary<string, string> dLBit;
        private DataTable dtbLBit, dtbCBit, dtbRBit, dtbXBit;
        public DataTable dtbPBit;

        public static string F_L = "L";

        public static string F_C1 = "C1";
        public static string F_C2 = "C2";
        public static string F_R1 = "R1";
        public static string F_R2 = "R2";

        public static string F_LL = "LL";
        public static string F_LR = "LR";

        public static string F_LRL = "LRL";
        public static string F_LRR = "LRR";

        static int ToInt(object obj)
        {
            return Convert.ToInt32(obj);
        }
        static string ToStr(object obj)
        {
            return Convert.ToString(obj).Trim();
        }
    }
    public partial class Shelf
    {
        /// <summary>客户列层 转 堆垛机列层</summary>
        public void ParseUTMColRow(HB hb)
        {
            string S, L, C, R;

            S = hb.SHB.Position1; L = S.Substring(0, 2).TrimStart('0'); C = S.Substring(3, 3).TrimStart('0'); R = S.Substring(7, 3).TrimStart('0');
            if (L == "") L = "0"; if (C == "") C = "0"; if (R == "") R = "0";
            foreach (var p1 in dtbPBit.Select(string.Format(
                "{0}='{6}' AND {2}='{7}' AND {3}='{8}' AND {4}<>'' OR {1}='{6}' AND {2}='{7}' AND {3}='{8}' AND {5}<>''",
                F_LL, F_LR, F_C1, F_R1, F_LRL, F_LRR, L, C, R)))
            {
                Position P1 = new Position(C, R);
                P1.MCol = ToInt(p1[F_C2]); P1.MRow = ToInt(p1[F_R2]); hb.P1 = P1;
                hb.LR1 = Util.PEnum<EForkLR>(ToStr(p1[F_LL]) == L ? ToStr(p1[F_LRL]) : ToStr(p1[F_LRR]));

                S = hb.SHB.Position2; L = S.Substring(0, 2).TrimStart('0'); C = S.Substring(3, 3).TrimStart('0'); R = S.Substring(7, 3).TrimStart('0');
                if (L == "") L = "0"; if (C == "") C = "0"; if (R == "") R = "0";
                foreach (var p2 in dtbPBit.Select(string.Format(
                    "{0}='{6}' AND {2}='{7}' AND {3}='{8}' AND {4}<>'' OR {1}='{6}' AND {2}='{7}' AND {3}='{8}' AND {5}<>''",
                    F_LL, F_LR, F_C1, F_R1, F_LRL, F_LRR, L, C, R)))
                {
                    Position P2 = new Position(C, R);
                    P2.MCol = ToInt(p2[F_C2]); P2.MRow = ToInt(p2[F_R2]); hb.P2 = P2;
                    hb.LR2 = Util.PEnum<EForkLR>(ToStr(p2[F_LL]) == L ? ToStr(p2[F_LRL]) : ToStr(p2[F_LRR])); return;
                }

                throw new Exception("终点位置不正确...");
            }

            throw new Exception("起点位置不正确...");
        }
        /// <summary>客户列层 转 堆垛机列层</summary>
        public void ParseUTMColRow(ref Position P)
        {
            foreach (var v in dtbPBit.Select(string.Format("{0}='{1}' AND {2}='{3}'", 
                F_C1, ToInt(P.UCol), F_R1, ToInt(P.URow))))
            {
                P.MCol = ToInt(v[F_C2]); P.MRow = ToInt(v[F_R2]); return;
            }

            throw new Exception("列层位置无法识别...");
        }
        /// <summary>堆垛机列层 转 客户列层</summary>
        public void ParseMTUColRow(int iMCol, int iMRow, ref string sUCol, ref string sURow)
        {
            try
            {
                //var row = dtbPBit.Rows.Cast<DataRow>().Where(x => x[""] == "").FirstOrDefault();
                //var q = from o in dtbCBit.Rows.Cast<DataRow>()
                //        where o["xx"] == ""
                //        select new
                //        {
                //            XX = o["xx"],
                //            YY = o["yyy"]
                //        };

                foreach (var v in dtbPBit.Select(string.Format("{0}='{2}' AND {1}='{3}'", F_C2, F_R2, iMCol, iMRow)))
                {
                    sUCol = ToStr(v[F_C1]).PadLeft(3, '0');
                    sURow = ToStr(v[F_R1]).PadLeft(3, '0'); break;
                }

                if (sUCol == "???")
                {
                    foreach (var v in dtbPBit.Select(string.Format("{0}='{1}'", F_C2, iMCol)))
                    {
                        sUCol = ToStr(v[F_C1]).PadLeft(3, '0'); break;
                    }
                }

                //if (sURow == "???")
                //{
                //    foreach (var v in dtbPBit.Select(string.Format("{0}='{1}'", F_R2, iMRow)))
                //    {
                //        sURow = ToStr(v[F_R1]).PadLeft(3, '0'); break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1} {2} ##", DateTime.Now.ToString("HH-mm-ss.fffff"), sCraneName, ex.ToString());
            }
        }

        /// <summary>伸叉判断</summary>
        public void CheckPutGetOK(LA LA, EForkLR LR)
        {
            foreach (DataRow dtr in dtbPBit.Select(
                string.Format("{0}='{3}' AND {1}='{4}' AND {2}='{5}' OR {0}='{3}' AND {1}='{4}' AND {6}='{5}'",
                F_C2, F_R2, F_LRL, LA.Position.MCol, LA.Position.MRow, LR == EForkLR.L ? "1" : "2", F_LRR)))
                return;

            throw new Exception("非站点位置");
        }
        /// <summary>检查堆垛机状态判断</summary>
        public void CheckStateOK(LA LA, bool bCheckErroEventrState)
        {
            if (LA.State == ECraneState.E12)
                throw new Exception("手动操作, 不能下发任务!");

            if (bCheckErroEventrState)
            {
                if (LA.ErrorCode != "0000")
                    throw new Exception(string.Format("堆垛机返回错误码 '{0}', 不能下发任务", LA.ErrorCode));
                else if (LA.Event != ECraneEvent.E06 && LA.Event != ECraneEvent.E00)
                    throw new Exception("任务事件未完成, 不能下发任务!");
                else if (LA.State != ECraneState.E00 && LA.State != ECraneState.E02 && LA.State != ECraneState.E03)
                    throw new Exception("堆垛机处于非待命状态, 不能下发任务!");
                else if (LA.AtPosition == false)
                    throw new Exception("载货台不在站点位置, 不能下发任务!");
            }

            if (LA.ForkLR != ECraneLR.E1)
                throw new Exception("货叉不在中位, 不能下发任务!"); 
        }
    }
}
