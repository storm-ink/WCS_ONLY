using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wcs.Framework
{

    /// <summary>
    /// 差异对象.即两个相当类型的对象进行比较之后的属性差异数据
    /// </summary>
    public class Different
    {
        /// <summary>
        /// 属性的值类型
        /// </summary>
        public Type valueType { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public String propertyName { get; set; }
        /// <summary>
        /// 原对象的值，a.compare(b) 通常指的是 a 的值.
        /// </summary>
        public dynamic oldValue { get; set; }
        /// <summary>
        /// 原对象的值，a.compare(b) 通常指的是 b 的值.
        /// </summary>
        public dynamic newValue { get; set; }
        /// <summary>
        /// 属性对象
        /// </summary>
        public PropertyInfo propertyInfo { get; set; }
        /// <summary>
        /// 返回表示当前 <see cref="T:System.Object" /> 的 <see cref="T:System.String" />。.
        /// </summary>
        /// <returns>
        /// <see cref="T:System.String" />，表示当前的 <see cref="T:System.Object" />。.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}:{1} -> {2}", this.propertyName, oldValue ?? "<null>", newValue ?? "<null>");
        }
        /// <summary>
        /// 将当前对象转换为可读的描述性信息
        /// </summary>
        /// <returns>
        /// 描述性文本
        /// </returns>
        public virtual string ToReadableDescription()
        {
            if (propertyInfo == null)
            {
                return ToString();
            }

            var value = (newValue ?? oldValue);
            if (value is Enum)
            {
                string enumExtentionTypeName = "Wcs.EnumExtentions, Wcs";
                var enumExtentionType = Type.GetType(enumExtentionTypeName);
                if (enumExtentionType == null)
                {
                    throw new EntryPointNotFoundException("未找到类型 " + enumExtentionTypeName);
                }
                var getDescriptionMethod = enumExtentionType.GetMethod("GetDescription");
                if (getDescriptionMethod == null)
                {
                    throw new EntryPointNotFoundException(string.Format("未找到类型 {0} 的 GetDescription 方法", enumExtentionType));
                }

                getDescriptionMethod = getDescriptionMethod.MakeGenericMethod(valueType);
                return String.Format("{0}:{1} -> {2}", propertyInfo.GetDisplayName(), oldValue == null ? "<null>" : getDescriptionMethod.Invoke(oldValue, new object[] { oldValue }), newValue == null ? "<null>" : getDescriptionMethod.Invoke(newValue, new object[] { newValue }));
            }
            else
            {
                return String.Format("{0}:{1} -> {2}", propertyInfo.GetDisplayName(), oldValue ?? "<null>", newValue ?? "<null>");
            }
        }
    }
}
