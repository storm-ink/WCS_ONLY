using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wcs.Framework
{
    /// <summary>
    /// 数据对比接口
    /// </summary>
    /// <typeparam name="TResult">  比较结果的数据类型 </typeparam>
    /// <typeparam name="TCompare"> 比较时使用的数据类型 </typeparam>
    public interface IDataComparer<TResult,TCompare>
    {
        /// <summary>
        /// 当发现传入的数据和旧的数据有变化时发生
        /// </summary>
        event DataChangedEventHandler<TResult> DataChanged;
        /// <summary>
        /// 将指定数据和原来旧的数据进行比较
        /// </summary>
        /// <param name="oldData"> 旧数据 </param>
        /// <param name="oldData"> 新数据 </param>
        /// <returns>
        /// 当前对象和指定对象的差异属性集合
        /// </returns>
        CompareResult<TResult>[] Compare(TCompare oldData, TCompare newData);
    }
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
            if(propertyInfo==null)
            {
                return ToString();
            }

            var value = (newValue ?? oldValue);
            if (value is Enum)
            {
#warning 此处反射引用，修改 dll 名字将引发异常
                var extentionMethod = Type.GetType("Wcs.EnumExtentions, Wcs").GetMethod("GetDescription").MakeGenericMethod(valueType);
                return String.Format("{0}:{1} -> {2}", propertyInfo.GetDisplayName(), oldValue == null ? "<null>" : extentionMethod.Invoke(oldValue, new object[] { oldValue }), newValue == null ? "<null>" : extentionMethod.Invoke(newValue, new object[] { newValue }));
            }
            else
            {
                return String.Format("{0}:{1} -> {2}", propertyInfo.GetDisplayName(), oldValue ?? "<null>", newValue ?? "<null>");
            }
        }
    }
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
            return ((object)newObject ?? (object)oldObject).GetType() == type;
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
        /// 用来做为某些操作的辅助标记，标记此差异是否已被某些程序处理过<br />
        /// 视情况由开发者决定怎么使用
        /// </summary>
        public Boolean Handled { get; set; }

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
                    sb.AppendFormat("{0}: {1}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '),getValueDescription(property.PropertyType, property.GetValue(oldObject, null)));
                }
                else
                {
                    sb.AppendFormat("{0}: {1} -> {2}\r\n", property.GetDisplayName().ASSICPadRight(maxNameLength, ' '), getValueDescription(differnt.valueType,differnt.oldValue), getValueDescription(differnt.valueType,differnt.newValue));
                }
            }

            return sb.ToString();
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

#warning 此处反射引用，修改 dll 名字将引发异常
                MethodInfo mi = Type.GetType("Wcs.EnumExtentions, Wcs").GetMethod("GetDescription").MakeGenericMethod(valueType);

                if (mi == null)
                {
                    return string.Format("{0}", value ?? (object)"");
                }

                return mi.Invoke(value, new object[] { value });
            }
            else
            {
                return string.Format("{0}", value ?? (object)"");
            }
        }
    }
}
