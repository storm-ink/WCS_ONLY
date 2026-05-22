using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;

namespace Wcs.Framework
{
    /// <summary>
    /// 请求的占位信号处理程序
    /// </summary>
    public abstract class RequestSignalHandler
    {
        /// <summary>
        /// 日志输出目标
        /// </summary>
        /// <param name="logTarget"></param>
        public RequestSignalHandler(LogTarget logTarget)
        {
            Logger = new Logger(this, logTarget);
        }
        /// <summary>
        /// 在新的请求保存前调用的方法
        /// </summary>
        /// <param name="request">新请求</param>
        /// <param name="signal">原始请求信号</param>
        /// <param name="unitOfWork">持久化事务</param>
        /// <param name="result">处理结果</param>
        public abstract void BeforeSave(Request request, OccupiedSignal signal, NHUnitOfWork unitOfWork, ref RequestSignalHandleResult result);
        /// <summary>
        /// 在请求被取消前调用的方法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="result">处理结果</param>
        public abstract void BeforeCancel(Request request, NHUnitOfWork unitOfWork, ref RequestSignalHandleResult result);
        /// <summary>
        /// 在请求被使用时发生
        /// </summary>
        /// <param name="request"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="result">处理结果</param>
        public abstract void Processing(Request request, NHUnitOfWork unitOfWork, ref RequestSignalHandleResult result);
        /// <summary>
        /// 日志对象
        /// </summary>
        public Logger Logger { get; private set; }
    }
}
