using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个发送 <see cref="T:AddTaskCommand"/> 命令给堆垛机后，堆垛机的应答状态报文
    /// </summary>
    public class AddTaskCommandReplyTelexTransferObject : TelexTransferObject
    {
        public UInt16 fff = 0;
        protected String _telex;
        public AddTaskCommandReplyTelexTransferObjectResult Result { get;  set; }
        public String TaskId { get;  set; }
        public AddTaskCommandReplyTelexTransferObject()
            : base()
        {
        }
        public AddTaskCommandReplyTelexTransferObject(String telex)
        {
            if (!ValidateType(telex))
            {
                throw new InvalidTelexTransferObjectException<AddTaskCommandReplyTelexTransferObject>(telex);
            }
            try
            {
                this.Result = Utils.ConvertTo<AddTaskCommandReplyTelexTransferObjectResult>(telex.Substring(3, 1));
                this.TaskId = telex.Substring(4, 8);
                _telex = telex;
            }
            catch (Exception ex)
            {
                throw new InvalidTelexTransferObjectException<AddTaskCommandReplyTelexTransferObject>(telex, ex);
            }
        }
        public override byte[] ToTelex()
        {
            return new byte[0];
        }

        public override string TypeFlag
        {
            get;set;
        }

        public override int Length
        {
            get { return 13; }
        }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Result":
                        return this.Result;
                    case "TaskId":
                        return this.TaskId;
                    case "TypeFlag":
                        return this.TypeFlag;
                    case "Length":
                        return this.Length;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set { throw new NotImplementedException(); }
        }
    }
}
