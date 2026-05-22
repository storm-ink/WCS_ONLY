using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    public class ReflectionHelper
    {
        /// <summary>
        /// 创建指定的类型名称的实例，并转换为指定的类型
        /// </summary>
        /// <typeparam name="T">泛型参数，要返回的实例类型。</typeparam>
        /// <param name="typeName">类型。</param>
        /// <param name="args">构造函数参数集合。</param>
        /// <returns>指定类型名称的实例</returns>
        /// <example>在未找到 typeName 中指定的类型时，将引发此异常.</example>
        public static T CreateInstance<T>(Type type, params object[] args)
        {
            return (T)type.Assembly
                .CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, args, null, null);
        }

        /// <summary>
        /// 创建指定的类型名称的实例，并转换为指定的类型
        /// </summary>
        /// <typeparam name="T">泛型参数，要返回的实例类型。</typeparam>
        /// <param name="typeName">类型名称。</param>
        /// <param name="args">构造函数参数集合。</param>
        /// <returns>指定类型名称的实例</returns>
        /// <example>在未找到 typeName 中指定的类型时，将引发此异常.</example>
        public static T CreateInstance<T>(string typeName, params object[] args)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new Exception(String.Format("未找到类型 “{0}”", typeName));
            }
            return (T)type.Assembly
                .CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, args, null, null);
        }
    }
}
