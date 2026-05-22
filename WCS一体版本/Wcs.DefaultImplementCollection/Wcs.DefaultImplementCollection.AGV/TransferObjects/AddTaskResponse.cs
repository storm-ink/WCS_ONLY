using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.AGV
{
    // <Res>
    //    <ErrCode>0</ErrCode>
    //    <ErrInfo>strErrInfo</ErrInfo> // ErrCode不为0时，存在错误信息
    //    <Result>
    //         <Data IKey=""/>
    //    </Result>
    // </Res>
    //      ErrCode：错误码，为0表述无错误
    //      ErrInfo：错误信息，ErrCode≠0时有效
    //      Data:
    //      IKey: 返回AGV管理系统的任务索引，一条Order对应一个Data

    public class AddTaskResponse
    {
        public Int32 ErrorCode { get; set; }
        public String ErrorInfo { get; set; }

        public AddTaskResult AddTaskResult { get; set; }

        public AddTaskResponse(String responseXml)
        { 
            
        }
    }
}
