using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using Wcs.Framework.Events;

namespace Wcs.DefaultImpls.Business
{
    /// <summary>
    /// 自动循环（整体）测试任务
    /// _randomDeviceList 是否开启堆垛机内部随机位置选择
    /// </summary>
    public class AutoTestCreateCircleTaskStartUp : IApplicationStartup
    {
        static Logger _logger;
        List<String> _taskTypes;
        List<string> _clearList;
        List<string> _randomDeviceList;
        Random random;

        public void Initialize(StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            random = new Random();
            _taskTypes = element.GetAttributeOrDefault<string>("taskTypes", "test").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            _clearList = element.GetAttributeOrDefault<string>("clearList", "").Split(',').ToList();
            _randomDeviceList = element.GetAttributeOrDefault<string>("randomDeviceList", "").Split(',').ToList();
        }

        public void Run(IWcsApplication application)
        {
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(onTaskFinished);
        }

        private void onTaskFinished(TaskFinishedEvent args)
        {
            if (args.Source != Wcs.Framework.TaskSource.Unknow || !_taskTypes.Contains(args.TaskType) || args.Status != TaskStatus.Completed)
                return;

            Handle(args);
        }

        private void Handle(TaskFinishedEvent args)
        {
            try
            {
                Task _task;

                Boolean alreadyCreateTask = false;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    _task = unitOfWork.session.Get<Task>(args.Id);
                    alreadyCreateTask = unitOfWork.session.Query<Task>().Any(x => x.Description == args.TaskCode);
                    unitOfWork.Commit();
                }
                if (_task == null)
                    return;
                if (alreadyCreateTask)
                {
                    BusinessHelper.PushDeleteTaskList(_task);
                    return;
                }

                //if (_clearList.Contains(_task.EndLocation.DeviceCode))
                //    ClearLocationTask(_task.EndLocation);

                String taskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();
                LocationInfo start = _task.EndLocation;
                LocationInfo end = _task.StartLocation;
                if (_randomDeviceList.Contains(_task.StartLocation.DeviceName))
                {
                    var _ends = DeviceConverter.ToDevice<Crane.CraneDevice>(_task.StartLocation.DeviceName).Locations.Where(x => !(x is Crane.RackLocationWildcard) && x.Synonymous.Length == 0).ToArray();
                    end = LocationConverter.ToLocationInfo(_ends[random.Next(0, _ends.Length)]);
                }
                Task task = new Task(taskCode, start, end)
                {
                    Source = TaskSource.Unknow,
                    TaskType = _task.TaskType,
                    Description = _task.TaskCode
                };
                task.ContainerCodes.Add(_task.TaskCode);

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    unitOfWork.session.Save(task);
                    unitOfWork.Commit();
                }
                Wcs.Framework.EventBus.EventBus.Instance.Publish(new Wcs.Framework.Events.TaskAddedEvent(task));

                BusinessHelper.PushDeleteTaskList(_task);
                //conveyor.清除占位(holdSingle);
                _logger.Info(String.Format("成功创建一条从 {0} 到 {1} 的 {2} 任务", start.DeviceCode, end.DeviceCode, task.TaskType), this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ClearLocationTask(LocationInfo endLocation)
        {
            //try
            //{
            //    var conveyor = DeviceConverter.ToDevice<Conveyor.ConveyorDevice>(endLocation.DeviceName);
            //    if (conveyor.LocationCurrentTasks == null)
            //        return;
            //    var posNo = UInt16.Parse(endLocation.DeviceCode);
            //    var locTask = conveyor.LocationCurrentTasks.FirstOrDefault(x => x.PosNo == posNo && x.TaskNo != 0);
            //    if (locTask == null)
            //        return;
            //    Conveyor.ClearLocationTaskCommand cmd = new Conveyor.ClearLocationTaskCommand(posNo, locTask.TaskNo,locTask.NextPosNo,(UInt16)locTask.AtPacketIndex);
            //    conveyor.Write(cmd, cmd.SendSuccess);
            //}
            //catch (Exception ex)
            //{
            //}
        }
    }
}
