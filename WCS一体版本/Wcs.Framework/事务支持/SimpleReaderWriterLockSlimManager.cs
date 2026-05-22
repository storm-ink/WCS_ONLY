using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wcs.Framework
{
    /// <summary>
    /// 一个简单的锁状态管理器
    /// </summary>
    public class SimpleReaderWriterLockSlimManager
    {
        Dictionary<String, ReaderWriterLockSlim> _lockSlims = new Dictionary<string, ReaderWriterLockSlim>();
        /// <summary>
        /// 获取指定名称的锁对象，如果不存在，将创建一个
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ReaderWriterLockSlim this[string name]
        {
            get
            {
                lock (_lockSlims)
                {
                    if (!_lockSlims.ContainsKey(name))
                    {
                        _lockSlims.Add(name, new ReaderWriterLockSlim());
                    }

                    return _lockSlims[name];
                }
            }
        }
        /// <summary>
        /// 移除指定名称的锁状态
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            lock (_lockSlims)
            {
                if (!_lockSlims.ContainsKey(name))
                {
                    return;
                }

                _lockSlims[name].Dispose();
                _lockSlims.Remove(name);
            }
        }
    }
}
