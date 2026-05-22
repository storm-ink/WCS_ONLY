
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>站点位置</summary>
    [Serializable]
    [DataContract]
    public struct Position
    {
        int iCol, iRow; string sCol, sRow;

        /// <summary>用户 列层</summary>
        public Position(string sCol, string sRow)
        {
            this.sCol = sCol.PadLeft(3, '0');
            this.sRow = sRow.PadLeft(3, '0');

            this.iCol = this.iRow = 0;
        }
        /// <summary>设备 列层 转 客户 列层</summary>
        public Position(string sCName, int iCol, int iRow)
        {
            this.iCol = iCol; this.iRow = iRow; sCol = sRow = "???";

            Config.Get(sCName).Shelf.ParseMTUColRow(iCol, iRow, ref sCol, ref sRow);
        }

        /// <summary>设备 列</summary>
        [DataMember]
        public int MCol
        {
            get
            {
                return iCol;
            }
            set
            {
                iCol = value;
            }
        }
        /// <summary>设备 层</summary>
        [DataMember]
        public int MRow
        {
            get
            {
                return iRow;
            }
            set
            {
                iRow = value;
            }
        }

        /// <summary>用户 列</summary>
        [DataMember]
        public string UCol
        {
            get
            {
                return sCol;
            }
            set
            {
                sCol = value;
            }
        }
        /// <summary>用户 层</summary>
        [DataMember]
        public string URow
        {
            get
            {
                return sRow;
            }
            set
            {
                sRow = value;
            }
        }
    }
}
