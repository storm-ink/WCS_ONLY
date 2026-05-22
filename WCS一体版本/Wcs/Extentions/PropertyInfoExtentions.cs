using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// PropertyInfo 扩展
    /// </summary>
    public static class PropertyInfoExtentions
    {
        /// <summary>
        /// 获取当前属性类型定义的显示名，优先从 DisplayNameAttribute 找，如果没有定义，则找 DescriptionAttribute。否则直接返回 Name
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static String GetDisplayName(this PropertyInfo source)
        {
            var displayNameAttr = source.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false).Cast<System.ComponentModel.DisplayNameAttribute>().FirstOrDefault();
            if (displayNameAttr != null)
            {
                return displayNameAttr.DisplayName;
            }

            var descriptionAttr = source.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).Cast<System.ComponentModel.DescriptionAttribute>().FirstOrDefault();
            if (descriptionAttr != null)
            {
                return descriptionAttr.Description;
            }

            return source.Name;
        }
    }
}
