using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.FrameworkExtend
{
    public class PreTask : Comparable<PreTask>, IComparer<PreTask>
    {
        String m_group;
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 关联的主控任务号（父任务号）
        /// </summary>
        public virtual String MasterTaskCode { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public virtual String TaskCode { get; set; }
        /// <summary>
        /// 任务来源
        /// </summary>
        public virtual TaskSource Source { get; set; }
        /// <summary>
        /// 任务起始位置
        /// </summary>
        public virtual LocationInfo StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo EndLocation { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual TaskStatus Status { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public virtual TaskBizType BizType { get; set; }
        /// <summary>
        /// 任务类型
        /// <para>此属性的值来源于业务系统，在与业务系统交互时可能需要用到。</para>
        /// </summary>
        public virtual String TaskType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public virtual DateTime? StartedAt { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public virtual DateTime? FinishedAt { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 任务优先级（默认为 0），值越大，优先级越高。
        /// </summary>
        public virtual Int32 Priority { get; set; }
        /// <summary>
        /// WCS任务优先级（默认为 0），值越大，优先级越高。
        /// 该优先级为WCS系统自动调整
        /// 防呆措施，防止任务长时间呆死在任务池
        /// </summary>
        public virtual Int32 WcsPriority { get; set; }
        /// <summary>
        /// 人工指定优先执行
        /// 该字段设计是在条件允许的情况下优先执行该条任务其余任务即使优先级高也必须延后执行
        /// </summary>
        public virtual Boolean ManualPriority { get; set; }
        /// <summary>
        /// 托盘号（List<string> json格式 ）
        /// </summary>
        public virtual string ContainerCodes { get; set; }
        /// <summary>
        /// 附加属性集合（Dictionary<string,string> json格式）
        /// </summary>
        public virtual string AdditionalInfo { get; set; }

        public virtual string ReportRecords { get; set; }

        protected static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected PreTask()
        {
            ContainerCodes = JsonConvert.SerializeObject(new List<string>());
            AdditionalInfo = JsonConvert.SerializeObject(new Dictionary<String, String>());
            CreatedAt = DateTime.Now;
            Priority = 0;
            BizType = TaskBizType.Normal;
            Status = TaskStatus.New;
        }

        public PreTask(String taskCode, LocationInfo startLocation, LocationInfo endLocation)
            : this()
        {
            this.TaskCode = taskCode;
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
        }

        public virtual int Compare(PreTask x, PreTask y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.Id != y.Id
             || x.BizType != y.BizType
             || x.CreatedAt != y.CreatedAt
             || x.Description != y.Description
             || x.EndLocation != y.EndLocation
             || x.FinishedAt != y.FinishedAt
             || x.StartLocation!= y.StartLocation
             || x.Status != y.Status
             || x.TaskCode != y.TaskCode)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("预任务 {0}#{1}", this.Id, this.TaskCode);
        }
    }
}
