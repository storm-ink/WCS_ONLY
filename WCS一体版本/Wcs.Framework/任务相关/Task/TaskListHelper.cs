using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Events;

namespace Wcs.Framework
{
    /// <summary>
    /// 可执行任务列表
    /// </summary>
    public static class TaskListHelper
    {
        /// <summary>
        /// 是否启用任务控制
        /// </summary>
        const string enableTaskControl = "enableTaskControl";
        /// <summary>
        /// 任务列表
        /// </summary>
        public static Dictionary<string, Task> Tasks = new Dictionary<string, Task>();
        /// <summary>
        /// 执行中的任务列表
        /// </summary>
        public static List<string> RunningTasks = new List<string>();
        /// <summary>
        /// 待执行的任务列表
        /// </summary>
        public static List<string> WaittingRunningTasks = new List<string>();
        /// <summary>
        /// 可以执行的任务列表
        /// </summary>
        public static List<string> EnableTasks = new List<string>();
        /// <summary>
        /// 不可以执行的任务列表
        /// key - taskCode
        /// value - unable reson
        /// </summary>
        public static Dictionary<string, string> UnableTasks = new Dictionary<string, string>();
        /// <summary>
        /// 待取消任务列表
        /// </summary>
        public static List<string> WaitingCancleTasks = new List<string>();
        /// <summary>
        /// 待强制完成任务列表
        /// </summary>
        public static List<string> WaitingForceCompeletedTasks = new List<string>();

        /// <summary>
        /// 是否可以执行
        /// </summary>
        /// <param name="taskCode"></param>
        /// <param name="reson"></param>
        /// <returns></returns>
        public static bool CanRunning(string taskCode, out string reson)
        {
            if (UnableTasks.ContainsKey(taskCode))
            {
                reson = UnableTasks[taskCode];
                return false;
            }
            if (EnableTasks.Contains(taskCode) || WaittingRunningTasks.Contains(taskCode) || RunningTasks.Contains(taskCode))
            {
                reson = "";
                return true;
            }
            reson = "任务调度线程暂未调度该任务";
            return false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskAddedEvent>(TaskAddedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskCurrentLocationChangedEvent>(TaskCurrentLocationChangedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(TaskFinishedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskPriorityChangedEvent>(TaskPriorityChangedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskStatusChangedEvent>(TaskStatusChangedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskUpdateEvent>(TaskUpdateEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskFinishedEvent>(TaskFinishedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<Wcs.Framework.Events.TaskArchivedEvent>(TaskArchivedEventHand);
            Wcs.Framework.EventBus.EventBus.Instance.Subscribe<LogicMovementAddedEvent>(LogicMovementAddedEventHand);
        }

        private static void LogicMovementAddedEventHand(LogicMovementAddedEvent obj)
        {
            throw new NotImplementedException();
        }

        private static void TaskArchivedEventHand(TaskArchivedEvent obj)
        {
            if (Tasks.ContainsKey(obj.TaskCode))
                Tasks.Remove(obj.TaskCode);
            if (RunningTasks.Contains(obj.TaskCode))
                RunningTasks.Remove(obj.TaskCode);
            if (WaittingRunningTasks.Contains(obj.TaskCode))
                WaittingRunningTasks.Remove(obj.TaskCode);
            if (EnableTasks.Contains(obj.TaskCode))
                EnableTasks.Remove(obj.TaskCode);
            if (UnableTasks.ContainsKey(obj.TaskCode))
                UnableTasks.Remove(obj.TaskCode);
            if (WaitingCancleTasks.Contains(obj.TaskCode))
                WaitingCancleTasks.Remove(obj.TaskCode);
            if (WaitingForceCompeletedTasks.Contains(obj.TaskCode))
                WaitingForceCompeletedTasks.Remove(obj.TaskCode);
        }

        private static void TaskUpdateEventHand(TaskUpdateEvent obj)
        {
            if (Tasks.ContainsKey(obj.oldTask.TaskCode))
                Tasks[obj.oldTask.TaskCode] = obj.newTask;
        }

        private static void TaskStatusChangedEventHand(TaskStatusChangedEvent obj)
        {
            if (Tasks.ContainsKey(obj.TaskCode))
                Tasks[obj.TaskCode].Status = obj.Status;
        }

        private static void TaskPriorityChangedEventHand(TaskPriorityChangedEvent obj)
        {
            if (Tasks.ContainsKey(obj.TaskCode))
                Tasks[obj.TaskCode].Priority = obj.Priority;
        }

        private static void TaskFinishedEventHand(TaskFinishedEvent obj)
        {
            if (Tasks.ContainsKey(obj.TaskCode))
                Tasks.Remove(obj.TaskCode);
            if (RunningTasks.Contains(obj.TaskCode))
                RunningTasks.Remove(obj.TaskCode);
            if (WaittingRunningTasks.Contains(obj.TaskCode))
                WaittingRunningTasks.Remove(obj.TaskCode);
            if (EnableTasks.Contains(obj.TaskCode))
                EnableTasks.Remove(obj.TaskCode);
            if (UnableTasks.ContainsKey(obj.TaskCode))
                UnableTasks.Remove(obj.TaskCode);
            if (WaitingCancleTasks.Contains(obj.TaskCode))
                WaitingCancleTasks.Remove(obj.TaskCode);
            if (WaitingForceCompeletedTasks.Contains(obj.TaskCode))
                WaitingForceCompeletedTasks.Remove(obj.TaskCode);
        }

        private static void TaskCurrentLocationChangedEventHand(TaskCurrentLocationChangedEvent obj)
        {
            if (Tasks.ContainsKey(obj.TaskCode))
                Tasks[obj.TaskCode].CurrentLocation = obj.CurrentLocation;
        }

        private static void TaskAddedEventHand(TaskAddedEvent obj)
        {
            if (!Tasks.ContainsKey(obj.Task.TaskCode))
                Tasks.Add(obj.Task.TaskCode, obj.Task);
        }

        //static object
        //public static bool AddNewTask(Task task)
        //{ 

        //}
    }
}
