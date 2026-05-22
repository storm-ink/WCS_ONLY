using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.MessageBoard.Messages
{
    /// <summary>
    /// 表示一个设备被锁定的消息，在设备被解锁后消息将过期
    /// </summary>
    public sealed class LockedMessage : AbstractMessage
    {
        Device _device;
        public LockedMessage(Device device)
            : base(MessageLevel.Warning, device.Name, String.Format("被 {0} 锁定",device.Locker.UserName), null)
        {
            _device = device;
        }

        /// <summary>
        /// 在设备被解锁后消息将过期。
        /// </summary>
        public override bool IsOverdued
        {
            get
            {
                return _device.Locker.IsEmpty;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
