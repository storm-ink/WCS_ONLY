using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 货架位置
    /// </summary>
    public class RackLocation : Location
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="device">位置所属设备</param>
        /// <param name="userCode">用户编码</param>
        /// <param name="forkDirection">位置所在的排相对于设备所在的左右位置</param>
        /// <param name="forkAction">货叉允许的动作</param>
        /// <param name="column">列位置</param>
        /// <param name="level">层位置</param>
        /// <param name="userColumn">用户列位置</param>
        /// <param name="userLevel">用户层位置</param>
        /// <param name="enabled">是否启用该位置</param>
        /// <param name="height">高度</param>
        /// <param name="unifiedCode">统一编号</param>
        public RackLocation(CraneDevice device, String userCode, ForkDirection? forkDirection, ForkAction forkAction, int column, int level, int userLine, int userColumn, int userLevel, Boolean enabled, int height, string unifiedCode = null)
            : base(string.Format("{0:00}{1:000}{2:000}", userLine, column, level), userCode, device,unifiedCode)
        {
            this.Enabled = enabled;
            this.Column = column;
            this.Level = level;
            this.UserLine = userLine;
            this.UserColumn = userColumn;
            this.UserLevel = userLevel;
            this.ForkDirection = forkDirection;
            this.ForkAction = forkAction;
            this.Height = height;
        }

        /// <summary>
        /// 列位置
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// 指示该位置是否启用
        /// </summary>
        public Boolean Enabled { get; private set; }

        /// <summary>
        /// 货叉允许的动作
        /// </summary>
        public virtual ForkAction ForkAction { get; protected set; }

        /// <summary>
        /// 堆垛机执行任务时此位置的排相对于设备所在的左右位置
        /// 为空时表示不允许伸叉
        /// </summary>
        public virtual ForkDirection? ForkDirection { get; protected set; }

        /// <summary>
        /// 层位置
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 用户排位置(系统内不使用，只是用于配合手持地操的使用)
        /// </summary>
        public int UserLine { get; set; }
        /// <summary>
        /// 用户列位置(系统内不使用，只是用于配合手持地操的使用)
        /// </summary>
        public int UserColumn { get; set; }
        /// <summary>
        /// 用户层位置(系统内不使用，只是用于配合手持地操的使用)
        /// </summary>
        public int UserLevel { get; set; }
        /// <summary>
        /// 货格高度，一般默认0-未配置高度，如果需要配置高度则从低到高从1到n依次增大取值
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 判断指定的叉货动作是否在当前货位被允许
        /// </summary>
        /// <param name="forkAction">叉货动作</param>
        /// <returns>允许返回 true,不允许返回 false</returns>
        public Boolean IsForkActionAllowed(ForkAction forkAction)
        {
#warning 该方法有问题
            if (forkAction == ForkAction.None)
            {
                return false;
            }

            if (this.ForkAction == forkAction)
            {
                return true;
            }
            if (this.ForkAction == ForkAction.None)
            {
                return false;
            }

            if ((this.ForkAction & forkAction) == this.ForkAction)
            {
                return true;
            }
            if ((forkAction & this.ForkAction) == forkAction)
            {
                return true;
            }

            return false;
        }
    }
}
