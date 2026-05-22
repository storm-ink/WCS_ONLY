using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Wcs
{
    public static class ThreadExtentions
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        static Thread _processThread;
        static List<Thread> _threads = new List<Thread>();

        static ThreadExtentions()
        {
            _processThread = new Thread(Process);
            _processThread.Name = "线程管理";
            _processThread.StartAndManaged();
        }

        public static Thread[] Threads
        {
            get
            {
                lock (_threads)
                {
                    return _threads.ToArray();
                }
            }
        }

        /// <summary>
        /// 获取当前线程的系统Id（非托管线程Id）
        /// </summary>
        /// <param name="thread"></param>
        /// <returns></returns>
        public static int GetThreadId(this Thread thread)
        {
            var f = typeof(Thread).GetField("DONT_USE_InternalThread",
                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

            var pInternalThread = (IntPtr)f.GetValue(thread);
            var nativeId = Marshal.ReadInt32(pInternalThread, (IntPtr.Size == 8) ? 548 : 348); // found by analyzing the memory

            return nativeId;
        }

        /// <summary>
        /// 导致操作系统将当前实例的状态更改为 System.Threading.ThreadState.Running，并将该线程对象加入管理。
        /// </summary>
        /// <param name="thread">目标线程</param>
        public static void StartAndManaged(this Thread thread)
        {
            if ((thread.ThreadState & ThreadState.Stopped) != ThreadState.Stopped && (thread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
            {
                throw new InvalidOperationException(string.Format("该操作对 Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 无效。",
                    thread.ManagedThreadId, thread.Name, thread.ThreadState));
            }

            lock (_threads)
            {
                if (_threads.Any(x => x == thread))
                {
                    throw new InvalidOperationException(string.Format("Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 已存在。",
                    thread.ManagedThreadId, thread.Name, thread.ThreadState));
                }

                thread.Start();
                _threads.Add(thread);
            }
        }

        /// <summary>
        /// 使操作系统将当前实例的状态更改为 System.Threading.ThreadState.Running，并选择提供包含线程执行的方法要使用的数据的对象，并将该线程对象加入管理。
        /// </summary>
        /// <param name="thread">目标线程</param>
        /// <param name="parameter">一个对象，包含线程执行的方法要使用的数据。</param>
        public static void StartAndManaged(this Thread thread, object parameter)
        {
            if ((thread.ThreadState & ThreadState.Stopped) != ThreadState.Stopped && (thread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
            {
                throw new InvalidOperationException(string.Format("该操作对 Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 无效。",
                    thread.ManagedThreadId, thread.Name, thread.ThreadState));
            }

            lock (_threads)
            {
                if (_threads.Any(x => x == thread))
                {
                    throw new InvalidOperationException(string.Format("Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 已存在。",
                    thread.ManagedThreadId, thread.Name, thread.ThreadState));
                }

                thread.Start(parameter);
                _threads.Add(thread);

                _logger.Debug1((string.Format("Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 被添加到线程列表。",
                        thread.ManagedThreadId, thread.Name, thread.ThreadState)), "ThreadExtentions");
            }

        }

        static void Process()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1000);

                try
                {
                    lock (_threads)
                    {
                        var tmp = _threads.Where(x =>
                            x == null
                            || (x.ThreadState & ThreadState.Aborted) == ThreadState.Aborted
                            || (x.ThreadState & ThreadState.AbortRequested)== ThreadState.AbortRequested
                            || (x.ThreadState & ThreadState.StopRequested)== ThreadState.StopRequested
                            || (x.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                            .ToArray();

                        foreach (var thread in tmp)
                        {
                            _threads.Remove(thread);
                            if (thread != null)
                            {
                                _logger.Debug1(string.Format("Thread（ManagedThreadId：{0},Name:{1},ThreadState:{2}） 被从线程列表中移除。",thread.ManagedThreadId, thread.Name, thread.ThreadState), "ThreadExtentions");
                            }
                            else
                            {
                                _logger.Debug1("一个空的Thread对象被从线程列表中移除。", "ThreadExtentions");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, "ThreadExtentions");
                }
            }
        }
    }
}
