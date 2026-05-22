using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 请求设备报告当前状态
    /// </summary>
    public class RequestStateCommand : TelexTransferObject
    {
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
            get { return 4; }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
