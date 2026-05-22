using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Devices.Conveyor
{
    /// <summary>
    /// 一个特殊的传输对象。<br />
    /// 用于输送线设备的状态数据传输。
    /// </summary>
    public class _DB1 : NetTransferObject
    {
        public Dictionary<Type, NetTransferObject[]> Items { get; set; }

        public override object this[string name]
        {
            set { throw new NotImplementedException(); }
        }

        public T[] Get<T>() where T:NetTransferObject
        {
            if (Items.ContainsKey(typeof(T)))
            {
                return (T[])Items[typeof(T)];
            }
            else
            {
                return new T[0];
            }
        }
    }
}
