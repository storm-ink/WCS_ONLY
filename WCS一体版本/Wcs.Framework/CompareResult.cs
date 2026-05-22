using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wcs.Framework
{

    /// <summary>
    /// 两个对象的比较结果
    /// </summary>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    public class CompareResult<T>
    {
        /// <summary>
        /// 指示该对比结果参与对比的对象类型是否是指定的类型
        /// </summary>
        /// <param name="type">指定的类型</param>
        /// <returns></returns>
        public Boolean IsTypeOf(Type type)
        {
            var compareType=((object)newObject ?? (object)oldObject).GetType();
            if(type==compareType)
            {
                return true;
            }

            return compareType.IsSubclassOf(type);
            //return ((object)newObject ?? (object)oldObject).GetType() == type;
        }
        /// <summary>
        /// 获取或设置对象差异集合
        /// </summary>
        /// <value>
        /// 差异集合
        /// </value>
        public Different[] differences { get; set; }
        /// <summary>
        /// 获取或设置旧数据对象
        /// </summary>
        public T oldObject { get; set; }
        /// <summary>
        /// 获取或设置新数据对象
        /// </summary>
        public T newObject { get; set; }

        /// <summary>
        /// 返回表示当前 <see cref="T:System.Object" /> 的 <see cref="T:System.String" />。.<br />
        /// 形式如下：<br />
        ///     propertyName1/Description/DisplayName:propertyValue.ToString()<br />
        ///     propertyName2/Description/DisplayName:propertyValue.ToString()<br />
        ///     propertyName3/Description/DisplayName:propertyValue.ToString()<br />
        ///     .....<br />
        ///     propertyNameN/Description/DisplayName:propertyValue.ToString()<br />
        /// 即:<br />
        ///     默认为属性名称：属性值<br />
        ///     但是，当属性包含 <see cref="T:System.ComponentModel.DescriptionAttribute"/> 时，将显示 <see cref="T:System.ComponentModel.DescriptionAttribute.Description"/> :属性值<br />
        ///     或者，当属性包含 <see cref="T:System.ComponentModel.DisplayNameAttribute"/> 时，将显示 <see cref="T:System.ComponentModel.DisplayNameAttribute.Description"/> :属性值<br />
        /// </summary>
        /// <returns>
        /// <see cref="T:System.String" />，表示当前的 <see cref="T:System.Object" />。.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propertyInfos;
            if (oldObject != null)
            {
                propertyInfos = oldObject.GetType().GetProperties();
            }
            else if (newObject != null)
            {
                propertyInfos = newObject.GetType().GetProperties();
            }
            else
            {
                propertyInfos = typeof(T).GetProperties();
            }
            int maxNameLength = propertyInfos.Max(x => x.GetDisplayName().GetASCIILength());
            foreach (var property in propertyInfos)
            {
                //不处理索引器
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                Different differnt = differences.SingleOrDefault(x => x.propertyName == property.Name);
                if (differnt == null)
                {
                    sb.AppendFormat("{0}: {1}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '), getValueDescription(property.PropertyType, property.GetValue(oldObject, null)));
                }
                else
                {
                    sb.AppendFormat("{0}: {1} -> {2}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '), getValueDescription(differnt.valueType, differnt.oldValue), getValueDescription(differnt.valueType, differnt.newValue));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <returns></returns>
        public Type GetDataType()
        {
            return ((object)newObject ?? (object)oldObject).GetType();
        }

        /// <summary>
        /// 一个辅助性函数，用于获取指定类型的值描述信息的表现形式
        /// </summary>
        /// <param name="valueType">    值的类型. </param>
        /// <param name="value">        值. </param>
        /// <returns>
        /// 值的描述性形式。由 DescriptionAttribute.Description 或 DisplayNameAttribute.DisplayName 决定
        /// </returns>
        private string getValueDescription(Type valueType, dynamic value)
        {
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
                if (getDescriptionMethod == null)
                {
                    return Convert.ToString(value) + "";
                }

                return getDescriptionMethod.Invoke(value, new object[] { value });
            }
            else
            {
                return Convert.ToString(value) + "";
            }
        }
    }
}
