using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示实现此接口的类型是一个位置通配符
    /// </summary>
    public interface ILocationWildcard
    {
        /// <summary>
        /// 获取该通配符匹配的所有位置
        /// </summary>
        /// <returns></returns>
        Location[] GetMatchedLocations();

        /// <summary>
        /// 标识该通配符位置是否亦可以作为任务执行中的唯一位置
        /// 暂定：true是可以视为唯一位置的，false仅仅只是通配符标志位置
        /// </summary>
        Boolean AbleAsOnlyLocation { get; }
    }
}
