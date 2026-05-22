using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// Wcs内部类型转换器
    /// </summary>
    public interface IWcsTypeConverter
    {
        TDevice ToDevice<TDevice>(String name);
        Wcs.Framework.Devices.Location ToLocation(String deivceCode);
    }
}
