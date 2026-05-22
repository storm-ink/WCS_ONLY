using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 表示该类的子类是一个子系统通讯报文对象
    /// </summary>
    [DataContract]
    public abstract class AGVSubSystemTelexTransferObject : DeviceCommand
    {
        /// <summary>
        /// 报文前缀
        /// </summary>
        public const Char Prefix = (Char)2;
        /// <summary>
        /// 报文后缀
        /// </summary>
        public const Char Suffix = (Char)3;
        /// <summary>
        /// 转换为字符串形式的报文内容
        /// </summary>
        /// <returns></returns>
        public abstract String ToTelex();
        /// <summary>
        /// 报文类型标识
        /// </summary>
        public abstract String TypeFlag { get; }
        /// <summary>
        /// 报文的长度<br />
        /// 堆垛机的报文长度通常固定，为了增加安全性，强制编码固定该长度，并在接收后进行报文验证。
        /// </summary>
        public abstract Int32 Length { get; }
        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public AGVSubSystemTelexTransferObject()
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
            {
                return false;
            }

            if (telex.Length != this.Length)
            {
                return false;
            }

            if (!telex.StartsWith(Prefix.ToString()) || !telex.EndsWith(Suffix.ToString()))
            {
                return false;
            }

            return telex.StartsWith(String.Concat(Prefix, this.TypeFlag));
        }

        /// <summary>
        /// 将对象转换为可以写入网络流的的 byte 数组
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            string telex = ToTelex();
            byte[] bytes = Encoding.ASCII.GetBytes(telex);

            return bytes;
        }

        public override string ToString()
        {
            return ToTelex();
        }


    }
}
