using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework.Exceptions
{
    /// <summary>
    /// 未找到 <see cref="T:Wcs.Framework.Task"/> 时引发的异常
    /// </summary>
    [Serializable]
    public class TaskNotFoundException:Exception
    {
        public Int32 Id { get; set; }
        public String TaskCode { get; set; }
        public TaskNotFoundException(Int32 id)
            :base(string.Format("未找到 id 为 {0} 的任务",id))
        {
            this.Id = id;
        }

        public TaskNotFoundException(String taskCode)
            : base(string.Format("未找到任务号为 {0} 的任务", taskCode))
        {
            this.TaskCode = taskCode;
        }

        public TaskNotFoundException(Int32 id,String taskCode)
            : base(string.Format("未找到任务 {0}#{1}", id,taskCode))
        {
            this.Id = id;
            this.TaskCode = taskCode;
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
