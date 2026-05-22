using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 堆垛机当前位置
    /// </summary>
    public class CraneCurrentLocation:IComparer<CraneCurrentLocation>
    {
        /// <summary>
        /// 当前所在列设备码
        /// </summary>
        public Int32? ColumnDeviceCode { get; set; }
        /// <summary>
        /// 当前所在层设备码
        /// </summary>
        public Int32? LevelDeviceCode { get; set; }

        /// <summary>
        /// 当前所在列用户码
        /// </summary>
        public String ColumnUserCode { get; set; }
        /// <summary>
        /// 当前所在层用户码
        /// </summary>
        public String LevelUserCode { get; set; }

        public int Compare(CraneCurrentLocation x, CraneCurrentLocation y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.ColumnUserCode != y.ColumnUserCode || x.LevelUserCode != y.LevelUserCode)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return String.Format("{0}列{1}层", this.ColumnUserCode??"", this.LevelUserCode??"");
        }
    }
}
