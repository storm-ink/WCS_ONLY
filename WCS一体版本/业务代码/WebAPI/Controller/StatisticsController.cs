using NHibernate.Linq;
using ZHQXC.WebAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Wcs;
using Wcs.Framework;
using ZHQXC.AlarmPool;
using ZHQXC.PreTaskHand;
using Wcs.FrameworkExtend;

namespace ZHQXC.WebAPI
{
    public class StatisticsController : ApiController
    {

        public string[] DeviceNames = new string[] { "C001", "C002", "C003", "C004", "输送线1", "输送线2", "R001", "R002", "R003", "R004" };

        public Dictionary<string, string[]> Dictionary = new Dictionary<string, string[]>();

        [HttpGet]
        public object All()
        {
            ResponseDto responseDto = new ResponseDto();
            Get();
            try
            {
                responseDto.Success = true;
                responseDto.Status = 0;
                responseDto.Error = "操作成功";

                StatisticsDto statisticsDto = new StatisticsDto();
                var warings = BuildWarningImage();

                Tables(ref statisticsDto, warings);
                PieChart_Alarm(ref statisticsDto, warings);
                PieChart_Task(ref statisticsDto);
                Progress(ref statisticsDto, warings);
                Histogram(ref statisticsDto, warings);

                responseDto.Data = statisticsDto;
                return responseDto;
            }
            catch (Exception m)
            {
                responseDto.Error = $"操作失败:{m.Message}";
                responseDto.Success = false;
                responseDto.Status = 1;
                return responseDto;
            }
        }

        public void Get()
        {
            Dictionary.Add("堆垛机", new string[] { "C001", "C002", "C003", "C004" });
            Dictionary.Add("输送线", new string[] { "输送线1", "输送线2" });
            Dictionary.Add("穿梭车", new string[] { "R001", "R002", "R003", "R004" });
        }

        /// <summary>
        /// 报警数据采集
        /// </summary>
        /// <returns></returns>
        private List<dynamic> BuildWarningImage()
        {
            DateTime t = DateTime.Now.AddDays(-7);
            List<AlarmRecord> warnings = new List<AlarmRecord>();
            List<AlarmRecord> historyWarnings = new List<AlarmRecord>();
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                warnings = unitOfWork.session.Query<AlarmRecord>().Where(x => x.BeginingAt > t).ToList();
                unitOfWork.Commit();
            }
            using (NHBackupServerUnitOfWork unitOfWork1 = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                historyWarnings = unitOfWork1.session.Query<AlarmRecord>().Where(x => x.BeginingAt > t).ToList();
                unitOfWork1.Commit();
            }
            var warning = warnings.Cast<AlarmRecord>().Where(x => DeviceNames.Contains(x.OwnerDevice)).ToList();
            var historyWarning = historyWarnings.Cast<AlarmRecord>().Where(x => DeviceNames.Contains(x.OwnerDevice)).ToList();
            warning.AddRange(historyWarning);
            return warning.Cast<dynamic>().ToList();
        }

