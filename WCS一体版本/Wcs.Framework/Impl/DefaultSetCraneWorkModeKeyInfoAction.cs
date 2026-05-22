using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using Wcs.Framework.Cfg;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 同步安装在输送线上的堆垛机远程控制盒信息动作.<br />
    /// 一个默认实现，在堆垛机远程控制盒信息发生变化时同步到堆垛机设备
    /// </summary>
    public class DefaultSetCraneWorkModeKeyInfoAction:UpdateStatusAction
    {
        public override void Execute(Device device,object status)
        {
            if (status == null || device as NewConveyor==null)
            {
                foreach (Crane crane in Configuration.Devices.Where(x => x.DeviceType == DeviceType.Crane).Select(x => (Crane)x))
                {
                    crane.UpdateCraneWorkModeKeyInfo(new CraneWorkModeKeyInfo
                    {
                        CraneNo = Convert.ToUInt16(crane.DeviceNo),
                        IsBrake =true,
                        IsManual=true
                    });
                }
                return;
            }

            byte[] bytes = status as byte[];
            NewConveyor conveyor = device as NewConveyor;

            CraneWorkModeKeyInfo[] craneWorkModeKeyInfos = conveyor.ReceiverDecoder.Get<CraneWorkModeKeyInfo>(bytes);
            foreach (Crane crane in Configuration.Devices.Where(x=>x.DeviceType==DeviceType.Crane).Select(x=>(Crane)x))
            {
                var craneWorkModeKeyInfo = craneWorkModeKeyInfos.SingleOrDefault(x => x.CraneNo == Convert.ToUInt16(crane.DeviceNo));
                if (craneWorkModeKeyInfo == null)
                {
                    craneWorkModeKeyInfo = new CraneWorkModeKeyInfo
                    {
                        CraneNo = Convert.ToUInt16(crane.DeviceNo),
                        IsBrake=true,
                        IsManual=true
                    };
                }
                crane.UpdateCraneWorkModeKeyInfo(craneWorkModeKeyInfo);
            }
        }
    }
}
