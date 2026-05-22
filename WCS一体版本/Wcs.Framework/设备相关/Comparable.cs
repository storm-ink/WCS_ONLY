using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace Wcs.Framework.Devices
{
    /// <summary>
    /// 可对比的数据类型
    /// </summary>
    /// <typeparam name="T">    泛型参数. </typeparam>
    [DataContract]
    public abstract class Comparable<T> where T : Comparable<T>
    {
        /// <summary>
        /// 将指定的对象和当前对象的所有属性进行比较，并返回两个对象的差异数据集合.
        /// 该操作会枚举对象的所的属性，逐一进行比较。如果属性类型实现了 <see cref="T:System.Collections.Generic.IComparer"/> 接口，将调用 <see cref="T:System.Collections.Generic.IComparer.Compare"/> 进行比较.否则使用 <see cref="T:System.Object"/>.<see cref="T:System.Object.Equals"/> 方法进行对比
        /// </summary>
        /// <exception cref="ArgumentNullException">newObj 为 null 时将抛出此异常.</exception>
        /// <param name="newObj">要进行比较的对象 </param>
        /// <returns>
        /// 当前对象和指定对象的差异属性集合
        /// </returns>
        public virtual CompareResult<T> Compare(T newObj)
        {
            if (newObj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (newObj.GetType() != this.GetType())
            {
                throw new ArgumentNullException(String.Format("{0} 无法和 {1} 对比", newObj.GetType(), this.GetType()));
            }

            List<Different> differences = new List<Different>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (var property in propertyInfos)
            {
                //不处理索引器
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                object oldValue = property.GetValue(this, null);
                PropertyInfo newObjectProperty = newObj.GetType().GetProperty(property.Name);
                object newValue = newObjectProperty.GetValue(newObj, null);
                if (oldValue == newValue) continue;

                if (oldValue == null || newValue == null)
                {
                    differences.Add(new Different
                    {
                        newValue = newValue,
                        oldValue = oldValue,
                        propertyName = property.Name,
                        valueType = property.PropertyType,
                        propertyInfo =property
                    });
                    continue;
                }

                //处理集合
                if (property.PropertyType.GetInterfaces().Any(x=>x == (typeof(System.Collections.IEnumerable))))
                {
                    var oldEnumerable = ((System.Collections.IEnumerable)oldValue).Cast<object>();
                    var newEnumerable = ((System.Collections.IEnumerable)newValue).Cast<object>();
                    if (!oldEnumerable.SequenceEqual(newEnumerable, new EnumerableEqualityComparer()))
                    {
                        differences.Add(new Different
                        {
                            newValue = newValue,
                            oldValue = oldValue,
                            propertyName = property.Name,
                            valueType = property.PropertyType,
                            propertyInfo = property
                        });
                    }
                }
                else
                {

                    Type comparerInterface = property.PropertyType.GetInterface("IComparer`1");
                    if (comparerInterface != null)
                    {
                        var compareMethod = comparerInterface.GetMethod("Compare", new Type[2] { property.PropertyType, property.PropertyType });
                        int result = (int)compareMethod.Invoke(oldValue ?? newValue, new object[] { oldValue, newValue });
                        if (result != 0)
                        {
                            differences.Add(new Different
                            {
                                newValue = newValue,
                                oldValue = oldValue,
                                propertyName = property.Name,
                                valueType = property.PropertyType,
                                propertyInfo = property
                            });
                        }
                    }
                    else
                    {
                        if (!object.Equals(oldValue, newValue))
                        {
                            differences.Add(new Different
                            {
                                newValue = newValue,
                                oldValue = oldValue,
                                propertyName = property.Name,
                                valueType = property.PropertyType,
                                propertyInfo = property
                            });
                        }
                    }
                }
            }

            CompareResult<T> cr = new CompareResult<T>();
            cr.oldObject = (T)this;
            cr.newObject = newObj;
            cr.differences = differences.ToArray();

            return cr;
        }

        private class EnumerableEqualityComparer : IEqualityComparer<object>
        {
            public bool Equals(object x, object y)
            {
                if (x == y) return true;

                if (x == null && y != null)
                {
                    return false;
                }

                if (x != null && y == null)
                {
                    return false;
                }

                Type type = (x ?? y).GetType();

                Type comparerInterface = type.GetInterface("IComparer`1");
                if (comparerInterface != null)
                {
                    var compareMethod = comparerInterface.GetMethod("Compare", new Type[2] { type, type });
                    int result = (int)compareMethod.Invoke(x ?? y, new object[] { x, y });
                    return result == 0;
                }
                else
                {
                    return object.Equals(x, y);
                }
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
