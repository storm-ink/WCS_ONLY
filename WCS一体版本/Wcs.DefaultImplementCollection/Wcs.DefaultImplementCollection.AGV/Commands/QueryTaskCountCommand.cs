using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.AGV
{
    /// <summary>
    /// 查询任务数量
    /// </summary>
    public class QueryTaskCountCommand
    {
        public QueryTaskCountCommand()
        { }

        // <Req>
        //     <Order Num />
        // </Req>
        public String ToXml()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<Req>");
            stringBuilder.AppendFormat("<Order Num />");
            stringBuilder.Append("</Req>");

            return stringBuilder.ToString();
        }
    }
}
