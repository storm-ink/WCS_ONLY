using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wcs.Framework
{
    /// <summary>
    /// 数据对比接口
    /// </summary>
    /// <typeparam name="TResult">  比较结果的数据类型 </typeparam>
    /// <typeparam name="TCompare"> 比较时使用的数据类型 </typeparam>
    public interface IDataComparer<TResult,TCompare>
    {
        /// <summary>
        /// 当发现传入的数据和旧的数据有变化时发生
        /// </summary>
        event DataChangedEventHandler<TResult> DataChanged;
        /// <summary>
        /// 将指定数据和原来旧的数据进行比较
        /// </summary>
        /// <param name="oldData"> 旧数据 </param>
        /// <param name="oldData"> 新数据 </param>
        /// <returns>
        /// 当前对象和指定对象的差异属性集合
        /// </returns>
        CompareResult<TResult>[] Compare(TCompare oldData, TCompare newData);
    }
}
