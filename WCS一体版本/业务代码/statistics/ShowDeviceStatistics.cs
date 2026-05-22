using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.Statistics
{
    /// <summary>
    /// 设备状态统计
    /// </summary>
    [Description("设备状态统计")]
    public class ShowDeviceStatistics
    {
        public string Item { get; set; }

        public string StartAt { get; set; } = "-";

        public string EndAt { get; set; } = "-";

        public string Total { get; set; } = "-";
        public string RunningValue { get; set; } = "00:00:00";

        public string RunningRate { get; set; } = "0%";

        public string AlarmValue { get; set; } = "00:00:00";
        public string AlarmRate { get; set; } = "0%";
        public string UnAlarmRate { get; set; } = "0%";

        public string RepairValue { get; set; } = "00:00:00";
        public string RepairRate { get; set; } = "0%";

        public string IdleValue { get; set; } = "00:00:00";

        public string IdleRate { get; set; } = "0%";

        public string DisconnectedValue { get; set; } = "00:00:00";
        public string DisconnectedRate { get; set; } = "0%";

        public string ManualValue { get; set; } = "00:00:00";
        public string ManualRate { get; set; } = "0%";

        public string Activation { get; set; } = "0%";
        public string OKRate { get; set; } = "0%";

        public string Mark { get; set; }
    }
}
