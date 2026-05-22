using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Wcs.Security
{
    /// <summary>
    /// 表示一个Wcs用户凭证
    /// </summary>
    public sealed class WcsPrincipal:IPrincipal
    {
        static WcsPrincipal _currentPrincipal;
        /// <summary>
        /// 表示一个空的用户凭证
        /// </summary>
        public static WcsPrincipal Empty = new WcsPrincipal(WcsIdentity.Empty);
         /// <summary>
         /// 获取当前用户的标识
         /// </summary>
        public IIdentity Identity { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="identity">用户标识</param>
        public WcsPrincipal(WcsIdentity identity)
        {
            this.Identity = identity;
        }
        /// <summary>
        /// 确定当前用户是否属于指定的角色
        /// </summary>
        /// <param name="role">要检查其成员资格的角色的名称</param>
        /// <returns>如果当前用户是指定角色的成员，则为 true；否则为 false。</returns>
        public bool IsInRole(string role)
        {
            var id=(WcsIdentity)this.Identity;

            return id.RoleNames.Any(r => String.Equals(r, role, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// 指示当前对象是否是一个空的用户凭证
        /// </summary>
        public Boolean IsEmpty
        {
            get
            {
                return this == Empty;
            }
        }
        /// <summary>
        /// 获取或设置当前用户凭证
        /// </summary>
        public static WcsPrincipal CurrentPrincipal
        {
            get
            {
                if (_currentPrincipal == null)
                {
                    return Empty;
                }

                return _currentPrincipal;
            }
            set
            {
                _currentPrincipal = value;
            }
        }
    }
}
