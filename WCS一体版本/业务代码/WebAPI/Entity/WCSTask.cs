using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.FrameworkExtend;

namespace ZHQXC.Client
{
    public class WCSTask
    {
        public WCSTask()
        {

        }

        public WCSTask(Task task)
        {
            this.Id = task.Id;
            this.TaskCode = task.TaskCode;
            this.Source = task.Source.ToString();
            this.StartLocation = new WcsLocation(task.StartLocation);
            this.EndLocation = new WcsLocation(task.EndLocation);
            this.CurrentLocation = new WcsLocation(task.CurrentLocation);
            this.Status = task.Status.ToString();
            this.TaskType = task.TaskType;
            this.CreatedAt = task.CreatedAt;
            this.FinishedAt = task.FinishedAt;
            this.Description = task.Description == null ? "" : task.Description;
            this.ContainerCodes = task.ContainerCodes == null ? new List<string>() : task.ContainerCodes.ToList();
            this.Priority = task.Priority;
            this.Movements = GetMovements(task.Movements);
            this.AdditionalInfo = task.AdditionalInfo == null ? new Dictionary<string, string>() : task.AdditionalInfo.ToDictionary(x => x.Key, x => x.Value);
        }

        private List<WCSLogicMovement> GetMovements(Iesi.Collections.Generic.ISet<Wcs.Framework.LogicMovement> movements)
        {
            List<WCSLogicMovement> list = new List<WCSLogicMovement>();
            if (movements != null)
            {
                foreach (var item in movements)
                {
                    var movement = new WCSLogicMovement(item);
                    list.Add(movement);
                }
            }
            return list;
        }


