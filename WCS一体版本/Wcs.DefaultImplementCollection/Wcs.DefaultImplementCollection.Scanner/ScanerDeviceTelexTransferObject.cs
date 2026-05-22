using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NLog;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    /// <summary>
    /// 表示一个报文传输对象
    /// </summary>
    [DataContract]///如果报错需要手动引用4.0.0.0版本的System.Model和System.Runtime.Serialization
    public class ScanerDeviceTelexTransferObject : NetTransferObject
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        byte[] _data;
        /// <summary>
        /// 报文前缀
        /// </summary>
        public const Char Prefix = (Char)2;

        /// <summary>
        /// 报文后缀
        /// </summary>
        public const Char Suffix = (Char)3;
        /// <summary>
        /// 默认无参的构造函数
        /// </summary>
        public ScanerDeviceTelexTransferObject()
        { 
            _data=new byte[0];
        }

        public ScanerDeviceTelexTransferObject(byte[] dataPart)
            : this()
        {
            if (dataPart.First() == Prefix && dataPart.Last() == Suffix)
            {
                _data = dataPart.Skip(1).Take(dataPart.Length - 2).ToArray();
            }
            else
                _data = dataPart;
        }

        /// <summary>
        /// 将对象转换为可以写入网络流的 byte 数组
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes(new Char[] { Prefix })
                .Concat(_data)
                .Concat(System.Text.Encoding.ASCII.GetBytes(new Char[] { Suffix }))
                .ToArray();
        }

        public override string ToString()
        {
            string str = "";
            foreach (var bytes in _data)
            {
                str += bytes+",";
            }
            //Console.WriteLine(str);

            _logger.Info(string.Format("接收到扫码器发来的报文：{0}{1}",str,DateTime.Now));
            //return System.Text.Encoding.Default.GetString(_data);
            //return System.Text.Encoding.ASCII.GetString(_data);
            return System.Text.Encoding.UTF8.GetString(_data);
        }

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        //public override ReceivedDataLog ToLogData(Device device)
        //{
        //    return new ScanerDeviceTelexTransferObjectLogData(device, this);
        //}
    }
}
