using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS.APP
{/// <summary>
 /// 进程间通讯，共享映射文件
 /// </summary>
    public class MappingHelper
    {
        long capacity = 1024 * 1024;
        public MemoryMappedFile file;

        public MappingHelper()
        {
        }

        public MappingHelper(string fileName, string mapName)
        {
            file = MemoryMappedFile.CreateFromFile(fileName, FileMode.OpenOrCreate, mapName, capacity);
        }

        /// <summary>
        /// 将消息写入共享映射文件
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void WriteString(string msg)
        {
            using (var stream = file.CreateViewStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(msg);
                }
            }
        }
        /// <summary>
        /// 从共享映射文件中读取消息
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            using (var stream = file.CreateViewStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    List<byte> bytes = new List<byte>();
                    byte[] temp = new byte[1024];
                    while (true)
                    {
                        int readCount = reader.Read(temp, 0, temp.Length);
                        if (readCount == 0)
                        {
                            break;
                        }
                        for (int i = 0; i < readCount; i++)
                        {
                            bytes.Add(temp[i]);
                        }
                    }
                    if (bytes.Count > 0)
                    {
                        return Encoding.Default.GetString(bytes.ToArray()).Replace("\0", "");
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
