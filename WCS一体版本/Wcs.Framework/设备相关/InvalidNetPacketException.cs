using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个无效的网络数据包异常
    /// </summary>
    [Serializable]
    public class InvalidNetPacketException:Exception
    {
        /// <summary>
        /// 无效的数据包数据
        /// </summary>
        public byte[] InvalidNetPacketBytes { get; private set; }

        public InvalidNetPacketException(byte[] invalidNetPacketBytes)
            : this(invalidNetPacketBytes, null)
        {

        }

        public InvalidNetPacketException(byte[] invalidNetPacketBytes,Exception innerException)
            : base("无效的网络数据包",innerException)
        {
            this.InvalidNetPacketBytes = invalidNetPacketBytes;
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
