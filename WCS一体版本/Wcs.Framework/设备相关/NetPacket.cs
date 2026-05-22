using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 网络数据包
    /// </summary>
    public abstract class NetPacket
    {
        /// <summary>
        /// 起始符数据
        /// </summary>
        public virtual byte[] StartOf { get; private set; }
        /// <summary>
        /// 数据部分
        /// </summary>
        public virtual byte[] Data { get; protected set; }
        /// <summary>
        /// 校验码数据
        /// </summary>
        public virtual byte[] Checksum { get; protected set; }
        /// <summary>
        /// 结束符数据
        /// </summary>
        public virtual byte[] EndOf { get; private set; }
        /// <summary>
        /// 获取数据包的所有 byte 集合<br />
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
        /// <summary>
        /// 计算当前数据包的检验码
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ComputerChecksum();
        /// <summary>
        /// 创建一个完整的数据包
        /// </summary>
        /// <param name="dataPart">数据区字节集合</param>
        /// <returns>完整的带验证码的数据包对象</returns>
        public  abstract NetPacket CreateNetPackage(byte[] dataPart);

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NetPacket packet = obj as NetPacket;
            if (packet==null)
            {
                return false;
            }

            return this.Data.SequenceEqual(packet.Data);
        }

        /// <summary>
        /// 判断两个 NetPacket 是否相等。<br />
        /// 该方法已被重写，并改变了最基本的判等含义。<br />
        /// 此处只判断两个参于对比的 NetPacket 的 Data 部分 byte 是否一致。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator ==(NetPacket a,NetPacket b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static Boolean operator !=(NetPacket a, NetPacket b)
        {
            return !(a == b);
        }
    }
}
