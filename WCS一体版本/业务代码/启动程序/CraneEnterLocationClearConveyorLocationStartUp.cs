using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using Wcs.DefaultImpls.Conveyor;
using System.Threading;
using Wcs.DefaultImpls.Crane;
using NLog;
using NHibernate.Linq;

namespace BOE
{
    /// <summary>
    /// 堆垛机入口输送线货位任务清除辅助程序
    /// </summary>
    public class CraneEnterLocationClearConveyorLocationStartUp : IApplicationStartup
    {
        static Thread _thread;
        Int32 _interval;
        Logger _logger = LogManager.CreateNullLogger();
        Random _random = new Random();

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _interval = element.GetAttributeOrDefault<Int32>("interval", 3000);
        }

        public void Run(IWcsApplication application)
        {
            _thread = new System.Threading.Thread(Proc);
            _thread.Name = "堆垛机入口输送线货位任务清除辅助程序";
            _thread.IsBackground = true;
            _thread.StartAndManaged();
        }

        private void Proc()
        {
            var locations = Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteCollection.Routes
                .Where(x => x.Device is CraneDevice && !(x.StartLocation is RackLocationWildcard))
                .Select(x => x.StartLocation is ConveyorLocation ? x.StartLocation : x.StartLocation.Synonymous.First(y => y is ConveyorLocation))
                .Distinct()
                .ToArray();

            while (true)
            {
                Thread.Sleep(_interval);
                foreach (var location in locations)
                {
                    var _device = (ConveyorDevice)location.Device;
                    try
                    {
                        if (!_device.IsConnected || _device.LocationCurrentTasks == null || _device.Tasks == null)
                            continue;

                        var locationCurrentTask = _device.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(location.DeviceCode));
                        if (locationCurrentTask == null)
                            continue;

                        if (locationCurrentTask.TaskNo == 0)
                            continue;

                        ///如果DB1000包含任务则一定不需要清除
                        if (_device.Tasks != null && _device.Tasks.Any(x => x.AssignmentID == locationCurrentTask.TaskNo))
                            continue;

                        var occupyState = _device.OccupyStatus.FirstOrDefault(x => x.PosNo == Convert.ToUInt16(location.DeviceCode));
                        if (occupyState == null)
                            continue;
                        if (occupyState.AftPosPotocell || occupyState.AftProPotocell || occupyState.AftSloPotocell || occupyState.FroPosPotocell || occupyState.FroProPotocell || occupyState.FroSloPotocell)
                        //{
                        //    if (location.DeviceCode == "1909")
                        //    {
                        //        Task _backTask;
                        //        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        //        {
                        //            _backTask = unitOfWork.session.Query<Task>().FirstOrDefault(x => 
                        //                                                                            x.StartLocation.DeviceCode == "1909" 
                        //                                                                            && x.EndLocation.DeviceCode == "1009" 
                        //                                                                            && x.CurrentLocation.DeviceCode == "1909");
                        //            unitOfWork.Commit();
                        //        }
                        //        if (_backTask != null)
                        //            ClearPickLocationTaskNo(_device, locationCurrentTask);
                        //    }
                            continue;
                        //}

                        EquipmentAction _action;
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            _action = unitOfWork.session.Query<EquipmentAction>().FirstOrDefault(x => x.EquipmentTaskId == locationCurrentTask.TaskNo);
                            unitOfWork.Commit();
                        }
                        if (_action == null || _action.Movement.Task.Status == TaskStatus.Cancelled || _action.Movement.Task.Status == TaskStatus.Completed)
                        {
                            ClearPickLocationTaskNo(_device, locationCurrentTask);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(String.Format("堆垛机入口输送线货位任务清除辅助程序，清理取货口任务，设备 {0} 货位 {1} 清除任务命令发送失败,异常信息：", _device.Name, location.DeviceCode, ex.Message), this);
                    }
                }
            }
        }

        /// <summary>
        /// 清理输送线货位任务
        /// </summary>
        /// <param name="pick"></param>
        private void ClearPickLocationTaskNo(ConveyorDevice _device, LocationTaskNetTransferObject locationCurrentTask)
        {
            var _index = _device.LocationCurrentTasks.FindIndex(x => x.PosNo == locationCurrentTask.PosNo);
            var _dataId = (UInt16)_random.Next(1, UInt16.MaxValue);
            ClearLocationTaskCommand _cmd = new ClearLocationTaskCommand(locationCurrentTask.PosNo, locationCurrentTask.TaskNo, locationCurrentTask.TUID, (UInt16)(_index + 1), _dataId);
            try
            {
                _device.Write(_cmd, _cmd.SendSuccess);

                _logger.Error(String.Format("堆垛机入口输送线货位任务清除辅助程序，清理取货口任务，设备 {0} 货位 {1} 清除任务命令（任务号{2}/TUID{3}/Index{4}/DataId{5}）发送成功", _device.Name, locationCurrentTask.PosNo, locationCurrentTask.TaskNo, locationCurrentTask.TUID, (UInt16)(_index + 1), _dataId), this);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("堆垛机入口输送线货位任务清除辅助程序，清理取货口任务，设备 {0} 货位 {1} 清除任务命令（任务号{2}/TUID{3}/Index{4}/DataId{5}）发送失败,异常信息：", _device.Name, locationCurrentTask.PosNo, locationCurrentTask.TaskNo, locationCurrentTask.TUID, (UInt16)(_index + 1), _dataId, ex.Message), this);
            }
        }
    }
}
