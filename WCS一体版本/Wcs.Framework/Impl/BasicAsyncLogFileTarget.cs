using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 基础的（简单的）异步日志文件输出目标
    /// </summary>
    public class BasicAsyncLogFileTarget:LogTarget
    {
        /// <summary>
        /// 获取日志目录完全路径
        /// </summary>
        public String DefaultLogDir
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Logs");
            }
        }
        /// <summary>
        /// 获取文件名称
        /// </summary>
        public String FileName
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            }
        }
        /// <summary>
        /// 日志保存目录
        /// </summary>
        public String Dir { get; protected set; }
        /// <summary>
        /// 文件编码格式
        /// </summary>
        public Encoding Encoding { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="targetSelection">配置节点</param>
        public BasicAsyncLogFileTarget(XmlNode targetSelection):base(targetSelection)
        {
            string dir = "";
            if (targetSelection.Attributes["dir"] != null)
            {
                dir = targetSelection.Attributes["dir"].Value;
            }

            Dir = System.IO.Path.Combine(DefaultLogDir, dir);
            string encodingName = "";
            if(targetSelection.Attributes["encoding"]!=null)
            {
                encodingName = targetSelection.Attributes["encoding"].Value;
            }
            if (!string.IsNullOrWhiteSpace(encodingName))
            {
                Encoding = System.Text.Encoding.GetEncoding(encodingName);
            }
            else
            {
                Encoding = System.Text.Encoding.Default;
            }
            lock (this)
            {
                if (!System.IO.Directory.Exists(Dir))
                {
                    System.IO.Directory.CreateDirectory(Dir);
                }
            }
        }
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="logger">日志对象</param>
        /// <param name="logEntry">日志条目</param>
        public override void Output(Logger logger, LogEntry logEntry)
        {
            //writeBytesDelegate x = writeBytes;
            //x.BeginInvoke(logger, logEntry, (IAsyncResult asyncResult) =>
            //{
            //    ((writeBytesDelegate)asyncResult.AsyncState).EndInvoke(asyncResult);
            //},x);
            if (_thread == null)
            {
                _thread = new Thread(dispatch);
                _thread.IsBackground = true;
                _thread.Priority = ThreadPriority.BelowNormal;
                _thread.Start();
            }

            logEntries.Add(logEntry);
        }

        List<LogEntry> logEntries = new List<LogEntry>();
        Thread _thread;
        void dispatch()
        {
            while (true)
            {
                Thread.Sleep(50);

                try
                {
                    if (logEntries.Count == 0)
                    {
                        continue;
                    }
                    LogEntry logEntry = logEntries.First();
                    logEntries.Remove(logEntry);
                    lock (this)
                    {
                        using (FileStream writer = new FileStream(Path.Combine(Dir, FileName), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            var bytes = this.Encoding.GetBytes(logEntry.ToString() + "\r\n");
                            writer.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
