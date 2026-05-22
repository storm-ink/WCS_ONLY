using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Wcs.Security
{
    /// <summary>
    /// 表示一个用户标志
    /// </summary>
    public sealed class WcsIdentity:IIdentity
    {
        /// <summary>
        /// 表示一个空的用户标志
        /// </summary>
        public static WcsIdentity Empty = new WcsIdentity(String.Empty,String.Empty, new String[0], new String[0]);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="realName">真实姓名</param>
        /// <param name="roleNames">角色名</param>
        /// <param name="operations">用户拥有的操作权限</param>
        public WcsIdentity(String userName,String realName, String[] roleNames, String[] operations)
        {
            this.Name = userName;
            this.RoleNames = roleNames;
            this.RealName = realName;
            this.Operations = operations;
            this.IsAuthenticated = true;
        }
        /// <summary>
        /// 获取所使用的身份验证的类型
        /// </summary>
        public string AuthenticationType
        {
            get
            {
                return "WcsIdentity";
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否验证了用户
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// 获取当前用户的名称。
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 获取当前用户的真实名称。
        /// </summary>
        public string RealName { get; private set; }
        /// <summary>
        /// 允许的操作
        /// </summary>
        public String[] Operations { get; private set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public String[] RoleNames { get; private set; }
    }
}
