using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线数据包
    /// </summary>
    public class ConveyorNetPacket : NetPacket
    {
        public override byte[] StartOf
        {
            get
            {
                return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            }
        }
        public override byte[] EndOf
        {
            get
            {
                return new byte[] { 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE };
            }
        }
        public override byte[] GetBytes()
        {
            return _bytes;
        }

        byte[] _bytes;
        public ConveyorNetPacket()
        {

        }
        public ConveyorNetPacket(byte[] packageBytes)
        {
            try
            {
                if (!packageBytes.Take(12).SequenceEqual(this.StartOf))
                {
                    throw new InvalidNetPacketException(packageBytes);
                }

                this.Data = packageBytes.Skip(this.StartOf.Length).Take(packageBytes.Length - this.StartOf.Length - 4 - this.EndOf.Length).ToArray();
                this.Checksum = packageBytes.Skip(this.StartOf.Length + this.Data.Length).Take(4).ToArray();

                if (!packageBytes.Skip(this.StartOf.Length + this.Data.Length + this.Checksum.Length).SequenceEqual(this.EndOf))
                {
                    throw new InvalidNetPacketException(packageBytes);
                }

                _bytes = packageBytes;
            }
            catch (Exception ex)
            {

                throw new InvalidNetPacketException(packageBytes, ex);
            }
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
            UInt32 _checksum = 0;
            for (int i = 0; i < datapart.Length; i += 4)
            {
                byte[] bytes = new byte[4];
                if (datapart.Length > i)
                {
                    bytes[0] = datapart[i];
                }

                if (datapart.Length > i + 1)
                {
                    bytes[1] = datapart[i + 1];
                }
                else
                {
                    bytes[1] = (byte)0;
                }

                if (datapart.Length > i + 2)
                {
                    bytes[2] = datapart[i + 2];
                }
                else
                {
                    bytes[2] = (byte)0;
                }

                if (datapart.Length > i + 3)
                {
                    bytes[3] = datapart[i + 3];
                }
                else
                {
                    bytes[3] = (byte)0;
                }
                _checksum = _checksum ^ BitConverter.ToUInt32(bytes, 0);
            }

            return BitConverter.GetBytes(_checksum);
        }

        public override NetPacket CreateNetPackage(byte[] dataPart)
        {
            List<byte> datagramBytes = new List<byte>();
            datagramBytes.AddRange(this.StartOf);
            datagramBytes.AddRange(dataPart);
            datagramBytes.AddRange(this.ComputerChecksum(dataPart));
            datagramBytes.AddRange(this.EndOf);

            return new ConveyorNetPacket(datagramBytes.ToArray());
        }
    }
}
