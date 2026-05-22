using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace Wcs.Framework
{
    /// <summary>
    /// 一个简单用于管理带生存期对象集合的生命周期管理器.
    /// </summary>
    /// <typeparam name="T">    泛型参数. </typeparam>
    public class LifeCycleManager<T>
    {
        private ReaderWriterLockSlim _itemsLock = new ReaderWriterLockSlim();
        private static Hashtable _items = new Hashtable();
        public Logger Logger { get; private set; }
        /// <summary>
        /// 获取已存在的元素个数.
        /// </summary>
        public int CachedItemsNumber
        {
            get
            {
                _itemsLock.EnterReadLock();
                try
                {
                    return _items.Count;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message, this, ex);
                }
                finally
                {
                    _itemsLock.ExitReadLock();
                }

                return 0;
            }
        }
        /// <summary>
        /// 默认构造函数.
        /// </summary>
        public LifeCycleManager()
        {
            this.Logger = new Logger(this, new Impl.NLogTarget(this.ToString()));
            ThreadPool.QueueUserWorkItem(refreshTimerCallback, null);
        }
        /// <summary>
        /// 获取指定键的键值.
        /// </summary>
        /// <param name="key">  键. </param>
        public HaveLifeCyclesObject<T> this[object key]
        {
            get
            {
                _itemsLock.EnterUpgradeableReadLock();
                try
                {
                    if (_items.ContainsKey(key))
                    {
                        HaveLifeCyclesObject<T> res = (HaveLifeCyclesObject<T>)_items[key];
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message, this, ex);
                }
                finally
                {
                    _itemsLock.ExitUpgradeableReadLock();
                }

                return null;
            }
        }
        /// <summary>
        /// 添加或修改一个对象.
        /// </summary>
        /// <param name="key">                  键. </param>
        /// <param name="source">               源数据. </param>
        /// <param name="timeoutMilliseconds">  生命周期. </param>
        public void Add(object key, T source, Int32 timeoutMilliseconds)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new HaveLifeCyclesObject<T>(source, TimeSpan.FromMilliseconds(timeoutMilliseconds));
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex.Message, this, ex);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// 移除指定键的对象.
        /// </summary>
        /// <param name="key">  键. </param>
        public void Remove(object key)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items.Remove(key);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex.Message, this, ex);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// 生命周期检测主线程.
        /// </summary>
        /// <param name="state">    状态. </param>
        private void refreshTimerCallback(object state)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                Dictionary<object, HaveLifeCyclesObject<T>> delItems = new Dictionary<object, HaveLifeCyclesObject<T>>();
                DateTime dtNow = DateTime.Now;
                foreach (DictionaryEntry de in _items)
                {
                    HaveLifeCyclesObject<T> ci = (HaveLifeCyclesObject<T>)de.Value;
                    if (ci.IsOverdue)
                    {
                        delItems.Add(de.Key, ci);
                    }
                }
                if (delItems.Count > 0)
                {
                    foreach (KeyValuePair<object, HaveLifeCyclesObject<T>> kvp in delItems)
                    {
                        if (_items.ContainsKey(kvp.Key))
                        {
                            _items.Remove(kvp.Key);
                        }
                    }
                    delItems = null;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex.Message, this, ex);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
            Thread.Sleep(100);
        }
    }
}
