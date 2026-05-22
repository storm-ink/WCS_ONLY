using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{
    /// <summary>
    /// 数据级别
    /// </summary>
    /// <remarks>
    /// 用于标识数据的级别，方便对数据进行分类查询。
    /// </remarks>
    public enum ReceivedDataLogLevel
    {
        /// <summary>
        /// 默认。表示数据处于正常状态
        /// </summary>
        Normal,
        /// <summary>
        /// 警告。表示设备回报了一个警告信息，但设备仍处于正常动作、状态可控的范围内
        /// </summary>
        Warn,
        /// <summary>
        /// 错误。表示设备回报了一个严重的错误信息，设备已停机或任务都已无法下发，需要人工干预。
        /// </summary>
        Error
    }
}
