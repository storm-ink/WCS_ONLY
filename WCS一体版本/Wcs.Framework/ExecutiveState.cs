using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 系统执行状态
    /// </summary>
    public static class ExecutiveState
    {
        static Dictionary<String, ExecutiveStateCounter> _activeCounters;
        static Dictionary<String, Boolean?> _marks;
        static ExecutiveState()
        {
            _activeCounters = new Dictionary<String, ExecutiveStateCounter>();
            _marks = new Dictionary<String, Boolean?>();
        }

        /// <summary>
        /// 获取所有活动计数器
        /// </summary>
        public static ExecutiveStateCounter[] ActiveCounters
        {
            get
            {
                lock(_activeCounters)
                {
                    return _activeCounters
                        .Select(x => x.Value)
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值，指示应用程序是否正在退出。
        /// </summary>
        public static Boolean ApplicationExiting
        {
            get
            {
                return GetMark("ApplicationExiting")
                    .GetValueOrDefault(false);
            }
            set
            {
                SetMark("ApplicationExiting", true);
            }
        }

        /// <summary>
        /// <para>获取一个指定名称的活动计数器。</para>
        /// <para>如果不存在，则会自动创建一个。</para>
        /// <para>该功能设计的初衷是为了记录某些线程被激活的实例数，以便在应用程序退出时等待这些实例正常退出。</para>
        /// <para>所以所有记数器都应该维护其值为0，防止在应用程序退出时出现持续等待的现象。</para>
        /// </summary>
        /// <param name="name">活动计数器名称</param>
        /// <returns></returns>
        public static ExecutiveStateCounter ActiveCounter(String name)
        {
            lock(_activeCounters)
            {
                if (!_activeCounters.ContainsKey(name))
                {
                    _activeCounters.Add(name, new ExecutiveStateCounter(name));
                }

               return _activeCounters[name];
            }

        }

        /// <summary>
        /// 获取指示名称的标记值，如果不存在将返回null
        /// </summary>
        /// <param name="name">要获取值的标记名称</param>
        /// <returns></returns>
        public static Boolean? GetMark(String name)
        {
            lock (_marks)
            {
                if (!_marks.ContainsKey(name))
                {
                    return null;
                }

                return _marks[name];
            }
        }
        /// <summary>
        /// 根据名称，设置指定的标记值
        /// </summary>
        /// <param name="name">要设置的标记名称</param>
        /// <param name="value">新的标记值</param>
        public static void SetMark(String name,Boolean value)
        {
            lock (_marks)
            {
                _marks[name] = value;
            }
        }
    }

    /// <summary>
    /// 表示一个状态计数器
    /// </summary>
    public class ExecutiveStateCounter
    {
        String _name;
        Int32 _value;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">状态计数器名称</param>
        public ExecutiveStateCounter(String name)
        {
            _name = name;
        }

        /// <summary>
        /// 获取当前状态计数器的名称
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 获取计数器当前值
        /// </summary>
        public Int32 Value
        {
            get
            {
                lock (this)
                {
                    return _value;
                }
            }
        }

        /// <summary>
        /// 当前计数器值减1
        /// </summary>
        /// <returns>返回增加计数器的新的当前值</returns>
        public Int32 Decrement()
        {
            lock (this)
            {
                if (_value == 0)
                {
                    throw new InvalidOperationException("当前计数器值为 0，无法执行此操作。");
                }

                _value--;
            }

            return _value;
        }

        /// <summary>
        /// 当前计数器值加1
        /// </summary>
        /// <returns>返回增加计数器的新的当前值</returns>
        public Int32 Increment()
        {
            lock(this)
            {
                _value++;
            }

            return _value;
        }
    }
}
