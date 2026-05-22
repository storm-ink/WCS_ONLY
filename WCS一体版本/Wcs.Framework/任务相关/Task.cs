using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
namespace Wcs.Framework
{
    /// <summary>
    /// Wcs 主任务，描述一个完整的作业
    /// </summary>
    public class Task : Comparable<Task>, IComparer<Task>
    {
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public virtual String TaskId { get; set; }
        /// <summary>
        /// 该任务来自哪个请求(Wms在下任务时会将原来的RequestId带过来)
        /// </summary>
        public virtual Request FromRequest { get; set; }
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
        /// 当前位置
        /// </summary>
        public virtual LocationInfo CurrentLocation { get; set; }
        /// <summary>
        /// 任务方向
        /// </summary>
        public virtual TaskDirection Direction { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual TaskStatus Status { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public virtual TaskBizType BizType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
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
        /// 已存在的动作
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<LogicMovement> Movements { get; protected set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<String> ContainerCodes { get; protected set; }

        public Task()
        {
            Movements = new Iesi.Collections.Generic.HashedSet<LogicMovement>();
            ContainerCodes = new Iesi.Collections.Generic.HashedSet<String>();
            CreatedAt = DateTime.Now;
            Priority = 0;
        }

        /// <summary>
        /// 添加一个新的逻辑动作
        /// </summary>
        /// <param name="movement">新逻辑动作</param>
        public virtual void AddMovement(LogicMovement movement)
        {
            movement.Task = this;
            movement.Ordering = this.Movements.Count();
            this.Movements.Add(movement);
        }

        /// <summary>
        /// 获取下一个物理动作
        /// </summary>
        /// <remarks>
        /// 在当前任务中尝试获取下一个待执行的 LogicMovement，如果不存在，将尝试使用FindNextPath 函数搜索连通网络并拆分形成一个新的 LogicMovement对象返回。否则返回 null.
        /// </remarks>
        /// <param name="unitOfWork">数据库持久化上下文</param>
        /// <returns>当前可用的待执行的 LogicMovement 对象</returns>
        public virtual LogicMovement GetNextMovement(NHUnitOfWork unitOfWork)
        {
            throw new NotImplementedException();
        }

        public virtual int Compare(Task x, Task y)
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
             || x.CurrentLocation.Compare(x.CurrentLocation, y.CurrentLocation) != 0
             || x.Description != y.Description
             || x.Direction != y.Direction
             || x.EndLocation.Compare(x.EndLocation, y.EndLocation) != 0
             || x.FinishedAt != y.FinishedAt
             || x.StartLocation.Compare(x.StartLocation, y.StartLocation) != 0
             || x.Status != y.Status
             || x.TaskId != y.TaskId
                )
            {
                return 1;
            }

            return 0;
        }

    }
}
