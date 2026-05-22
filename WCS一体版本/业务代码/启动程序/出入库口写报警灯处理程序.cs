using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wcs;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace BOE
{
    public class 出入库口写报警灯处理程序 : ThreadRunningLog, IApplicationStartup
    {
        static Thread _thread;
        Int32 _interval;
        Logger _logger;
        string _deviceName;
        AlivePlatformCommand cmd;
       
        Wcs.Framework.Cfg.StartupElement _element;
        public void Initialize(StartupElement element)
        {
            _logger = LogManager.GetCurrentClassLogger();
            
            _element = element;
        }

        public void Run(IWcsApplication application)
        {
            _interval = _element.GetAttributeOrDefault<Int32>("interval", 5000);
            this.Init("写报警信息处理线程");
            ParameterizedThreadStart Start = new ParameterizedThreadStart(check);
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
            this._logger.Debug1(String.Format("{0} 线程已经启动！", this), this);
            this.Log($"{this} 线程已经启动！");
        }

        public void check(object o)
        {
            while (true)
            {                
                try
                {
                    List<Task> tasks;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        tasks = unitOfWork.session.Query<Wcs.Framework.Task>().Where(x => x.TaskType == "尺检失败回退" && x.Status != TaskStatus.Completed && x.Status != TaskStatus.Cancelled).ToList();
                        unitOfWork.Commit();
                    }
                    foreach (Task task in tasks)
                    {
                        var device = DeviceConverter.ToDevice<ConveyorDevice>(task.EndLocation.DeviceName);                       
                        var result = device.ReadStatus<WarningInfoNetTransferObject>().FirstOrDefault(x => x.PosNo == Convert.ToUInt16(task.EndLocation.DeviceCode));
                        SendWarningInfoCommand cmd = new SendWarningInfoCommand();
                        if (task.AdditionalInfo.ContainsKey("SHAPECHECK_RESULT") && task.AdditionalInfo["SHAPECHECK_RESULT"]!="OK")
                        {
                            cmd.PosNo = Convert.ToUInt16(task.EndLocation.DeviceCode);
                            cmd.WarningInfo = 1;
                        }                       
                       
                        if (result != null && result.WarningInfo != cmd.WarningInfo)
                        {                           
                            device.Write<SendWarningInfoCommand>(cmd, cmd.SendSuccess);
                        }

                    }
                }
                catch(Exception ex)
                {
                    this.Log(ex.Message);
                }
                Thread.Sleep(_interval);
            }
        }
    }
}
