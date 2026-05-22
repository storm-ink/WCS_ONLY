using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 数据接收器接口
    /// </summary>
    public interface IDataReceiver
    {
        /// <summary>
        /// 设备名
        /// </summary>
        string DeviceName { get; set; }
        /// <summary>
        /// 设备
        /// </summary>
        Device Device { get; }
        /// <summary>
        /// 接收器名称
        /// </summary>
        String Name { get; }
        /// <summary>
        /// 将数据包转换为网络传输对象（协议对象）
        /// </summary>
        /// <param name="netPacket">数据包</param>
        /// <returns></returns>
        TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject;
        /// <summary>
        /// 将网络传输对象（协议对象）转换为数据包
        /// </summary>
        /// <param name="netTransferObject">网络传输对象（协议对象）</param>
        /// <returns></returns>
        NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject;
        /// <summary>
        /// 向数据接收器添加接收到的数据
        /// </summary>
        /// <param name="bytes">接收到的数据</param>
        void AddBytes(byte[] bytes);
        /// <summary>
        /// 清空数据接收器
        /// </summary>
        void Clear();
        /// <summary>
        /// 当接收到新的数据时发生
        /// </summary>
        event EventHandler<DataReceiverReceivedEventArgs> DataReceived;

        void Log(string msg);
    }
}
