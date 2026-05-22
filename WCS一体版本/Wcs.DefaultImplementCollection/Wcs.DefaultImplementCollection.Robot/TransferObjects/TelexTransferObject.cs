using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 表示一个报文传输对象
    /// </summary>
    [DataContract]
    public abstract class TelexTransferObject : DeviceCommand
    {
        /// <summary>
        /// 报文前缀
        /// </summary>
        public byte[] Prefix
        {
            get
            {
                return new byte[] { 0x5B };
            }
        }
        /// <summary>
        /// 报文后缀
        /// </summary>
        /// 
        public byte[] Suffix
        {
            get
            {
                return new byte[] { 0x5D };
            }
        }
    
        /// <summary>
        /// 转换为字符串形式的报文内容
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToTelex();
        /// <summary>
        /// 报文类型标识
        /// </summary>
        public abstract String TypeFlag { get; set; }
        /// <summary>
        /// 报文的长度<br />
        /// 报文长度通常固定，为了增加安全性，强制编码固定该长度，并在接收后进行报文验证。
        /// </summary>
        public abstract Int32 Length { get; }
        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public TelexTransferObject()
        {

        }
        /// <summary>
        /// 验证一个字符串形式的报文内容是否是与该类型一致
        /// </summary>
        /// <param name="telex">要进行比较的报文串</param>
        /// <returns></returns>
        public virtual Boolean ValidateType(String telex)
        {
            if (String.IsNullOrWhiteSpace(telex))
                return false;

            if (telex.Length != this.Length)
                return false;

            if (!telex.StartsWith(Prefix.ToString()) || !telex.EndsWith(Suffix.ToString()))
                return false;

            return telex.StartsWith(String.Concat(Prefix, this.TypeFlag));
        }
        /// <summary>
        /// 验证一个字符串形式的报文内容是否是与该类型一致
        /// </summary>
        /// <param name="telex">要进行比较的报文串</param>
        /// <returns></returns>
        public virtual Boolean ValidateType(byte[] bytes)
        {
            if (bytes == null)
                return false;

            if (bytes.Length != this.Length)
                return false;

            return true;
        }

        public override string ToString()
        {
            return ToTelex().ToString();
        }
    }
}
