using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// 辅助函数库，提供一些日常经常性用到的函数
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 将指定的数据转为指定类型
        /// </summary>
        /// <typeparam name="T">    泛型类型参数. </typeparam>
        /// <param name="value">    要转换的数据. </param>
        public static T ConvertTo<T>(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return default(T);

            Type conversionType = typeof(T);
            if (conversionType.IsGenericType &&
                conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                System.ComponentModel.NullableConverter nullableConverter
                    = new System.ComponentModel.NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;

                //如果是枚举
                if (typeof(T).GetGenericArguments()[0].IsEnum)
                {
                    if (value != null && !Convert.IsDBNull(value))
                    {
                        return (T)Enum.Parse(typeof(T).GetGenericArguments()[0], Convert.ToString(value));
                    }
                }
            }
            else
            {
                if (typeof(T).IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), Convert.ToString(value));
                }
            }


            return (T)Convert.ChangeType(value, conversionType);
        }
    }
}
