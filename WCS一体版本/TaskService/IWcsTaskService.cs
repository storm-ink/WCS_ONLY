using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Wcs.Framework;

namespace WcsTaskService
{
    [ServiceContract]
    public interface IWcsTaskService
    {
        /// <summary>
        /// 归档
        /// </summary>
        /// <param name="taskId"></param>
        [OperationContract]
        Boolean ArchiveFromTaskCode(String taskCode);
        /// <summary>
        /// 归档
        /// </summary>
        /// <param name="taskId"></param>
        [OperationContract]
        Boolean ArchiveFromTaskId(Int32 taskId);
        /// <summary>
        /// 暂停指定的任务
        /// </summary>
        /// <param name="taskNo">任务号</param>
        /// <returns></returns>IWcsTaskService
        [OperationContract]
        Boolean SuspendTaskFromTaskCdoe(String taskCode);
        /// <summary>
        /// 完成指定的任务对象
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [OperationContract]
        Boolean CompleteTaskFromTaskId(Int32 taskId);
        /// <summary>
        /// 完成指定的逻辑动作对象
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [OperationContract]
        Boolean CompleteMovenentFromLogicMovementId(Int32 logicMovementId);
        /// <summary>
        /// 完成指定的物理动作对象
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        [OperationContract]
        Boolean CompleteActionFromActionId(Int32 actionId);
        /// <summary>
        /// 根据任务号取消指定的任务
        /// </summary>
        /// <param name="taskNo">任务号</param>
        /// <returns></returns>
        [OperationContract]
        Boolean CancelTaskFromTaskCode(String taskNo);
        /// <summary>
        /// 根据任务Id取消指定的任务
        /// </summary>
        /// <param name="taskId"></param>
        [OperationContract]
        Boolean CancelTaskFromTaskId(Int32 taskId);
        /// <summary>
        /// 根据逻辑动作ID取消指定的逻辑动作
        /// </summary>
        /// <param name="logicMovementId"></param>
        [OperationContract]
        Boolean CancelMovementFromLogicMovementId(Int32 logicMovementId);
        /// <summary>
        /// 根据物理动作Id取消制定的物理动作
        /// </summary>
        /// <param name="actionId"></param>
        [OperationContract]
        Boolean CancelActionFromActionId(Int32 actionId);
        ///// <summary>
        ///// 继续执行指定的任务
        ///// </summary>
        ///// <param name="taskNo">任务号</param>
        /////</returns>
        //[OperationContract]
        //void ResumeTask(String taskNo);
        /// <summary>
        /// 继续执行指定的任务
        /// </summary>
        /// <param name="taskNo">任务号</param>
        /// <param name="currentLocation">当前停靠位置UserCode/单任务设备名</param>
        /// <returns></returns>
        [OperationContract]
        Boolean ResumeTaskFromTaskCodeWithCurrentLocation(String taskCode, String currentLocation);
    }
}
