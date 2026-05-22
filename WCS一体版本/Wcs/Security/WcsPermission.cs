using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Wcs.Security
{
    [Serializable]
    public sealed class WcsPermission : IPermission
    {
        WcsPermissionAttribute _wcsPermissionAttribute;
        public WcsPermission(PermissionState state)
        {
            
        }
        /// <summary>
        /// 获取一个值，指示当前是否处于管理员模式。
        /// 管理员模式下将不再检查用户权限。
        /// </summary>
        public static Boolean IsAdministratorMode
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();

                if (args == null || args.Length == 0)
                {
                    return false;
                }


                return args.Any(x => string.Equals(x, "IsAdministrator", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public WcsPermission(WcsPermissionAttribute wcsPermissionAttribute)
        {
            if (wcsPermissionAttribute == null)
            {
                throw new ArgumentNullException("wcsPermissionAttribute");
            }

            _wcsPermissionAttribute = wcsPermissionAttribute;
        }

        public void Demand()
        {
            if (IsAdministratorMode)
            {
                return;
            }

            WcsPrincipal principal = WcsPrincipal.CurrentPrincipal;
            WcsIdentity id = (WcsIdentity)principal.Identity;

            if (principal.IsEmpty)
            {
                throw new System.Security.SecurityException(string.Format("未登录用户无法访问 {0} 方法", _wcsPermissionAttribute.OperationName));
            }

            if (id.RoleNames.Any(x => x == "管理员"))
            {
                return;
            }

            if (
                !String.IsNullOrWhiteSpace(_wcsPermissionAttribute.RoleName)
                && !id.RoleNames.Any(r => String.Equals(r, _wcsPermissionAttribute.RoleName, StringComparison.CurrentCultureIgnoreCase))
                )
            {

                throw new System.Security.SecurityException(string.Format("未被授权访问 {0} 方法", _wcsPermissionAttribute.OperationName));
            }

            if (
                !String.IsNullOrWhiteSpace(_wcsPermissionAttribute.UserName)
                && !String.Equals(_wcsPermissionAttribute.UserName, id.Name, StringComparison.CurrentCultureIgnoreCase)
                )
            {

                throw new System.Security.SecurityException(string.Format("未被授权访问 {0} 方法", _wcsPermissionAttribute.OperationName));
            }

            if (id.Operations==null 
                || id.Operations.Length==0 
                ||!id.Operations.Any(op => String.Equals(_wcsPermissionAttribute.OperationName, op, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new System.Security.SecurityException(string.Format("未被授权访问 {0} 方法", _wcsPermissionAttribute.OperationName));
            }
        }
        public bool IsSubsetOf(IPermission target)
        {
            return false;
        }

        public IPermission Copy()
        {
            throw new NotImplementedException();
        }

        public IPermission Intersect(IPermission target)
        {
            throw new NotImplementedException();
        }

        public IPermission Union(IPermission target)
        {
            throw new NotImplementedException();
        }

        public void FromXml(SecurityElement e)
        {
            var m_specifiedAsUnrestricted = false;
            var m_flags = 0;

            // If XML indicates an unrestricted permission, make this permission unrestricted.
            String s = (String)e.Attributes["Unrestricted"];
            if (s != null)
            {
                m_specifiedAsUnrestricted = Convert.ToBoolean(s);
                if (m_specifiedAsUnrestricted)
                    m_flags = 0;
            }

        }

        public SecurityElement ToXml()
        {
            SecurityElement e = new SecurityElement("IPermission");

            e.AddAttribute("class", GetType().AssemblyQualifiedName.Replace('\"', '\''));
            e.AddAttribute("version", "1");
            e.AddAttribute("Unrestricted", "false");

            return e;
        }
    }
}
