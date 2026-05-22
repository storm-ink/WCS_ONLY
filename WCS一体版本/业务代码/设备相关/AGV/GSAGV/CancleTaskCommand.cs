using System;
using System.Text;
using Wcs.Framework;

namespace BOE.设备相关.AGV.GSAGV
{
    /// <summary>
    /// 取消任务
    /// </summary>
    public class CancleTaskCommand : DeviceCommand
    {
        public String TaskCode { get; set; }
        public String Index { get; set; }

        public CancleTaskCommand()
        { }

        public CancleTaskCommand(String taskCode, String index)
        {
            this.TaskCode = taskCode;
            this.Index = index;
        }

        public void SendCancleTask()
        {

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

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            return true;
        }
    }
}
