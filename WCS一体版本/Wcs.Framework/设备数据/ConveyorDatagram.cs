using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线数据报
    /// </summary>
    public class ConveyorDatagram : Datagram
    {
        public override byte[] StartOf { get; protected set; }

        public override byte[] Data { get; protected set; }

        public override byte[] Checksum { get; protected set; }

        public override byte[] EndOf { get; protected set; }

        public override byte[] GetBytes()
        {
            return _bytes;
        }

        byte[] _bytes;
        public ConveyorDatagram(byte[] bytes)
        {
            _bytes = bytes;
            this.StartOf = _bytes.Take(8).ToArray();
            this.Data = _bytes.Skip(this.StartOf.Length).Take(_bytes.Length - 20).ToArray();
            this.Checksum = _bytes.Skip(this.StartOf.Length + this.Data.Length).Take(4).ToArray();
            this.EndOf = _bytes.Skip(this.StartOf.Length + this.Data.Length + this.Checksum.Length).ToArray();
        }

        /// <summary>
        /// 创建一个完整的数据包结构
        /// </summary>
        /// <param name="dataPart">包的数据部分</param>
        /// <returns></returns>
        public static ConveyorDatagram CreateDatagram(byte[] dataPart)
        {
            List<byte> datagramBytes = new List<byte>();
            datagramBytes.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            datagramBytes.AddRange(dataPart);
            datagramBytes.AddRange(new byte[] { 0, 0, 0, 0 });
            datagramBytes.AddRange(new byte[] { 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE });

            return new ConveyorDatagram(datagramBytes.ToArray());
        }
        /// <summary>
        /// 获取数据报长度
        /// </summary>
        public override int Length
        {
            get
            {
                return this.StartOf.Length + this.Data.Length + this.Checksum.Length + this.EndOf.Length;
            }
        }
    }
}
