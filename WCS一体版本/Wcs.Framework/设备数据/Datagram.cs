using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 数据报
    /// </summary>
    public abstract class Datagram
    {
        /// <summary>
        /// 起始符数据
        /// </summary>
        public abstract byte[] StartOf { get; protected set; }
        /// <summary>
        /// 数据部分
        /// </summary>
        public abstract byte[] Data { get; protected set; }
        /// <summary>
        /// 校验码数据
        /// </summary>
        public abstract byte[] Checksum { get; protected set; }
        /// <summary>
        /// 结束符数据
        /// </summary>
        public abstract byte[] EndOf { get; protected set; }
        /// <summary>
        /// 获取数据报的所有 byte 集合<br />
        /// 默认使用 头+数据+校验+尾 的拼接方式
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            return StartOf
                .Concat(Data)
                .Concat(Checksum)
                .Concat(EndOf)
                .ToArray();
        }
        /// <summary>
        /// 获取数据报的长度
        /// </summary>
        public virtual int Length
        {
            get
            {
                return this.StartOf.Length + this.Data.Length + this.Checksum.Length + this.EndOf.Length;
            }
        }
    }
}
