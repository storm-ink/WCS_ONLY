using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// 角色。
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public Role()
        {
            this.CreatedAt = DateTime.Now;
            this.Operations = new Iesi.Collections.Generic.HashedSet<String>();
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 Id { get; protected set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }

        /// <summary>
        /// 角色名称。自然键。
        /// </summary>
        public virtual String RoleName { get; set; }

        /// <summary>
        /// 是否内置角色
        /// </summary>
        public virtual Boolean IsBuiltIn { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Comments { get; set; }

        /// <summary>
        /// 返回表示此实例的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "角色#" + this.RoleName;
        }

        /// <summary>
        /// 此角色可以执行的操作。
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<String> Operations { get; protected set; }
    }

}


