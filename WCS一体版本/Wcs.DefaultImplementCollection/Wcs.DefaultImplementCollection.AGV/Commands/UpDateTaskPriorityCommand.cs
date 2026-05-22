using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 更新任务优先级
    /// </summary>
    public class UpDateTaskPriorityCommand
    {
        public String TaskCode { get; set; }
        public String Index { get; set; }

        public UpDateTaskPriorityCommand()
        { }

        public UpDateTaskPriorityCommand(String taskCode, String index)
        {
            this.TaskCode = taskCode;
            this.Index = index;
        }

        // <Req>
        //     <Order No="" IKey=""/>
        // </Req>
        public virtual String ToXml()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<Req>");
            stringBuilder.AppendFormat("<Order No='{0}' IKey='{1}'/>", this.TaskCode, this.Index);
            stringBuilder.Append("</Req>");

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return String.Format("任务号 {0} 任务索引 {1}", this.TaskCode, this.Index);
        }
    }
}
