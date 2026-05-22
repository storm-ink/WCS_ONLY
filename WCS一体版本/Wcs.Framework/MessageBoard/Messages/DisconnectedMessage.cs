using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.MessageBoard.Messages
{
    /// <summary>
    /// 表示一个设备已断开的消息，当设备再次连接成功后，它将已经失效。
    /// </summary>
    public sealed class DisconnectedMessage : AbstractMessage
    {
        Device _device;
        public DisconnectedMessage( Device device)
            : base(MessageLevel.Warning, device.Name, "未连接", null)
        {
            _device = device;
        }

        /// <summary>
        /// 当设备再次连接成功后，它将已经失效。
        /// </summary>
        public override bool IsOverdued
        {
            get
            {
                return _device.IsConnected;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
