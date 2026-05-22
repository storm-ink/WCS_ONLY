using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 应在与输送线通讯的块（DB2中将该类型加上）
    /// </summary>    
#warning 请复制已下代码到 db2.xml 当中

    [DisplayName("堆垛机远程控制盒")]

    public class WithRemoteControllerCraneDeviceNetTransferObject:Wcs.Framework.NetTransferObject
   {
        /// <summary>
        /// 设备编号
        /// </summary>  
        [DisplayName("设备编号")]

        public UInt16 DeviceNo { get; set; }

        /// <summary>
        /// 是否急停
        /// </summary>
        [DisplayName("是否急停")]
        public bool IsEmergency{ get; set; }

        /// <summary>
        /// 是否手动
        /// </summary>
        [DisplayName("是否手动")]
        public bool IsManual { get; set; }


       public override object this[string name]
       {
           get
           {
               switch (name)
               {
                   case "DeviceNo":
                       return this.DeviceNo;
                   case "IsEmergency":
                       return this.IsEmergency;
                   case "IsManual":
                       return this.IsManual;
                 
                   default:
                       throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
               }
           }
           set
           {
               switch (name)
               {
                   case "DeviceNo":
                       this.DeviceNo = Convert.ToUInt16(value);
                       break;
                   case "IsEmergency":
                       this.IsEmergency = Convert.ToBoolean(value);
                       break;
                   case "IsManual":
                       this.IsManual = Convert.ToBoolean(value);
                       break;
             
                   default:
                       throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
               }
           }
       }
	   
       public override Wcs.Framework.ReceivedDataLog ToLogData(Wcs.Framework.Device device)
       {
           return null;
       }
    }
}
