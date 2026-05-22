/*
 * ================================================
 * 创建人：王建军
 * 创建日期：2013/1/7
 * 备注：
 * 
 * 修改人：
 * 修改日期：
 * 备注：
 * ================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Wcs.Framework
{
    /// <summary>
    /// 手工任务号
    /// </summary>
    public class ManualTaskCodeSerialNumber
    {
        public virtual DateTime DateValue { get; set; }
        public virtual Int32 NextSn { get; set; }
    }
}
