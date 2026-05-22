using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 表示一个ROBOT状态上报报文
    /// </summary>
    public class RobotStatusTelexTransferObject : TelexTransferObject
    {
        public RobotBasicMessage RobotBasicMessage { get; set; }
        /// <summary>
        /// 机械手报警数据（润西转换成16进制字符串为机械手报警代码）
        /// </summary>
        public UInt16[] AlarmList { get; set; }
        /// <summary>
        /// 机械手站点数据
        /// </summary>
        //public RobotStationState[] RobotStationStatus { get; set; }

        /// <summary>
        /// 机械手任务数据
        /// </summary>
        public RobotTaskTransferObject RobotTask { get; set; }
        /// <summary>
        /// 机械手干接点数据
        /// </summary>
        //public RoboPickAndPutransferObject RobotPickPutData { get; set; }

        /// <summary>
        /// 接收数据缓存
        /// </summary>
        [XmlIgnore]
        public byte[] bytes;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotStatusTelexTransferObject()
            : base()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotStatusTelexTransferObject(byte[] bytes) : base()
        {
            this.bytes = bytes;
            if (!ValidateType(bytes))
            {
                throw new Exception("无效的报文传输对象");
            }
            try
            {
                this.RobotBasicMessage = new RobotBasicMessage(bytes.Skip(1).Take(7).ToArray());
                this.RobotTask = new RobotTaskTransferObject(bytes.Skip(8).Take(36).ToArray());
                string s = Encoding.ASCII.GetString(bytes.Skip(44).Take(2).ToArray()).Trim();
                this.AlarmList = new UInt16[10];
                this.AlarmList[0] = UInt16.Parse(s);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override int Length
        {
            get
            {
                return 47;
            }
        }

        public override string TypeFlag { get; set; }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }

        }

        //public override Framework.ReceivedDataLog ToLogData(Framework.Device device)
        //{
        //    return null;
        //}

        public override byte[] ToTelex()
        {
            return bytes;
        }
    }
}
