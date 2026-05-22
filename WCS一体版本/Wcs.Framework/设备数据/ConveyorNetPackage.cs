using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 输送线网络数据包
    /// </summary>
    public class ConveyorNetPackage : NetPackage
    {
        public override NetPacket FetchPackage()
        {
            lock (_bytes)
            {
                if (_bytes.Count < 8)
                {
                    return null;
                }

                int headerIndex = -1;
                int endIndex = -1;
                byte[] byteArray = _bytes.ToArray();
                for (int i = 0; i < byteArray.Length; /*i += 1*/)
                {
                    if (headerIndex == -1)
                    {
                        while (i < byteArray.Length - 1 && byteArray[i] != 255)
                        {
                            i++;
                        }

                        if (byteArray.Skip(i).Count() < 8)
                        {
                            return null;
                        }

                        //8个字节如果都是f，那说明是开头(对应64位uint)
                        if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'f'))
                        {
                            headerIndex = i;
                            i += 8;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        while (i < byteArray.Length - 1 && byteArray[i] != 238)
                        {
                            i++;
                        }

                        if (byteArray.Skip(i).Count() < 8)
                        {
                            return null;
                        }

                        //8个字节如果都是e，那说明是开头(对应64位uint)
                        if (BitConverter.ToUInt64(byteArray, i).ToString("x") == "".PadLeft(16, 'e'))
                        {
                            endIndex = i;
                            i += 8;
                            while (i < byteArray.Length && byteArray[i] == 238)
                            {
                                endIndex++;
                                i++;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (headerIndex != -1 && endIndex != -1)
                    {
                        int contentStartIndex = headerIndex + 8;
                        int contentEndIndex = endIndex;
                        //byte[] result = byteArray.Skip(contentStartIndex)
                        //    .Take(contentEndIndex - contentStartIndex)
                        //    .ToArray();
                        byte[] result = byteArray.Skip(headerIndex)
                           .Take(endIndex + 8 - headerIndex)
                           .ToArray();

                        _bytes = byteArray.Skip(endIndex + 8).ToList();


                        return new ConveyorDatagram(result);
                    }
                }

                return null;
            }
        }

        public override NetPacket CreateSendPackage(byte[] dataPart)
        {
            UInt32 _checksum = CreateChecksum(ConveyorDatagram.CreateDatagram(dataPart));

            List<byte> result = new List<byte>();
            //头
            result.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            //数据
            result.AddRange(dataPart);
            //检验码
            result.AddRange(BitConverter.GetBytes(_checksum));
            //尾
            result.AddRange(new byte[] { 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE, 0xEE });

            return new ConveyorDatagram(result.ToArray());
        }

        public bool CheckPackage(ConveyorDatagram datagram)
        {
            uint checksum = GetChecksum(datagram);


            UInt32 _checksum = CreateChecksum(datagram);

            return checksum == _checksum;
        }

        /// <summary>
        /// 计算指定 byte 数组的校验码
        /// </summary>
        /// <param name="packageDataPart">
        /// 数据部分 bytes <br />
        /// 获取方法：<br />
        /// 完整的包去头去尾去校验码的数据部分 起始为 8(8 bytes 起始符) 结束为包大小减12(8 bytes 结束符，4 bytes 校验码)
        /// </param>
        /// <returns></returns>
        public UInt32 CreateChecksum(ConveyorDatagram datagram)
        {
            UInt32 _checksum = 0;
            for (int i = 0; i < datagram.Data.Length; i += 4)
            {
                byte[] bytes = new byte[4];
                if (datagram.Data.Length > i)
                {
                    bytes[0] = datagram.Data[i];
                }

                if (datagram.Data.Length > i + 1)
                {
                    bytes[1] = datagram.Data[i + 1];
                }
                else
                {
                    bytes[1] = (byte)0;
                }

                if (datagram.Data.Length > i + 2)
                {
                    bytes[2] = datagram.Data[i + 2];
                }
                else
                {
                    bytes[2] = (byte)0;
                }

                if (datagram.Data.Length > i + 3)
                {
                    bytes[3] = datagram.Data[i + 3];
                }
                else
                {
                    bytes[3] = (byte)0;
                }
                _checksum = _checksum ^ BitConverter.ToUInt32(bytes, 0);
            }

            return _checksum;
        }
    }
}
