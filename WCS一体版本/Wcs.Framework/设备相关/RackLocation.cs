using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 货架位置
    /// </summary>
    public class RackLocation : Location
    {
        /// <summary>
        /// 货位所在 排
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// 货位所在 列
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// 货位所在 层
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 用户编码.
        /// </summary>
        public override string UserCode
        {
            get
            {
                return string.Format("{0:00}-{1:000}-{2:000}", Line, Column, Level);
            }
        }
        /// <summary>
        /// 该位置在设备中的编码形式.
        /// </summary>
        public override string DeviceCode
        {
            get
            {
                return string.Format("{0:00}{1:000}{2:000}", Line, Column, Level);
            }
        }

        public override bool Equals(object obj)
        {
            RackLocation rackLocation = obj as RackLocation;
            if (rackLocation == null)
            {
                return false;
            }

            if (rackLocation.Device == this.Device
                && string.Equals(rackLocation.DeviceCode, this.DeviceCode, StringComparison.CurrentCultureIgnoreCase)
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 将堆垛机配置里的货位配置转换为强类型
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IEnumerable<RackLocation> ConvertTo(Crane crane,DataRowCollection rows)
        {
            List<RackLocation> ret = new List<RackLocation>();
            foreach (DataRow row in rows)
            {
                int column = Convert.ToInt32(row["C1"]);
                int level = Convert.ToInt32(row["R1"]);

                if (row.Table.Columns.Contains("LL"))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(row["LL"])) && !string.IsNullOrEmpty(Convert.ToString(row["LRL"])))
                    {
                        ret.Add(new RackLocation
                        {
                            Level = level,
                            Column = column,
                            Line = Convert.ToInt32(row["LL"]),
                            Device = crane
                        });
                    }

                }

                if (row.Table.Columns.Contains("LR"))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(row["LR"])) && !string.IsNullOrEmpty(Convert.ToString(row["LRR"])))
                    {
                        ret.Add(new RackLocation
                        {
                            Level = level,
                            Column = column,
                            Line = Convert.ToInt32(row["LR"]),
                            Device = crane
                        });
                    }

                }
            }
            return ret;
        }
        /// <summary>
        /// 类型.
        /// </summary>
        public override LocationType Type
        {
            get { return LocationType.RackLocation; }
        }
    }
}
