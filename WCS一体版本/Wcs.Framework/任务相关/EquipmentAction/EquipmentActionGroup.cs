using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 物理动作组
    /// 物理动作在被分成多个设备动作后，有些动作必须连接执行。
    /// 为了防止在排序时破坏原来的执行顺序，所以引入物理动作组的概念
    /// </summary>
    public class EquipmentActionGroup:IComparer<EquipmentActionGroup>
    {
        public virtual Guid Id { get; protected set; }
        public EquipmentActionGroup()
        {
            this.Id = Guid.NewGuid();
        }
        
        public virtual int Compare(EquipmentActionGroup x, EquipmentActionGroup y)
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

            if (x.Id != y.Id)
            {
                return 1;
            }

            return 0;
        }

        public override int GetHashCode()
        {
            if (Id == Guid.Empty)
            {
                return 0;
            }

            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as EquipmentActionGroup;
            if (o == null)
            {
                return false;
            }

            return o.Id == this.Id;
        }
    }
}
