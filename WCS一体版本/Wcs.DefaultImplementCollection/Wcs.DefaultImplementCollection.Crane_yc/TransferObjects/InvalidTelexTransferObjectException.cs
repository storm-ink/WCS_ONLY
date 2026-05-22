using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个无效的报文传输对象异常
    /// </summary>
    public class InvalidTelexTransferObjectException<TTelexTransferObject>:Exception
        where TTelexTransferObject:TelexTransferObject
    {
        /// <summary>
        /// 无效的报文内容
        /// </summary>
        public String InvalidTelex { get; private set; }

        public InvalidTelexTransferObjectException(String invalidTelex)
            : this(invalidTelex, null)
        {

        }

        public InvalidTelexTransferObjectException(String invalidTelex, Exception innerException)
            : base(string.Format("{0} 无法转换为 {1} 类型",invalidTelex,typeof(TTelexTransferObject)),innerException)
        {
            this.InvalidTelex = invalidTelex;
        }
    }
}
