using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 远程控制命令返回块
    /// </summary>
    /// <remarks>
    /// 应用说明：
    /// 1.PLC完全反馈WCS发送的数据，WCS根据Data_ID判断是否发送成功
    /// 2.PLC根据Data_ID判断只响应一次
    /// </remarks>
    public class RemoteCMDBlock : NetTransferObject
    {
        /// <summary>
        /// 区域编号
        /// </summary>
        public UInt16 AreaNo { get; set; }
        /// <summary>
        /// 急停
        /// </summary>
        public Boolean Emergency { get; set; }
        /// <summary>
        /// 消音
        /// </summary>
        public Boolean Mute { get; set; }
        /// <summary>
        /// 停止
        /// </summary>
        public Boolean Stop { get; set; }
        /// <summary>
        /// 复位
        /// </summary>
        public Boolean Reset { get; set; }
        /// <summary>
        /// 启动
        /// </summary>
        public Boolean Start { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        /// <remarks>此处DataID是实时返回WCS下发的RemoteCMD DataID，供WCS判断RemoteCMD是否下发成功</remarks>
        public UInt16 DataID { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "AreaNo":
                        return AreaNo;
                    case "Emergency":
                        return Emergency;
                    case "Mute":
                        return Mute;
                    case "Stop":
                        return Stop;
                    case "Reset":
                        return Reset;
                    case "Start":
                        return Start;
                    case "DataID":
                        return DataID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "AreaNo":
                        this.AreaNo = Convert.ToUInt16(value);
                        break;
                    case "Emergency":
                        this.Emergency = Convert.ToBoolean(value);
                        break;
                    case "Mute":
                        this.Mute = Convert.ToBoolean(value);
                        break;
                    case "Stop":
                        this.Stop = Convert.ToBoolean(value);
                        break;
                    case "Reset":
                        this.Reset = Convert.ToBoolean(value);
                        break;
                    case "Start":
                        this.Start = Convert.ToBoolean(value);
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
