using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public enum RequestProcessResultStatus
    {
        /// <summary>
        /// 未执行任何加工操作
        /// </summary>
        Unprocessed,
        /// <summary>
        /// 已加工，并正常结束
        /// </summary>
        Processed,
        /// <summary>
        /// 请求调用方法取消对 Request 的操作。（中止调用）
        /// </summary>
        Abort,
    }
    /// <summary>
    /// Request 对象包装结果
    /// </summary>
    public class RequestProcessResult
    {
        /// <summary>
        /// 指示包装过程的结果
        /// </summary>
        public RequestProcessResultStatus Result { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public String Description { get; set; }
    }
    /// <summary>
    /// Request 包装对象
    /// </summary>
    public interface IRequestProcess
    {
        /// <summary>
        /// 执行包装过程
        /// </summary>
        /// <param name="request">需要包装的 Request 对象</param>
        /// <param name="sesson">执行化上下文</param>
        /// <param name="result">包装结果</param>
        void Process(Request request,NHibernate.ISession sesson,RequestProcessResult result);
    }
}
