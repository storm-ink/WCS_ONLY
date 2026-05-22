using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    /// <summary>
    /// 基恩士扫码命令
    /// </summary>
    public class KEYENCECommand : DeviceCommand,IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 空构造函数
        /// </summary>
        public KEYENCECommand(ScanerDevice scanner)
        {
            device = scanner;
            CreateAt = DateTime.Now;
        }

        public byte[] Bytes = new byte[] { 76, 79, 78, 13 };
        DateTime CreateAt;
        ScanerDevice device;

        public override object this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            //return device.Barcodes.Any(x => x.Key <= this.CreateAt);
            return true;
        }
    }
}
