using Newtonsoft.Json;
using NLog;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace ZHQXC
{
    public class TopCoverTreatmentRequestHandStartup_1018 : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Logger _logger;
        int interval = 3000;
        int waitTime = 3000;
        int loc = 1018;
        public void Initialize(StartupElement element)
        {
            interval = element.GetAttributeOrDefault<int>("interval", 3000);
            waitTime = element.GetAttributeOrDefault<int>("waitTime", 10000);
        }

        public void Run(IWcsApplication application)
        {
            _logger = LogManager.CreateNullLogger();
            this.Init($"拆盖上盖{loc}处理");
            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();
            _logger.Info1($"{this} 线程已经启动", this);
        }

        private void check(object obj)
        {
            var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV");
            ///当前设计：上报成功隔10秒再重新上报
            KeyValuePair<int, DateTime?> keyValuePair = new KeyValuePair<int, DateTime?>(0, null);
            while (true)
            {
                Thread.Sleep(interval);
                try
                {
                    this.Log("*********1018循环线程start*********");
                    if (!conveyor.IsConnected)
                    {
                        this.Log($"专机CV设备未连接");
                        this.Log("*********1018循环线程end*********");
                        continue;
                    }
                    var readData = conveyor.ReadStatus<RequestBlock>().FirstOrDefault(x => x.PosNo == loc);
                    if (readData == null)
                    {
                        this.Log($"PLC上报数据 PackingAutoScanCodeLabelingResultObject({typeof(RequestBlock).GetDisplayName()})中未找到编号是1018的数据");
                        this.Log("*********1018循环线程end*********");
                        continue;
                    }

                    if (readData.HandShake == RequestHandShakes.New && readData.IOData!=0) {

                   //     var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV");
                        SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                        {
                            PosNo = (UInt16)loc,
                            IsFull = 1,
                            TaskType = 2,//(UInt16)int.Parse(action.Movement.Task.TaskType),
                            TrayBarcode = "test",
                            DataID = (UInt16)new Random().Next(0, UInt16.MaxValue)
                        };
                        try
                        {
                         
                            //conveyor.CallSynchronousMethodWithTimeout_Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, this);
                            conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                        }
                        catch (Exception ex)
                        {
                            //PackingPLCHelper.Logger.Log($"{ex}");
                            throw ex;
                        }



                    }

                }
                catch (Exception ex)
                {
                    this.Log($"{ex}");
                    this.Log("*********循环线程end，异常end*********");
                    _logger.Error1(ex, this);
                }
            }
        }

        //private void ClearFosbScan(string scancode)
        //{
        //    Wcs.Framework.Cfg.WcsConfiguration.Instance
        //                   .DeviceCollection.ParticularDeviceCollection
        //                   .SelectMany(x => x.DeviceElements).Where(x => x.Device is ScanerDevice)
        //                   .Select(x => x.Device as ScanerDevice)
        //                   .FirstOrDefault(x => x.Name.StartsWith(scancode)).CurrentBarcode = "";
        //}

        //private List<string> ScannerReadBarcode(string scanner)
        //{
        //    var scaner = Wcs.Framework.Cfg.WcsConfiguration.Instance
        //                        .DeviceCollection.ParticularDeviceCollection
        //                        .SelectMany(x => x.DeviceElements).Where(x => x.Device is ScanerDevice)
        //                        .Select(x => x.Device as ScanerDevice)
        //                        .FirstOrDefault(x => x.Name.StartsWith(scanner));
        //    return scaner.CurrentBarcode.Split(',').ToList();//扫码值
        //}

       
    }
}
