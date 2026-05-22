using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// Enum 扩展
    /// </summary>
    public static class EnumExtentions
    {
        /// <summary>
        /// 获取指定枚举类型的描述信息<br />
        /// 描述信息包括 DisplayNameAttribute 和 DescriptionAttribute 属性
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="source">源</param>
        /// <returns></returns>
        public static string GetDescription<T>(this T source) where T : struct
        {
            Type type = source.GetType();
            string fileName = Enum.GetName(type, source);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Convert.ToString(source);
            }
            System.Reflection.FieldInfo field = type.GetField(fileName);

            if (field == null)
            {
                return Convert.ToString(source);
            }

            System.ComponentModel.DescriptionAttribute descriptionAttr = field
                .GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
            if (descriptionAttr != null)
            {
                return descriptionAttr.Description;
            }

            System.ComponentModel.DisplayNameAttribute displayNameAttr = field
                .GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false)
                .FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;
            if (displayNameAttr != null)
            {
                return displayNameAttr.DisplayName;
            }

            return Convert.ToString(source);
        }

        /// <summary>
        /// 将指定的枚举类型转换为 键/值 列表
        /// </summary>
        /// <typeparam name="T">指定的枚举类型</typeparam>
        /// <returns></returns>
        public static List<KeyValuePair<String, String>> ToKeyValueList<T>() where T : struct
        {
            List<KeyValuePair<String, String>> result = new List<KeyValuePair<String, String>>();
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                if (field.IsSpecialName) continue;

                var key = field.Name;
                var text = GetDescription((T)Enum.Parse(typeof(T), key));
                result.Add(new KeyValuePair<String, String>(key, text));
            }

            return result;
        }
    }
}
