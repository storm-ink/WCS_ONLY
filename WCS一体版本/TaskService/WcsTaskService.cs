using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NLog;
using System.Runtime.Remoting.Messaging;
using Wcs;
using Wcs.Framework;

namespace WcsTaskService
{
    public class WcsTaskService : IWcsTaskService
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public Boolean SuspendTaskFromTaskCdoe(string taskNo)
        {
            TaskHelper.Suspend(taskNo);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskNo);
                unitOfWork.Commit();
            }
            return task.Status == TaskStatus.Suspend;
        }

        public Boolean CompleteTaskFromTaskId(int id)
        {
            TaskHelper.Complete(id);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(id);
                unitOfWork.Commit();
            }
            return task == null || task.Status == TaskStatus.Completed;
        }

        public Boolean CompleteMovenentFromLogicMovementId(int id)
        {
            TaskHelper.CompleteMovement(id);
            LogicMovement movement;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                movement = unitOfWork.session.Get<LogicMovement>(id);
                unitOfWork.Commit();
            }
            return movement == null || movement.Status == LogicMovementStatus.Completed;
        }

        public Boolean CompleteActionFromActionId(int id)
        {
            TaskHelper.CompleteAction(id);
            EquipmentAction action;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                action = unitOfWork.session.Get<EquipmentAction>(id);
                unitOfWork.Commit();
            }
            return action == null || action.Status == EquipmentActionStatus.Completed;
        }

        public Boolean CancelTaskFromTaskCode(string taskNo)
        {
            TaskHelper.CancelTask(taskNo);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskNo);
                unitOfWork.Commit();
            }
            return task == null || task.Status == TaskStatus.Cancelled;
        }

        public Boolean CancelTaskFromTaskId(Int32 taskId)
        {
            TaskHelper.CancelTask(taskId);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(taskId);
                unitOfWork.Commit();
            }
            return task == null || task.Status == TaskStatus.Cancelled;
        }

        public Boolean CancelMovementFromLogicMovementId(Int32 logicMovementId)
        {
            TaskHelper.CancleLogicMovement(logicMovementId);
            LogicMovement movement;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                movement = unitOfWork.session.Get<LogicMovement>(logicMovementId);
                unitOfWork.Commit();
            }
            return movement == null || movement.Status == LogicMovementStatus.Cancelled;
        }

        public Boolean CancelActionFromActionId(Int32 actionId)
        {
            TaskHelper.CancleEquipmentAction(actionId);
            EquipmentAction action;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                action = unitOfWork.session.Get<EquipmentAction>(actionId);
                unitOfWork.Commit();
            }
            return action == null || action.Status == EquipmentActionStatus.Cancelled;
        }

        public Boolean ResumeTaskFromTaskCodeWithCurrentLocation(string taskNo, string currentLocation)
        {
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                task = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskNo);

                unitOfWork.Commit();
            }

            //在本次调用上下文中关闭用户信息验证框（仅本次有效）
            CallContext.SetData("双货叉.启用任务继续执行信息验证", false);
            CallContext.SetData("单货叉.启用任务继续执行信息验证", false);

            TaskHelper.Resume(task.Id, LocationConverter.UserCodeToLcation(currentLocation));

            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskNo);
                unitOfWork.Commit();
            }
            return task != null && task.Status != TaskStatus.Suspend && task.Status != TaskStatus.Error;
        }

        public Boolean ArchiveFromTaskCode(String taskCode)
        {
            TaskHelper.Archive(taskCode);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Query<Task>().FirstOrDefault(x => x.TaskCode == taskCode);
                unitOfWork.Commit();
            }
            return task == null;
        }

        public Boolean ArchiveFromTaskId(int taskId)
        {
            TaskHelper.Archive(taskId);
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(taskId);
                unitOfWork.Commit();
            }
            return task == null;
        }
    }
}
