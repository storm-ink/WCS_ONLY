using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 有生命周期的对象类型
    /// </summary>
    /// <typeparam name="T">源对象类型</typeparam>
    public class HaveLifeCyclesObject<T>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="source">原对象</param>
        /// <param name="lifeCycle">生命周期</param>
        public HaveLifeCyclesObject(T source, TimeSpan lifeCycle)
        {
            this.Source = source;
            this.LifeCycle = lifeCycle;
            this.OverdueTime = DateTime.Now.Add(lifeCycle);
        }
        /// <summary>
        /// 原对象值
        /// </summary>
        public T Source { get; private set; }
        /// <summary>
        /// 生命周期
        /// </summary>
        public TimeSpan LifeCycle { get; private set; }
        /// <summary>
        /// 获取过期时间
        /// </summary>
        public DateTime OverdueTime { get; private set; }
        /// <summary>
        /// 是否已过期
        /// </summary>
        public Boolean IsOverdue
        {
            get
            {
                return DateTime.Now <= this.OverdueTime;
            }
        }
    }
}
