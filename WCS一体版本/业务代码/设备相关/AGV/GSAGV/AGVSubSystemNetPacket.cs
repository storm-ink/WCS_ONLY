using System;
using System.Collections.Generic;
using Wcs.Framework;


namespace BOE.设备相关.AGV.GSAGV
{
    /// <summary>
    /// 定义该对象只为套用系统结构
    /// </summary>
    public class AGVSubSystemNetPacket : NetPacket
    {
        public override byte[] StartOf
        {
            get
            {
                return new byte[0];
            }
        }
        public override byte[] EndOf
        {
            get
            {
                return new byte[0];
            }
        }
        public override byte[] GetBytes()
        {
            return _bytes;
        }

        byte[] _bytes;
        public AGVSubSystemNetPacket()
        {

        }
        public AGVSubSystemNetPacket(byte[] packageBytes)
        {
            this.Data = packageBytes;
            this.Checksum = new byte[0];

            _bytes = packageBytes;
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
        public override byte[] ComputerChecksum(byte[] datapart)
        {
            throw new NotImplementedException();
        }

        public override NetPacket CreateNetPackage(byte[] dataPart)
        {
            List<byte> datagramBytes = new List<byte>();
            datagramBytes.AddRange(this.StartOf);
            datagramBytes.AddRange(dataPart);
            datagramBytes.AddRange(this.ComputerChecksum(dataPart));
            datagramBytes.AddRange(this.EndOf);

            return new AGVSubSystemNetPacket(datagramBytes.ToArray());
        }
    }
}