        /// <summary>
        /// 组装面板数据
        /// </summary>
        /// <param name="statisticsDto"></param>
        public void Tables(ref StatisticsDto statisticsDto, List<dynamic> warnings)
        {
            List<TableDto> tableDtos = new List<TableDto>();
            var day = (DateTime.Now - Convert.ToDateTime("2023-01-1 00:00:00")).Days;
            var nowWarning = warnings.Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0).ToArray();
            var yesterdayWarning = warnings.Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == -1).ToArray();
            var pendingWarning = warnings.Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0 && x.EndingAt == null).ToArray();
            int proportion = 100;
            if (nowWarning.Length != 0 && yesterdayWarning.Length != 0)
            {
                proportion = Convert.ToInt32((Convert.ToDouble(nowWarning.Length) / Convert.ToDouble(yesterdayWarning.Length) - 1) * 100);
                // proportion =Convert.ToInt32(Convert.ToDouble(nowWarning.Length)/(Convert.ToDouble(yesterdayWarning.Length)+Convert.ToDouble(nowWarning.Length))*100);
            }

            tableDtos.Add(new TableDto() { Name = "系统稳定运行时间（天）", Value = day.ToString(), Color = "#73bd03" });
            tableDtos.Add(new TableDto() { Name = "今日异常（项）", Value = nowWarning.Length.ToString(), Color = "#f34336" });
            tableDtos.Add(new TableDto() { Name = "异常日环比", Value = proportion.ToString() + "%", Color = proportion > 0 ? "#CDDC3A" : "#FFD607" });
            tableDtos.Add(new TableDto() { Name = "今日已解决（项）", Value = Convert.ToString(nowWarning.Length - pendingWarning.Length), Color = "#f34336" });
            statisticsDto.Tables = tableDtos;
        }

        /// <summary>
        /// 饼形图数据-Alarm
        /// </summary>
        /// <param name="statisticsDto"></param>
        public void PieChart_Alarm(ref StatisticsDto statisticsDto, List<dynamic> warnings)
        {
            statisticsDto.PieChartTitle_Alarm = "各异常类型占比";
            statisticsDto.PieChartValue_Alarm = warnings.Cast<AlarmRecord>().ToList().Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0).Count().ToString();
            List<PieChartDto> pieChartDtos = new List<PieChartDto>();
            foreach (string key in Dictionary.Keys)
            {

                var warning = warnings.Cast<AlarmRecord>().ToList().Where(x => Dictionary[key].Contains(x.OwnerDevice) && Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0);
                pieChartDtos.Add(new PieChartDto() { Name = key, Value = warning.Count() });
            }
            statisticsDto.PieChartData_Alarm = pieChartDtos;
        }

        /// <summary>
        /// 饼形图数据-Task
        /// </summary>
        /// <param name="statisticsDto"></param>
        public void PieChart_Task(ref StatisticsDto statisticsDto)
        {
            statisticsDto.PieChartTitle_Task = "当日任务统计";
            List<PieChartDto> pieChartDtos = new List<PieChartDto>();
            int total = 0;
            var tasks = PreTaskHandHelper.PreTasks.Count(x => x.Status == Wcs.Framework.TaskStatus.New);
            total += tasks;
            pieChartDtos.Add(new PieChartDto() { Name = "待执行", Value = tasks });

            tasks = PreTaskHandHelper.PreTasks.Count(x => x.Status != Wcs.Framework.TaskStatus.New && x.Status != Wcs.Framework.TaskStatus.Completed && x.Status != Wcs.Framework.TaskStatus.Cancelled);
            total += tasks;
            pieChartDtos.Add(new PieChartDto() { Name = "执行中", Value = tasks });

            DateTime today = DateTime.Today;
            using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                tasks = unitOfWork.session.Query<PreTask>().Where(x=>x.CreatedAt >= today).Count();
                unitOfWork.Commit();
            }
            var _tasks = PreTaskHandHelper.PreTasks.Count(x => x.Status == Wcs.Framework.TaskStatus.Completed && x.Status == Wcs.Framework.TaskStatus.Cancelled);
            total += tasks;
            pieChartDtos.Add(new PieChartDto() { Name = "已完成", Value = tasks + _tasks });

            statisticsDto.PieChartValue_Task = total.ToString();

            statisticsDto.PieChartData_Task = pieChartDtos;
        }

        /// <summary>
        /// 条形图数据
        /// </summary>
        /// <param name="statisticsDto"></param>
        public void Progress(ref StatisticsDto statisticsDto, List<dynamic> warnings)
        {
            statisticsDto.ProgressTitle = "今日异常统计";
            List<ProgressDto> progressDtos = new List<ProgressDto>();
            //数据来源调整
            var todayWarning = warnings.Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0).ToArray();

            foreach (string key in DeviceNames)
            {
                var warning = warnings.Cast<AlarmRecord>().ToList().Where(x => key.Contains(x.OwnerDevice) && Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(DateTime.Now).Days == 0);

                int proportion = 0;
                var t = warning.Count();
                if (warnings.Count() != 0 && warning.Count() != 0)
                {
                    proportion = Convert.ToInt32(Convert.ToDouble(warning.Count()) / (Convert.ToDouble(todayWarning.Count())) * 100);
                }
                progressDtos.Add(new ProgressDto() { Name = key, Value = warning.Count(), Percent = proportion });
            }
            statisticsDto.ProgressData = progressDtos;
        }

        /// <summary>
        /// 柱状图数据
        /// </summary>
        /// <param name="statisticsDto"></param>
        public void Histogram(ref StatisticsDto statisticsDto, List<dynamic> warnings)
        {
            statisticsDto.HistogramTitle = "异常分类统计";
            List<HistogramDto> histogramDtos = new List<HistogramDto>();
            DateTime dateTime = DateTime.Now;
            for (int i = 6; i > 1; i--)
            {
                TimeSpan spacing = new TimeSpan(i - 1, 0, 0, 0);
                DateTime oldtime = dateTime.Subtract(spacing);
                oldtime = Convert.ToDateTime(oldtime.ToString("yyyy-MM-dd"));
                var warning = warnings.Where(x => Convert.ToDateTime(Convert.ToDateTime(x.BeginingAt).ToString("yyyy-MM-dd")).Subtract(oldtime).Days == 0).ToArray();
                var pendingWarning = warnings.Where(x => Convert.ToDateTime(x.BeginingAt).Subtract(oldtime).Days == 0 && x.EndingAt == null).ToArray();
                int PercentValue = 100;
                if (pendingWarning.Count() != 0 && warning.Length != 0)
                {
                    PercentValue = Convert.ToInt32(Convert.ToDouble(warning.Count() - pendingWarning.Count()) / (Convert.ToDouble(warning.Count())) * 100);
                }
                else if (warning.Count() == 0)
                {
                    PercentValue = 0;
                }
                if (PercentValue > 100)
                {
                    PercentValue = 100;
                }
                if (PercentValue < 0)
                {
                    PercentValue = 0;
                }
                histogramDtos.Add(new HistogramDto() { XValue = oldtime.ToString("MM-dd"), PercentValue = PercentValue, Y1Value = pendingWarning.Count(), Y2Value = warning.Count() - pendingWarning.Count() });

            }
            statisticsDto.HistogramData = histogramDtos;
        }
    }
}
