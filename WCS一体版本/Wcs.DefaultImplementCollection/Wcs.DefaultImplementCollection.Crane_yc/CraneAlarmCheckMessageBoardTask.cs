using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.MessageBoard;
using Wcs.Framework.MessageBoard.Cfg;
using Wcs.Framework.MessageBoard.Messages;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class CraneAlarmCheckMessageBoardTask:Wcs.Framework.MessageBoard.AbstractMessageBoardTask
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
                if (dev.LastStatus.State == CraneStatus.ResetAlarm || dev.LastStatus.State == CraneStatus.AlarmAndShutdown)
                {
                    if (dev.LastStatus.ErrorCodeList.Count() > 0)
                    {
                        foreach (var item in dev.LastStatus.ErrorCodeList)
                        {
                            var alarm = Wcs.Framework.DeviceErrorHelper.GetDeviceErrorFromErrorCode("堆垛机", item);
                            messages.Add(new TipMessage(MessageLevel, dev.Name, "已报警停机。", alarm.ErrorName));
                        }
                    }
                    else
                    {
                        messages.Add(new TipMessage(MessageLevel, dev.Name, "已报警停机。", null));
                    }
                    continue;
                }

                if (dev.LastStatus.Event == CraneEvent.EmergencyStop)
                {
                    if (dev.LastStatus.ErrorCodeList.Count() > 0)
                    {
                        foreach (var item in dev.LastStatus.ErrorCodeList)
                        {
                            var alarm = Wcs.Framework.DeviceErrorHelper.GetDeviceErrorFromErrorCode("堆垛机", item);
                            messages.Add(new TipMessage(MessageLevel, dev.Name, "已急停。", alarm.ErrorName));
                        }
                    }
                    else
                    {
                        messages.Add(new TipMessage(MessageLevel, dev.Name, "已急停。", null));
                    }
                    continue;
                }

                if (dev.LastStatus.Event == CraneEvent.Initialized && dev.LastStatus.State==CraneStatus.Initialized)
                {
                    messages.Add(new TipMessage(MessageLevel, dev.Name, "需要回原点后才能执行任务。", null));
                    continue;
                }
            }

            return messages.ToArray();
        }
    }
}
