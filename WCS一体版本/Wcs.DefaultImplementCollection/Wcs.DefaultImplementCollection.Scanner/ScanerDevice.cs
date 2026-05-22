using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Scanner
{
    [DisplayName("条码扫描")]
    public class ScanerDevice : TcpProtocolDevice
    {
        /// <summary>
        /// 状态标志位 手动触发穿梭车扫码器扫码时 需要先将其置位为 false ，只要扫码器收到扫码结果 就立即置位为 true
        /// </summary>
        public Boolean ManualState = false;

        public event EventHandler<BarcodeReceivedArgs> BarcodeReceived;

        public String CurrentBarcode { get; set; }

        public Dictionary<String, String> Barcodes { get; set; }
        /// <summary>
        /// 绑定到的位置（可转换的位置编码）
        /// </summary>
        public String BindingLocation { get; set; }



        public ScanerDevice(string name, int no, int receiveTimeout, int connectTimeout, int sendTimeout, IPEndPoint ipEndPoint, IDataReceiver dataReceiver, string bindingLocation)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, ipEndPoint,null, dataReceiver)
        {
            Barcodes = new Dictionary<string, string>();
            this.BindingLocation = bindingLocation;
            this.CurrentBarcode = "";
        }

        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            ScanerDeviceTelexTransferObject obj = (ScanerDeviceTelexTransferObject)netTransferObject;
            String barcode = obj.ToString().Trim();

            Barcodes.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff"), barcode);

            Barcodes = (from bar in Barcodes
                        orderby bar.Key ascending
                        select bar).ToDictionary(x => x.Key, x => x.Value);

            if (Barcodes.Count > 10)
            {
                Barcodes.Remove(Barcodes.First().Key);
            }

            CurrentBarcode = barcode;

            if (BarcodeReceived != null)
            {
                System.Threading.ThreadPool.QueueUserWorkItem((stat) =>
                {
                    BarcodeReceived(this, new BarcodeReceivedArgs(barcode));
                });
            }

            //持续给PLC发送相同data_ID的command指令，直到电气给出占位
            //需要增加条码判断，如果条码是符合条件的那么给电气写一个扫码成功的值，如果不成功则给电气写一个扫码不成功的值
            //1表示成功，2表示不成功
            //ScanerResultCommand lastCommand = new ScanerResultCommand();
            //do
            //{
            //    if (this.OwnerDevice == null || this.OwnerLocation == null)
            //    {
            //        this.OwnerDevice = (ConveyorDevice)LocationConverter.ConvertibleCodeToLcation(BindingLocation).Device;
            //        this.OwnerLocation = (ConveyorLocation)LocationConverter.ConvertibleCodeToLcation(BindingLocation);
            //    }


            //    UInt16 index = (ushort)OwnerDevice.ReadStatus<ScanerTransferObject>().Where(x => x.ScanerNo.ToString() == OwnerLocation.DeviceCode).FirstOrDefault().AtPacketIndex;
            //    if (lastCommand.Data_Id == 0)
            //    {
            //        ScanerResultCommand cmd = new ScanerResultCommand(1, 1, index);
            //        lastCommand = cmd;
            //    }
            //    OwnerDevice.Write(lastCommand, (device, _cmd) =>
            //    {
            //        return OwnerDevice.ReadStatus<ScanerTransferObject>().Where(x =>
            //            x.ScanerNo.ToString() == this.OwnerLocation.DeviceCode
            //            && x.ScanResult == 1
            //            ).FirstOrDefault() != null;
            //    });
            //    Thread.Sleep(1000);
            //} while (this.CurrentBarcode != "");
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new ScanerDeviceUserInterface(this);
        }

        public override IsIdleResult IsIdle
        {

            get { return IsConnected? new IsIdleResult(true, ""): new IsIdleResult(false, "未连接"); ; }
        }

        public override string[] Warnings
        {
            get { return new String[0]; }
        }
    }
}
