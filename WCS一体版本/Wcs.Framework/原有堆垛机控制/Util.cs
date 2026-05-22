using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Wcs.Framework.CraneControl
{
    public class Util
    {
        /// <summary>字符转枚举</summary>
        public static T PEnum<T>(string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }

        /// <summary>获取枚举的描述</summary>
        public static string PDescription(Enum e)
        {
            return ((DescriptionAttribute)Attribute.GetCustomAttribute(e.GetType().GetMember(e.ToString())[0], typeof(DescriptionAttribute))).Description;
        }
    }
}
