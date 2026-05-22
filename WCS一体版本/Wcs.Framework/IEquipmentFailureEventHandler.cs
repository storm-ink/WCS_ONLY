using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示继承此接口的类型是一个设备故障处理程序
    /// </summary>
    public interface IEquipmentFailureEventHandler<TArgs>
        where TArgs:EquipmentFailureEventArgs
    {
        /// <summary>
        /// 处理设备故障被添加的事件
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        void Handle(Device device, TArgs args);
    }
}
