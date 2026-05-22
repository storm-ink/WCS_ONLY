using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.DefaultImplementCollection.Crane;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using NHibernate.Linq;
using Wcs.FrameworkExtend;



namespace ZHQXC
{
    public class ConveyorTaskFilter : EquipmentActionSchedulerFilter
    {
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {

            if (!(equipmentActionScheduler.Device is ConveyorDevice))
                return new ActionSchedulerFilterResult(false, "非输送线任务");
            //防止1009和1006口任务两头堵塞
            if (action.Movement.Task.EndLocation.DeviceCode == "00-001-1009")
            {
                List<PreTask> alreadyCreateTasks = null;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    alreadyCreateTasks = unitOfWork.session.Query<PreTask>().Where(x => 
                    x.StartLocation.DeviceName == "C002"
                    &&(x.EndLocation.DeviceCode=="00-001-1001"|| x.EndLocation.DeviceCode == "00-001-1002"))
                    .ToList();
                    unitOfWork.Commit();
                }
                if (alreadyCreateTasks!=null)
                {
                    return new ActionSchedulerFilterResult(true, "当前存在C002出库至1001或1002任务，不许发送终点1009的出库任务");
                }
            }
            else if (action.Movement.Task.EndLocation.DeviceCode == "00-001-1006")
            {
                List<PreTask> alreadyCreateTasks = null;
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    alreadyCreateTasks = unitOfWork.session.Query<PreTask>().Where(x =>
                    x.StartLocation.DeviceName == "C001"
                    && (x.EndLocation.DeviceCode == "00-001-1001" || x.EndLocation.DeviceCode == "00-001-1002"))
                    .ToList(); 
                    unitOfWork.Commit();
                }
                if (alreadyCreateTasks != null)
                {
                    return new ActionSchedulerFilterResult(true, "当前存在C001出库至1001或1002任务，不许发送终点1006的出库任务");
                }

            }
            //如果RGV现在正在运行搬货中，对于下一个包含RG的动作，需要进行过滤



            //下料口附近仅能存在一条行走任务
            //string[] SpecialConveyor = {"1012","1016", "1020", "1021", "1023", "1024", "1025", "1026", "1027","1028" };
            //if (SpecialConveyor.Contains(action.Movement.Task.StartLocation.DeviceCode))
            //{
            //    List<PreTask> alreadyCreateTasks = null;
            //    using (NHUnitOfWork unitOfWork =new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            //    {
            //        alreadyCreateTasks = unitOfWork.session.Query<PreTask>().Where(x =>
            //       x.StartLocation.DeviceCode.ac
            //       .ToList();
            //        unitOfWork.Commit();
            //    }
            //}

            return new ActionSchedulerFilterResult(false, "");
        }
    }
}
