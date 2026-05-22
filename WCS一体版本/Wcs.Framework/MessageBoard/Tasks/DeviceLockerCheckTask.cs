using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.MessageBoard.Cfg;

namespace Wcs.Framework.MessageBoard.Tasks
{
    public class DeviceLockerCheckTask:AbstractMessageBoardTask
    {
        public DeviceLockerCheckTask(MessageBoardTaskElement element)
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
                .Where(x => x.Locker!=null && !x.Locker.IsEmpty);

            List<AbstractMessage> messages = new List<AbstractMessage>();

            foreach (var grouping in devices.GroupBy(x=>x.Locker.UserName))
            {
                String names = String.Join("、", grouping.Select(x => x.Name));

                var msg = new Messages.TipMessage(MessageLevel, "设备连接状态检测", names + "被" + grouping.Key + "锁定。", null);
                messages.Add(msg);
            }

            return messages.ToArray();
        }
    }
}
