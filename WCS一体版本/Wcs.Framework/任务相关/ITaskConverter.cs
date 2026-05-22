using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 将其它对象转换为Wcs任务的接口
    /// </summary>
    public interface ITaskConverter
    {
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns></returns>
        Task Convert(object obj);
    }
}
