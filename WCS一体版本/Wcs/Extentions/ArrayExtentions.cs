using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// Array 类型扩展
    /// </summary>
    public static class ArrayExtentions
    {
        /// <summary>
        /// 搜索与指定谓词所定义的条件相匹配的元素，并返回 sources 中第一个匹配元素的从零开始的索引。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <param name="match">System.Predicate<T> 委托，用于定义要搜索的元素的条件。</param>
        /// <returns>如果找到与 match 定义的条件相匹配的第一个元素，则为该元素的从零开始的索引；否则为 -1。</returns>
        public static Int32 FindIndex<T>(this T[] sources,Predicate<T> match)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (match(sources[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///  搜索与指定谓词所定义的条件相匹配的元素，并返回 sources 中最后一个匹配元素的从零开始的索引。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <param name="match">System.Predicate<T> 委托，用于定义要搜索的元素的条件。</param>
        /// <returns>搜索与指定谓词所定义的条件相匹配的元素，并返回 sources 中最后一个匹配元素的从零开始的索引。</returns>
        public static Int32 FindLastIndex<T>(this IEnumerable<T> sources, Predicate<T> match)
        {
            var count = sources.Count();
            for (int i = count-1; i >= 0; i--)
            {
                if (match(sources.ElementAt(i)))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 搜索指定的对象，并返回 sources 中第一个匹配项的从零开始的索引。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Int32 IndexOf<T>(this T[] sources, T item)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (ReferenceEquals(sources[i],item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
