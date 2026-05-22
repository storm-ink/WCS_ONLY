using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane
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
      //  public const Char Prefix = (Char)2;
        public byte[] Prefix
        {
            get
            {
                return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            }
        }
        /// <summary>
        /// 报文后缀
        /// </summary>
        public const Char Suffix = (Char)3;
        /// <summary>
        /// 转换为字符串形式的报文内容
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToTelex();
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

            return bytes.Select(x => Convert.ToString("X2")).ToString().StartsWith(Prefix.ToString());
        }

        /// <summary>
        /// 将对象转换为可以写入网络流的的 byte 数组
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            //string telex = ToTelex();
            //telex = telex.PadRight(30, '0');
            //byte[] bytes = Encoding.ASCII.GetBytes(telex);

            byte[] telex = ToTelex();

            return telex;
        }

        public override string ToString()
        {
            return ToTelex().ToString();
        }
        public static byte[] GetColumnsRowLevelData(string Location)
        {
            try
            {
                List<byte> list = new List<byte>();
                Location = Location.Trim();
                Location = Location.PadLeft(3, '0');
                list.Add(byte.Parse(Location.Trim().Substring(0, 1)));
                list.Add(byte.Parse(Location.Trim().Substring(1, 1)));
                list.Add(byte.Parse(Location.Trim().Substring(2, 1)));
                return list.ToArray();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public static byte[] SendconvertNumberToBytes(object value)
        {
            var basicType = value.GetType().BaseType.Name;
            if (basicType == "Enum")
                return BitConverter.GetBytes(Convert.ToInt16(value)).Reverse().ToArray();

            var valueType = value.GetType().Name;
            if (valueType == "Int16")
                return BitConverter.GetBytes(Convert.ToInt16(value)).Reverse().ToArray();

            if (valueType == "Int32")
                return BitConverter.GetBytes(Convert.ToInt32(value)).Reverse().ToArray();

            if (valueType == "Int64")
                return BitConverter.GetBytes(Convert.ToInt64(value)).Reverse().ToArray();

            if (valueType == "UInt16")
                return BitConverter.GetBytes(Convert.ToUInt16(value)).Reverse().ToArray();

            if (valueType == "UInt32")
                return BitConverter.GetBytes(Convert.ToUInt32(value)).Reverse().ToArray();

            if (valueType == "UInt64")
                return BitConverter.GetBytes(Convert.ToUInt64(value)).Reverse().ToArray();

            if (valueType == "Byte")
                return new Byte[] { Convert.ToByte(value) };

            if (valueType == "String" || valueType == "string")
                return System.Text.ASCIIEncoding.ASCII.GetBytes((string)value);

            throw new NotSupportedException(String.Format("不支持将 {0} 转换为 byte[]", valueType));
        }
    }
}
