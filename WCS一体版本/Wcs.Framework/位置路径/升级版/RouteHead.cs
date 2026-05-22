using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 路径
    /// </summary>
    public class RouteHead
    {
        /// <summary>
        /// 自增长ID
        /// </summary>
        [DisplayName("路径号")]
        public virtual Int32 HeadID { get; set; }
        /// <summary>
        /// 项目编号
        /// </summary>
        [DisplayName("项目编号")]
        public virtual String ProjectCode { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        [DisplayName("项目名称")]
        public virtual String ProjectName { get; set; }
        /// <summary>
        /// 路径ID
        /// </summary>
        [DisplayName("路径ID")]
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 路径No
        /// </summary>
        [DisplayName("路径No")]
        public virtual Int32 No { get; set; }
        /// <summary>
        /// 路径方向
        /// </summary>
        [DisplayName("路径方向")]
        public virtual String Direction { get; set; }
        /// <summary>
        /// 所属设备名
        /// </summary>
        [DisplayName("所属设备名")]
        public virtual String Device { get; set; }
        /// <summary>
        /// 路径和任务的关系
        /// </summary>
        public virtual RouteVsActionRelations RouteVsActionRelation { get; set; }
        /// <summary>
        /// 是否允许中途继续执行
        /// </summary>
        [DisplayName("中途继续执行")]
        public virtual Boolean AllowStartFromMidway { get; set; }
        /// <summary>
        /// 分组标志
        /// </summary>
        [DisplayName("拆分路径分组标志")]
        public virtual string Group { get; set; }
        /// <summary>
        /// 单任务设备只可以作为起点的位置集合
        /// ★★★★★选择单任务设备路径时 终点 必须 不包含 在此列表中
        /// </summary>
        [DisplayName("单任务设备只可以作为起点的位置集合")]
        public virtual string PathOnlyStarts { get; set; } = "";
        /// <summary>
        /// 单任务设备只可以作为终点的位置集合
        /// ★★★★★选择单任务设备路径时 起点 必须 不包含 在此列表中
        /// </summary>
        [DisplayName("单任务设备只可以作为终点的位置集合")]
        public virtual string PathOnlyEnds { get; set; } = "";
        /// <summary>
        /// 路径明细
        /// </summary>
        [System.ComponentModel.Browsable (false )]
        public virtual Iesi.Collections.Generic.ISet<RouteDetail> Details { get; protected set; }
        /// <summary>
        /// 路径关系
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public virtual Iesi.Collections.Generic.ISet<RouteRelation> Relations { get; protected set; }
        /// <summary>
        /// 锁明细
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public virtual Iesi.Collections.Generic.ISet<RouteLock> Locks { get; protected set; }

        /// <summary>
        /// 从本路径对象创建一个逻辑动作
        /// </summary>
        /// <returns></returns>
        public virtual LogicMovement CreateLogicMovement(Task task, Location start, Location end)
        {
            var device = DeviceConverter.ToDevice<TaskableDevice>(this.Device);

            return device.LogicMovementSelector.ToLogicMovement(this, task, start, end);
        }
    }
}
