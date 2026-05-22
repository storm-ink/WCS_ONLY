using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 缓冲区数据对比结果处理程序接口
    /// </summary>
    public interface ICompareResultHandler
    {
        void Handle(ConveyorDevice device, CompareResult<NetTransferObject>[] compareResults);
        void Handle(ConveyorDevice device, _DB2 db2);
    }
}
