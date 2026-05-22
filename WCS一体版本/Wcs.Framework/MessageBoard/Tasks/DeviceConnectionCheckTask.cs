using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.MessageBoard.Cfg;

namespace Wcs.Framework.MessageBoard.Tasks
{
    public class DeviceConnectionCheckTask:AbstractMessageBoardTask
    {
        public DeviceConnectionCheckTask(MessageBoardTaskElement element)
            : base(element)
        {
            MessageLevel = element.GetAttributeOrDefault<MessageLevel>("level", Framework.MessageBoard.MessageLevel.Warning);
        }

        public MessageLevel MessageLevel { get; private set; }
        protected override AbstractMessage[] FetchMessages()
        {
            var devices = Wcs.Framework
                .Cfg.WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Select(x => x.Device)
                .Where(x => !x.IsConnected);

            String names = String.Join("、", devices.Select(x => x.Name));

            if (names.Length > 0)
            {
                var msg = new Messages.TipMessage(MessageLevel, "设备连接状态检测", names + "未连接。", null);

                return new AbstractMessage[] { msg };
            }
            else
            {
                return new AbstractMessage[0];
            }
        }
    }
}
