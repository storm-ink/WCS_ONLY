using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    /// <summary>
    /// 监控数据
    /// </summary>
    public class StatisticsDto
    {
        /// <summary>
        /// 面板
        /// </summary>
        [JsonProperty("tables")]
        public List<TableDto> Tables { get; set; }

        #region 报警
        /// <summary>
        /// 饼图标题
        /// </summary>
        [JsonProperty("pieChartTitle_Alarm")]
        public string PieChartTitle_Alarm { get; set; }

        /// <summary>
        /// 饼图数值
        /// </summary>
        [JsonProperty("pieChartValue_Alarm")]
        public string PieChartValue_Alarm { get; set; }

        /// <summary>
        /// 饼图数据
        /// </summary>
        [JsonProperty("pieChartData_Alarm")]
        public List<PieChartDto> PieChartData_Alarm { get; set; }
        #endregion
        #region 任务
        /// <summary>
        /// 饼图标题
        /// </summary>
        [JsonProperty("pieChartTitle_Task")]
        public string PieChartTitle_Task { get; set; }

        /// <summary>
        /// 饼图数值
        /// </summary>
        [JsonProperty("pieChartValue_Task")]
        public string PieChartValue_Task { get; set; }

        /// <summary>
        /// 饼图数据
        /// </summary>
        [JsonProperty("pieChartData_Task")]
        public List<PieChartDto> PieChartData_Task { get; set; }
        #endregion
        /// <summary>
        /// 条形图标题
        /// </summary>
        [JsonProperty("progressTitle")]
        public string ProgressTitle { get; set; }
        /// <summary>
        /// 条形图数据
        /// </summary>
        [JsonProperty("progressData")]
        public List<ProgressDto> ProgressData { get; set; }
        /// <summary>
        /// 柱状图标题
        /// </summary>
        [JsonProperty("histogramTitle")]
        public string HistogramTitle { get; set; }
        /// <summary>
        /// 柱状图数据
        /// </summary>
        [JsonProperty("histogramData")]
        public List<HistogramDto> HistogramData { get; set; }
    }
}
