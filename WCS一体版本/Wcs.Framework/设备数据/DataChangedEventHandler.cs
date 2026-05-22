using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示 IDataComparer 的 DataChanged 事件处理程序
    /// </summary>
    public delegate void DataChangedEventHandler<T>(CompareResult<T> compareResult);
}
