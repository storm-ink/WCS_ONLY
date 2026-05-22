using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace Wcs.Security
{
    /// <summary>
    /// 为Wcs相关操作指定安全操作的特性类
    /// </summary>
    [Serializable]
    public sealed class WcsPermissionAttribute : CodeAccessSecurityAttribute
    {
        /// <summary>
        /// 操作名称（可用"\"来进行分组显示，如：任务管理\任务管理器\暂停任务）
        /// </summary>
        public String OperationName { get; set; }
        /// <summary>
        /// 授权指定的用户
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// 授权指定的角色
        /// </summary>
        public String RoleName { get; set; }
        /// <summary>
        /// 获取当前操作所在的完整路径。
        /// </summary>
        /// <returns>返回一个字符串数组，层级按数组内的元素随顺排列。最后一级为操作名称</returns>
        public String[] GetPath()
        {
            return OperationName.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
        }
        ///// 这个构造函数一加,在GetCurstomAttributes时就会报序列化错误。
        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="action">指定安全执行的安全操作</param>
        ///// <param name="operationName">操作名称（可用"\"来进行分组显示，如：任务管理\任务管理器\暂停任务）</param>
        //public WcsPermissionAttribute(SecurityAction action, String operationName)
        //    :this(action)
        //{
        //    this.OperationName = operationName;
        //}
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">指定安全执行的安全操作</param>
        public WcsPermissionAttribute(SecurityAction action)
            : base(action)
        {

        }
        /// <summary>
        /// 在派生类中重写时，将创建一个权限对象，该对象随后可序列化为二进制格式并与 System.Security.Permissions.SecurityAction 一起永久地存储在程序集的元数据中。
        /// </summary>
        /// <returns></returns>
        public override System.Security.IPermission CreatePermission()
        {
            return new WcsPermission(this);
        }
    }
}
