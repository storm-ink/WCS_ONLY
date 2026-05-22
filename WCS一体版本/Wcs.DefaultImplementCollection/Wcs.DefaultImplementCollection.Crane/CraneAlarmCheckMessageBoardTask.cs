using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.MessageBoard;
using Wcs.Framework.MessageBoard.Cfg;
using Wcs.Framework.MessageBoard.Messages;

namespace Wcs.DefaultImplementCollection.Crane
{
    public class CraneAlarmCheckMessageBoardTask : Wcs.Framework.MessageBoard.AbstractMessageBoardTask
    {
        public CraneAlarmCheckMessageBoardTask(MessageBoardTaskElement element)
            : base(element)
        {
            MessageLevel = element.GetAttributeOrDefault<MessageLevel>("level", Framework.MessageBoard.MessageLevel.Warning);
        }

        public MessageLevel MessageLevel { get; private set; }
        protected override Framework.MessageBoard.AbstractMessage[] FetchMessages()
        {
            var devices = Wcs.Framework
                .Cfg.WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Select(x => x.Device)
                .Where(x => x is CraneDevice)
                .Select(x => x as CraneDevice)
                .Where(x => x.LastStatus != null);

            List<AbstractMessage> messages = new List<AbstractMessage>();
            foreach (var dev in devices)
            {
                if (dev.LastStatus.CraneWorkModel != CraneWorkModels.Auto)
                {
                    messages.Add(new TipMessage(MessageLevel, dev.Name, "非自动模式。", null));
                    continue;
                }

                if (dev.LastStatus.DeviceState == CraneStatus.AlarmDown)
                {
                    if (dev.LastStatus.ErrorCodeList.Count() > 0)
                    {
                        List<string> alarms = new List<string>();
                        foreach (var item in dev.LastStatus.ErrorCodeList)
                        {
                            var alarm = DeviceErrorHelper.GetDeviceErrorFromErrorCode("堆垛机", item);
                            if (alarm == null)
                                alarms.Add($"未登记故障({item})");
                            else
                                alarms.Add($"{alarm.ErrorName}");
                        }
                        messages.Add(new TipMessage(MessageLevel, dev.Name, "已报警停机。", string.Join("|", alarms)));
                    }
                    else
                    {
                        messages.Add(new TipMessage(MessageLevel, dev.Name, "已报警停机。", null));
                    }
                    continue;
                }

                if (dev.LastStatus.DeviceState == CraneStatus.RemoteEmergency)
                {
                    messages.Add(new TipMessage(MessageLevel, dev.Name, "已远程急停。", null));
                    continue;
                }
            }

            return messages.ToArray();
        }
    }
}
