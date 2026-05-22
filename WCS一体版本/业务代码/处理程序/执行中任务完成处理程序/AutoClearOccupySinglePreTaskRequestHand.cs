using DevExpress.Utils.DragDrop;
using DevExpress.Utils.Drawing.Helpers;
using Newtonsoft.Json;
using NHibernate.Linq;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImplementCollection.Business;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.DefaultImplementCollection.Scanner;
using Wcs.Framework;
using Wcs.FrameworkExtend;
using ZHQXC.WebAPI;
using ZHQXC.WebAPI.Entity;

namespace ZHQXC
{
    public class AutoClearOccupySinglePreTaskRequestHand : WCSPreTaskRequestHandler
    {
        public override void Hand(PreTask preTask)
        {
            try
            {
                var additionaryInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(preTask.AdditionalInfo);
                if (preTask.TaskType == "入库口前置任务" && preTask.Status == Wcs.Framework.TaskStatus.Completed)
                {
                    _logger.Trace1(string.Format("准备将{0}任务{1}请求WMS...", preTask.TaskType, preTask), this, preTask);
                    RequestBlock request = new RequestBlock();
                    var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("立库CV");
                    if (!conveyor.IsConnected)
                    {
                        return;
                    }
                    string barcode = "";
                    string isContainer = "";
                    string msg = "";
                    List<string> ScanList = ScannerReadBarcode("1502扫码器"); 
                    if (ScanList.Count != 0)
                    {
                        barcode = string.Join(",", ScanList);
                    }
                    var readData_barcode = conveyor.ReadStatus<ShapeCheckBlock>().FirstOrDefault(x => x.PosNo == Convert.ToUInt16(preTask.StartLocation.DeviceCode));
                    if (readData_barcode.ShapeState == ShapeSatus.OK)
                    {
                        isContainer = "Y";
                    }
                    else
                    {
                        isContainer = "N";
                    }
                    var result = request.ScanCodeRequest("00-001-1502", barcode, "", isContainer, false, out msg);
                    if (!result)
                    {
                        String taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                        TaskSource _source = TaskSource.Wcs;
                        String __taskType = "外检失败任务回退";
                        Location _start = LocationConverter.UserCodeToLcation("00-001-1502");
                        Location _end = LocationConverter.UserCodeToLcation("00-001-1504");
                        PreTask preTask_return = new PreTask(taskCode, LocationConverter.ToLocationInfo(_start), LocationConverter.ToLocationInfo(_end))
                        {
                            Source = _source,
                            TaskType = __taskType
                        };  
                        //第六步：下发任务
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            unitOfWork.session.Save(preTask_return);
                            unitOfWork.Commit();
                        }
                        Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(new Wcs.FrameworkExtend.Events.PreTaskAddedEvent(preTask_return));
                    }
                    PreTaskHandHelper.PopWaitRequestPreTaskList(preTask);
                    PreTaskHandHelper.PushDeletePreTaskList(preTask);

                }
                else
                {
                    PreTaskHandHelper.PopWaitRequestPreTaskList(preTask);
                    PreTaskHandHelper.PushDeletePreTaskList(preTask);
                }
            }
            catch (Exception ex)
            {
                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                                   new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                                        String.Format("向WMS请求"),
                                        String.Format("向WMS请求失败，详细信息：{0}", ex.Message),
                                        null));
                _logger.Error1(ex, this, preTask);
            }
        }


        private List<string> ScannerReadBarcode(string scanner)
        {
            var scaner = Wcs.Framework.Cfg.WcsConfiguration.Instance
                                .DeviceCollection.ParticularDeviceCollection
                                .SelectMany(x => x.DeviceElements).Where(x => x.Device is ScanerDevice)
                                .Select(x => x.Device as ScanerDevice)
                                .FirstOrDefault(x => x.Name.StartsWith(scanner));
            return scaner.CurrentBarcode.Split(',').ToList();//扫码值
        }
    }


}
