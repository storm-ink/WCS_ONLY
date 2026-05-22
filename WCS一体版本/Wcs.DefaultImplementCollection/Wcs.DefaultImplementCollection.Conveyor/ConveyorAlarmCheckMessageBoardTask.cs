using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.MessageBoard;
using Wcs.Framework.MessageBoard.Cfg;
using Wcs.Framework.MessageBoard.Messages;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ConveyorAlarmCheckMessageBoardTask : Wcs.Framework.MessageBoard.AbstractMessageBoardTask
    {
        public ConveyorAlarmCheckMessageBoardTask(MessageBoardTaskElement element)
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
                   .Where(x => x is ConveyorDevice)
                   .Select(x => x as ConveyorDevice)
                   .Where(x => x.MachineAlarms != null && x.MachineAlarms.Length > 0);

            List<AbstractMessage> messages = new List<AbstractMessage>();
            foreach (var item in devices)
            {
                String names = String.Join("、", item.MachineAlarms.Where(x => x.Manual).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位处于手动模式。", null);
                    messages.Add(msg);
                    continue;
                }

                names = String.Join("、", item.MachineAlarms.Where(x => x.Isolator).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位隔离开关断开。", null);
                    messages.Add(msg);
                    continue;
                }

                names = String.Join("、", item.MachineAlarms.Where(x => x.Breaker).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位断路器断开。", null);
                    messages.Add(msg);
                    continue;
                }


                names = String.Join("、", item.MachineAlarms.Where(x => x.TaskNoGoods).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位有任务无货。", null);
                    messages.Add(msg);
                    continue;
                }

                names = String.Join("、", item.MachineAlarms.Where(x => x.Photocell).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位光电异常。", null);
                    messages.Add(msg);
                    continue;
                }

                names = String.Join("、", item.MachineAlarms.Where(x => x.RunOvertime).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位运行超时。", null);
                    messages.Add(msg);
                    continue;
                }

                names = String.Join("、", item.MachineAlarms.Where(x => x.OccupyOvertime).Select(x => x.PosNo.ToString()));
                if (names.Length > 0)
                {
                    var msg = new TipMessage(MessageLevel, item.Name, names + "货位占位超时。", null);
                    messages.Add(msg);
                    continue;
                }
            }

            return messages.ToArray();
        }
    }
}
