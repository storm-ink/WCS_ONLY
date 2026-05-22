using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using Wcs.DefaultImplementCollection.Conveyor;
using NHibernate.Linq;
using Wcs.FrameworkExtend;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Wcs.FrameworkExtend.Cfg;
using DevExpress.XtraEditors.Mask;

namespace ZHQXC
{
    /// <summary>
    /// 对到达专机的输送线任务完成时，需要给专机传送到达命令
    /// </summary>
    public class SpecialAircraftEquipmentTaskCompelteHand : WMSPreTaskCompeletedExternalHandler
    {
        List<string> lists = new List<string>() { "1020", "1021", "1023", "1024", "1025", "1026", "1027", "1028" };
        public override void Hand(PreTask preTask)
        {
            try
            {
                if (lists.Contains(preTask.EndLocation.DeviceCode))
                {
                    string trayBarcode = "";
                    UInt16 isfull = 0;
                    UInt16 _tasktype = 0;
                    var additionaryInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(preTask.AdditionalInfo);
                    if (additionaryInfo.ContainsKey("TrayBarcode"))
                        trayBarcode = additionaryInfo["TrayBarcode"];
                    if (preTask.TaskType == "满桶入库")
                    {
                        isfull = 2;
                        _tasktype = 3;
                    }
                    else if (preTask.TaskType == "box_supply")
                    {
                        isfull = 1;
                        _tasktype = 2;
                    }
                    else if(preTask.TaskType == "p2p")
                    {
                        isfull = 1;
                        _tasktype = 2;
                    }
                    SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                    {
                        PosNo = (UInt16)int.Parse(preTask.EndLocation.DeviceCode),
                        IsFull = isfull,
                        TaskType = _tasktype,
                        TrayBarcode = trayBarcode,
                        DataID = (UInt16)new Random().Next(0, UInt16.MaxValue)
                    };
                    if (preTask.EndLocation.DeviceCode == "1021" || preTask.EndLocation.DeviceCode == "1020")
                    {
                        var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV");
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    else if (preTask.EndLocation.DeviceCode == "1023" || preTask.EndLocation.DeviceCode == "1024")
                    {
                        var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(A)");
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    else if (preTask.EndLocation.DeviceCode == "1025" || preTask.EndLocation.DeviceCode == "1026")
                    {
                        var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(B)");
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    else if (preTask.EndLocation.DeviceCode == "1027" || preTask.EndLocation.DeviceCode == "1028")
                    {
                        var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV(C)");
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
