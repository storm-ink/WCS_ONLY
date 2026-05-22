using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// Type 扩展
    /// </summary>
    public static class TypeExtentions
    {
        /// <summary>
        /// 获取当前类型定义的显示名，优先从 DisplayNameAttribute 找，如果没有定义，则找 DescriptionAttribute。否则直接返回 Name
        /// </summary>
        /// <param name="source">源</param>
        /// <returns></returns>
        public static String GetDisplayName(this Type source)
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

        /// <summary>
        /// 创建指定的类型名称的实例，并转换为指定的类型
        /// </summary>
        /// <typeparam name="T">泛型参数，要返回的实例类型。</typeparam>
        /// <param name="typeName">类型名称。</param>
        /// <param name="args">构造函数参数集合。</param>
        /// <returns>指定类型名称的实例</returns>
        /// <example>在未找到 typeName 中指定的类型时，将引发此异常.</example>
        public static T CreateInstance<T>(this Type type, params object[] args)
        {
            return (T)type.Assembly
                .CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, args, null, null);
        }

        /// <summary>
        /// 判断类型是否是指定的类型或子类
        /// </summary>
        /// <param name="type">要判断类型</param>
        /// <param name="typeName">目标类型完整名称（Type.FullName），忽略大小写</param>
        /// <returns></returns>
        public static Boolean IsType(this Type type,String typeName)
        {
            var baseType=type;
            while (baseType != typeof(Object))
            {
                if (String.Equals(baseType.FullName, typeName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
