using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个只读的字典类型。一旦创建将不可修改
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ReadOnlyDictionary<TKey,TValue>:IEnumerable<KeyValuePair<TKey,TValue>>
    {
        Dictionary<TKey, TValue> _dictionary;
        /// <summary>
        /// 初始化 <see cref="T:ReadOnlyDictionary"/> 类型
        /// </summary>
        /// <param name="sources">数据源</param>
        public ReadOnlyDictionary(Dictionary<TKey,TValue> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException("sources");
            }

            _dictionary = sources;
        }

        public Int32 Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
        }
        public Boolean ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public Boolean ContainsValue(TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
