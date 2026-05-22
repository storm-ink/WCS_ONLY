using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public class BYCommandReplyTelexTransferObject : TelexTransferObject
    {
        public BYCommandReplyTelexTransferObject()
            : base()
        { }

        public BYCommandReplyTelexTransferObject(byte[] bytes)
        {
            if (!ValidateType(bytes))
                throw new InvalidTelexTransferObjectException<RequestStateCommandReplyTelexTransferObject>(bytes.Reverse().Select(x => x.ToString("X2")).ToString());

            try
            {
                HexTelexStr = bytes.Reverse().Select(x => x.ToString("X2")).ToString();

                this.TaskId = Convert.ToUInt32(bytes.Skip(14).Take(4).Reverse());
                this.ErrorCode = Convert.ToUInt16(bytes.Skip(18).Take(2).Reverse());
            }
            catch (Exception ex)
            {
                throw new InvalidTelexTransferObjectException<RequestStateCommandReplyTelexTransferObject>(bytes.Reverse().Select(x => x.ToString("X2")).ToString(), ex);
            }
        }

        public UInt32 TaskId { get; set; }

        public UInt16 ErrorCode { get; set; }

        public string HexTelexStr { get; set; }
        public override object this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string TypeFlag { get; set; } = "YB";

        public override int Length => 20;

        public override byte[] ToTelex()
        {
            throw new NotImplementedException();
        }
    }
}
