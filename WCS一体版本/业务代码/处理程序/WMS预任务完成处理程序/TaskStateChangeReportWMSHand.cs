using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.Framework;
using Wcs.FrameworkExtend;
using Wcs.FrameworkExtend.Cfg;
using static HibernatingRhinos.Profiler.Appender.Messages.MessageWrapper.Types;
using ZHQXC.WebAPI;
using Wcs;
using NHibernate.Hql.Ast;
using Wcs.DefaultImplementCollection.Conveyor;
using NHibernate.Linq;
using DevExpress.XtraEditors.Mask;
using Wcs.DefaultImplementCollection.Business;

namespace ZHQXC.PreTaskHand
{
    public class TaskStateChangeReportWMSHand : WMSPreTaskCompeletedExternalHandler
    {
        List<string> lists = new List<string>() { "1020", "1021", "1023", "1024", "1025", "1026", "1027", "1028" };

        public override void Hand(PreTask preTask)
        {
            //任务来源：WMS的任务
            if (preTask.Source == TaskSource.Wms)
            {
                string msg;
                var additionaryInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(preTask.AdditionalInfo);
                WMSTaskStatus wmsTaskState = WMSTaskStatus.start;
                if (preTask.Status == Wcs.Framework.TaskStatus.Completed)
                    wmsTaskState = WMSTaskStatus.finish;
                else if (preTask.Status == Wcs.Framework.TaskStatus.Cancelled)
                    wmsTaskState = WMSTaskStatus.cancel;
                //1、上报WMS任务状态
                try
                {
                    var result = RequestWMSHelper.EquipmentTaskStatusChangeReport(preTask.TaskCode, wmsTaskState, out msg, additionaryInfo);
                    if (result)
                    {
                        PreTaskHandHelper.PopWaitReportPreTaskList(preTask);
                        //PreTaskHandHelper.PopPreTaskList(preTask.TaskCode);
                        //3、任务进入归档列表
                        if (preTask.Status == Wcs.Framework.TaskStatus.Completed )
                        {
                            //2、给专机发送命令
                            if (lists.Contains(preTask.EndLocation.DeviceCode))
                            {
                                string trayBarcode = "";
                                UInt16 isfull = 0;
                                UInt16 _tasktype = 0;
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
                                else if (preTask.TaskType == "p2p")
                                {
                                    if (preTask.AdditionalInfo.Contains("isSelected"))
                                    {
                                        isfull = 2;
                                        _tasktype = 1;
                                    }
                                    else
                                    {
                                        isfull = 1;
                                        _tasktype = 2;
                                    }
                                   
                                }
                                else if (preTask.TaskType == "down") 
                                {
                                    
                                    isfull = 2;
                                    _tasktype = 1;
                                }
                               
                                SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                                {
                                    PosNo = (UInt16)int.Parse(preTask.EndLocation.DeviceCode),
                                    IsFull = isfull,
                                    TaskType = _tasktype,
                                    TrayBarcode = trayBarcode,
                                    DataID = (UInt16)new Random().Next(0, UInt16.MaxValue)
                                };
                                try
                                {
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
                                catch (Exception)
                                {
                                    PreTaskHandHelper.PushDeletePreTaskList(preTask);//从自动归档中添加
                                }

                            }

                            PreTaskHandHelper.PushDeletePreTaskList(preTask);//从自动归档中添加
                        }
                        else if ( preTask.Status == Wcs.Framework.TaskStatus.Cancelled)
                        {
                            PreTaskHandHelper.PushDeletePreTaskList(preTask);//从自动归档中添加
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
        }
    }
}
