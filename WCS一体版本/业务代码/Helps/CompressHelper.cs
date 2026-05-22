using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public static class CompressHelper
    {
        /// <summary>
        /// 使用Zip压缩算法压缩指定的字节数组
        /// </summary>
        /// <param name="bytes">目标字节数组</param>
        /// <returns></returns>
        public static byte[] ZipCompress(byte[] bytes)
        {
            using (var compressStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }

                return compressStream.ToArray();
            }
        }

        /// <summary>
        /// 使用Zip压缩算法解缩指定的字节数组
        /// </summary>
        /// <param name="compressbytes">目标压缩字节数组</param>
        /// <returns></returns>
        public static byte[] ZipDecompress(byte[] compressbytes)
        {
            using (var compressStream = new MemoryStream(compressbytes))
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}