        public Int32 Id { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public String TaskCode { get; set; }
        /// <summary>
        /// 任务来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 任务起始位置
        /// </summary>
        public WcsLocation StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public WcsLocation EndLocation { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        public WcsLocation CurrentLocation { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 任务类型
        /// <para>此属性的值来源于业务系统，在与业务系统交互时可能需要用到。</para>
        /// </summary>
        public String TaskType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? FinishedAt { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public String Description { get; set; } = "";
        /// <summary>
        /// 任务优先级（默认为 0），值越大，优先级越高。
        /// </summary>
        public Int32 Priority { get; set; }
        /// <summary>
        /// 已存在的动作
        /// </summary>
        public List<WCSLogicMovement> Movements { get; set; } = new List<WCSLogicMovement>();
        /// <summary>
        /// 托盘号
        /// </summary>
        public List<String> ContainerCodes { get; set; } = new List<string>();
        /// <summary>
        /// 附加属性集合
        /// </summary>
        public Dictionary<String, String> AdditionalInfo { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 即将行走路径集合
        /// </summary>
        public Dictionary<Int32, Int32> TaskPredictRoutes { get; set; } = new Dictionary<int, int>();
    }

    public class WCSLogicMovement
    {
        public WCSLogicMovement()
        {

        }
        public WCSLogicMovement(Wcs.Framework.LogicMovement movement)
        {
            this.Id = movement.Id;
            this.DeviceName = movement.DeviceName;
            this.CreatedAt = movement.CreatedAt;
            this.FinishedAt = movement.FinishedAt;
            this.RouteId = movement.RouteId;
            this.StartLocation = new WcsLocation(movement.StartLocation);
            this.EndLocation = new WcsLocation(movement.EndLocation);
            this.Status = (Int32)movement.Status;
            this.EquipmentActions = GetEquipmentActions(movement.EquipmentActions);
        }

        private List<WCSEquipmentAction> GetEquipmentActions(Iesi.Collections.Generic.ISet<Wcs.Framework.EquipmentAction> equipmentActions)
        {
            List<WCSEquipmentAction> list = new List<WCSEquipmentAction>();
            if (equipmentActions != null)
            {
                foreach (var item in equipmentActions)
                {
                    var action = new WCSEquipmentAction(item);
                    this.SendedAt = item.SentAt;
                    list.Add(action);
                }
            }
            return list;
        }

        //起点位置
        public WcsLocation StartLocation { get; set; }
        //     该逻辑动作依次需要执行的物理动作
        public List<WCSEquipmentAction> EquipmentActions { get; set; } = new List<WCSEquipmentAction>();
        //     结束时间
        public DateTime? FinishedAt { get; set; }
        public DateTime? SendedAt { get; set; }
        //     创建时间
        public DateTime CreatedAt { get; set; }
        public int? RouteId { get; set; }
        //     所属设备名称
        public string DeviceName { get; set; }
        public int Id { get; set; }
        //     终点位置
        public WcsLocation EndLocation { get; set; }
        //     获取逻辑动作的状态
        public Int32 Status { get; set; }
    }

    public class WCSEquipmentAction
    {
        public WCSEquipmentAction()
        {
        }
        public WCSEquipmentAction(Wcs.Framework.EquipmentAction action)
        {
            this.Id = action.Id;
            this.EquipmentTaskId = action.EquipmentTaskId;
            this.StartLocation = new WcsLocation(action.Movement.StartLocation);
            this.EndLocation = new WcsLocation(action.Movement.EndLocation);
            this.DeviceName = action.DeviceName;
            this.Status = (Int32)action.Status;
            this.CreatedAt = action.CreatedAt;
            this.SendedAt = action.SentAt;
            this.FinishedAt = action.FinishedAt;
            this.Alarms = action.Warnings == null ? "" : string.Join(",", action.Warnings);
            this.Description = action.ToReadableDescription();
        }

        public Int32 Id { get; set; }
        public Int32 EquipmentTaskId { get; set; }
        public WcsLocation StartLocation { get; set; }
        public WcsLocation EndLocation { get; set; }
        public string DeviceName { get; set; }
        public string ActuatingDeviceName { get; set; }
        public Int32 Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SendedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public string Alarms { get; set; }

        public string Description { get; set; }
    }

    public class WcsLocation
    {
        public WcsLocation()
        { }

        public WcsLocation(LocationInfo locationInfo)
        {
            UserCode = locationInfo.UserCode;
            DeviceCode = locationInfo.DeviceCode;
            UnifiedCode = locationInfo.UnifiedCode;
            DeviceName = locationInfo.DeviceName;
        }
        public WcsLocation(Location location)
        {
            UserCode = location.UserCode;
            DeviceCode = location.DeviceCode;
            UnifiedCode = location.UnifiedCode;
            DeviceName = location.Device.Name;
        }
        public string UserCode { get; set; }
        public string DeviceCode { get; set; }
        public string UnifiedCode { get; set; }
        public string DeviceName { get; set; }

        public override string ToString()
        {
            return UserCode;
        }
    }


    public class WCSPreTask
    {
        private string m_group;

        public virtual int Id { get; set; }

        //
        // 摘要:
        //     任务编号
        public virtual string TaskCode { get; set; }

        //
        // 摘要:
        //     任务来源
        public virtual TaskSource Source { get; set; }

        //
        // 摘要:
        //     任务起始位置
        public virtual WcsLocation StartLocation { get; set; }

        //
        // 摘要:
        //     终点位置
        public virtual WcsLocation EndLocation { get; set; }

        //
        // 摘要:
        //     任务状态
        public virtual TaskStatus Status { get; set; }

        //
        // 摘要:
        //     任务类型
        //     此属性的值来源于业务系统，在与业务系统交互时可能需要用到。
        public virtual string TaskType { get; set; }

        //
        // 摘要:
        //     创建时间
        public virtual DateTime CreatedAt { get; set; }

        //
        // 摘要:
        //     开始时间
        public virtual DateTime? StartedAt { get; set; }

        //
        // 摘要:
        //     结束时间
        public virtual DateTime? FinishedAt { get; set; }

        //
        // 摘要:
        //     备注信息
        public virtual string Description { get; set; }

        //
        // 摘要:
        //     任务优先级（默认为 0），值越大，优先级越高。
        public virtual int Priority { get; set; }

        //
        // 摘要:
        //     WCS任务优先级（默认为 0），值越大，优先级越高。 该优先级为WCS系统自动调整 防呆措施，防止任务长时间呆死在任务池
        public virtual int WcsPriority { get; set; }

        //
        // 摘要:
        //     人工指定优先执行 该字段设计是在条件允许的情况下优先执行该条任务其余任务即使优先级高也必须延后执行
        public virtual bool ManualPriority { get; set; }

        public virtual string ContainerCodes { get; set; }

        public virtual string AdditionalInfo { get; set; }

        public virtual string ReportRecords { get; set; }

        protected WCSPreTask()
        {
            ContainerCodes = JsonConvert.SerializeObject(new List<string>());
            AdditionalInfo = JsonConvert.SerializeObject(new Dictionary<string, string>());
            CreatedAt = DateTime.Now;
            Priority = 0;
            Status = TaskStatus.New;
        }

        public WCSPreTask(string taskCode, WcsLocation startLocation, WcsLocation endLocation)
            : this()
        {
            TaskCode = taskCode;
            StartLocation = startLocation;
            EndLocation = endLocation;
        }

        public WCSPreTask(PreTask pretask)
        {
            this.Id = pretask.Id;
            this.TaskCode = pretask.TaskCode;
            this.Source = pretask.Source;
            this.StartLocation = new WcsLocation(pretask.StartLocation);
            this.EndLocation = new WcsLocation(pretask.EndLocation);
            this.Status = pretask.Status;
            this.TaskType = pretask.TaskType;
            this.CreatedAt = pretask.CreatedAt;
            this.StartedAt = pretask.StartedAt;
            this.FinishedAt = pretask.FinishedAt;
            this.Description = pretask.Description;
            this.Priority = pretask.Priority;
            this.WcsPriority = pretask.WcsPriority;
            this.ManualPriority = pretask.ManualPriority;
            this.ContainerCodes = pretask.ContainerCodes;
            this.AdditionalInfo = pretask.AdditionalInfo;
            this.ReportRecords = pretask.ReportRecords;
        }

        public virtual int Compare(WCSPreTask x, WCSPreTask y)
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

            if (x.Id != y.Id || x.CreatedAt != y.CreatedAt || x.Description != y.Description || x.EndLocation != y.EndLocation || x.FinishedAt != y.FinishedAt || x.StartLocation != y.StartLocation || x.Status != y.Status || x.TaskCode != y.TaskCode)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return $"预任务 {Id}#{TaskCode}";
        }
    }
}
