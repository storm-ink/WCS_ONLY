using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 设备任务号流水
    /// </summary>
    public class EquipmentTaskIdSerialNumber
    {
        public virtual DateTime DateValue { get; set; }
        public virtual Int32 NextSn { get; set; }
    }    
}
