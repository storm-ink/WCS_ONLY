using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// 用户。
    /// </summary>
    [Serializable]
    public class User
    {
        /// <summary>
        /// 初始化 User 类的新实例。
        /// </summary>
        public User()
        {
            this.CreatedAt = DateTime.Now;

            this.Roles = new Iesi.Collections.Generic.HashedSet<Role>();
            this.Profile = new Dictionary<string, string>();
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
        /// 用户名。自然键。
        /// </summary>
        public virtual String UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public virtual String Password { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public virtual String RealName { get; set; }

        /// <summary>
        /// 密码盐度
        /// </summary>
        public virtual String PasswordSalt { get; set; }
        /// <summary>
        /// 是否内置用户
        /// </summary>
        public virtual Boolean IsBuiltIn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Comments { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public virtual String Email { get; set; }

        /// <summary>
        /// 此用户具有的角色。
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<Role> Roles { get; protected set; }

        /// <summary>
        /// 用户资料
        /// </summary>
        public virtual IDictionary<string, string> Profile { get; protected set; }

        /// <summary>
        /// 将此实例添加到角色。
        /// </summary>
        /// <param name="role"></param>
        public virtual void AddToRole(Role role)
        {
            this.Roles.Add(role);
        }

        /// <summary>
        /// 将此实例从角色移除。
        /// </summary>
        /// <param name="role"></param>
        public virtual void RemoveFromRole(Role role)
        {
            this.Roles.Remove(role);
        }

        /// <summary>
        /// 将此实例添加到角色。
        /// </summary>
        /// <param name="roles"></param>
        public virtual void AddToRoles(Role[] roles)
        {
            this.Roles.AddAll(roles);
        }

        /// <summary>
        /// 将此实例从角色移除。
        /// </summary>
        /// <param name="roles"></param>
        public virtual void RemoveFromRoles(Role[] roles)
        {
            this.Roles.RemoveAll(roles);
        }

        /// <summary>
        /// 用户是否在指定的角色中
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual Boolean IsInRole(string roleName)
        {
            return Roles.Select(x => x.RoleName)
                .ToList()
                .Any(x => x.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// 返回表示此 User 的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "用户#" + this.UserName;
        }
    }
}
