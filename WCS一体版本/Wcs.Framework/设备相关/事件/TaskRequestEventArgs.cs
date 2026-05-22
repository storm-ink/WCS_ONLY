using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework
{

    public class TaskRequestEventArgs : HandleableEventArgs
    {
        /// <summary>
        /// 任务请求的发生位置
        /// </summary>
        public Location Location { get; private set; }
        /// <summary>
        /// 请求源数据
        /// </summary>
        public dynamic SourceData{get;private set;}
        public TaskRequestEventArgs(Location location,dynamic sourceData)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }
            this.Location = location;
            this.SourceData = sourceData;
        }
    }
}
