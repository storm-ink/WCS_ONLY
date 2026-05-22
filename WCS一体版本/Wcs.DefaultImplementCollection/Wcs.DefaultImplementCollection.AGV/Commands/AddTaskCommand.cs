using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 增加一条新任务
    /// </summary>
    public class AddTaskCommand : DeviceCommand,IDeviceCommandAdjudicator
    {
        public AddTaskCommand()
        { }

        public AddTaskCommand(String taskCode, AGVSubSystemLocation from, AGVSubSystemLocation to, Int32 priority, String extend1, String extend2, String extend3)
        {
            this.TaskCode = taskCode;
            this.From = from;
            this.To = to;
            this.Priority = priority;
            this.Extend1 = extend1;
            this.Extend2 = extend2;
            this.Extend3 = extend3;
        }

        public String TaskCode { get; set; }

        public AGVSubSystemLocation From { get; set; }

        public AGVSubSystemLocation To { get; set; }

        public Int32 Priority { get; set; }

        public String Extend1 { get; set; }

        public String Extend2 { get; set; }

        public String Extend3 { get; set; }

        // <Req>
        //     <Order Pri='' From='' To='' No='' Ext1='' Ext2='' Ext3=''/>
        //     ...
        // </Req>
        public virtual String ToXml()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<Req>");
            stringBuilder.AppendFormat("<Order Pri='{0}' From='{1}' To='{2}' No='{3}' Ext1='{4}' Ext2='{5}' Ext3='{6}'/>", this.Priority, this.From.StationNo, this.To.StationNo, this.TaskCode, this.Extend1, this.Extend2, this.Extend3);
            stringBuilder.Append("</Req>");

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return String.Format("任务号 {0} 任务起点 {1} 任务终点 {2} 任务优先级 {3} 扩展属性1 {4} 扩展属性2 {5} 扩展属性3 {6}", this.TaskCode, this.From.StationNo, this.To.StationNo, this.Priority, this.Extend1, this.Extend2, this.Extend3);
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

        public AddTaskResponse AddTaskResponse { get; set; }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            if (this.AddTaskResponse != null && this.AddTaskResponse.ErrorCode == 0)
                return true;
            else
                return false;
        }
    }
}
