using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.Framework.Impl
{
    /// <summary>
    /// 默认的 Wms 通知程序
    /// </summary>
    public class DefaultWmsNotify : WmsNotify
    {
        public DefaultWmsNotify(Logger logger) : base(logger) { }


        Dictionary<String, List<Int32>> sendingNotify = new Dictionary<string, List<Int32>>();
        private void addToSending(string method, Int32 id)
        {
            lock (this)
            {
                if (!sendingNotify.ContainsKey(method))
                {
                    sendingNotify.Add(method, new List<int>());
                }
                sendingNotify[method].Add(id);
            }
        }

        private void removeFromSending(string method, Int32 id)
        {
            lock (this)
            {
                if (!sendingNotify.ContainsKey(method))
                {
                    return;
                }

                if (sendingNotify[method].Contains(id))
                {
                    sendingNotify[method].Remove(id);
                }
            }
        }

        private Boolean existsInSending(string method, Int32 id)
        {
            lock (this)
            {
                if (!sendingNotify.ContainsKey(method))
                {
                    return false;
                }

                return sendingNotify[method].Contains(id);
            }
        }

        public override void OnExecuting(Task task)
        {

        }

        public override void OnSuspend(Task task)
        {

        }

        public override void OnCancelled(Task task)
        {

        }

        public override void OnDeleted(Task task)
        {

        }

        public override void OnCompleted(Task task)
        {
#warning 如果来自于Wcs将直接删除
            if (task.Source == TaskSource.Wcs)
            {
                try
                {
                    if (Cfg.Configuration.GetSetting<Boolean>("来自Wcs的任务在完成后自动删除", false))
                    {
                        this.Logger.Info(string.Format("系统启用了【来自Wcs的任务在完成后自动删除】功能，{0} 将被自动归档", task), this, task);
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                        {
                            Repository.Query<Task>(unitOfWork, x => x.Id == task.Id).First().Delete(unitOfWork,this.Name);
                            unitOfWork.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (this.Logger != null)
                    {
                        this.Logger.Warning(string.Format("{0} OnCompleted {1} 失败.{2}", this, task, ex.ToString()), this, task);
                    }
                }
            }

            if (task.Source != TaskSource.Wms)
            {
                if (this.Logger != null)
                {
                    this.Logger.Info(string.Format("{0} OnCompleted 忽略 {1}", this, task), this, task);
                }
                return;
            }

            Action<Task> act = (tsk) =>
            {
                if (existsInSending("OnCompleted", tsk.Id))
                {
                    return;
                }
                addToSending("OnCompleted", tsk.Id);
                try
                {
                    WmsProxyProvider.Proxy.CompleteTask(new DC2.WcsTaskInfo
                    {
                        From = tsk.StartLocation.UserCode,
                        To = tsk.EndLocation.UserCode,
                        ContainerCodes = tsk.ContainerCodes.ToArray(),
                        Status = tsk.Status.ToString(),
                        TaskCode = tsk.TaskId,
                        TaskType = tsk.BizType.ToString(),
                        Success = true
                    });
                    if (this.Logger != null)
                    {
                        this.Logger.Info(string.Format("{0} OnCompleted {1} 成功", this, tsk), this, tsk);
                    }

                    DelayedWmsNotifyProcessor.GetInstance().RemoveDelayedWmsNotify(this, "OnCompleted", tsk);
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        Repository.Query<Task>(unitOfWork, x => x.Id == tsk.Id).First().Delete(unitOfWork, this.Name);
                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (this.Logger != null)
                    {
                        //this.Logger.Error(ex.Message, this, ex);
                        //this.Logger.Warning(string.Format("{0} OnCompleted {1} 失败.{2}", this, tsk, ex.Message), this, tsk);
                        this.Logger.Warning(string.Format("{0} OnCompleted {1} 失败.{2}", this, tsk, ex.ToString()), this, tsk);
                    }

                    DelayedWmsNotifyProcessor.GetInstance().Delay(this, "OnCompleted", tsk);
                }
                removeFromSending("OnCompleted", tsk.Id);
            };

            //act.BeginInvoke(task, null, null);
            System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                act((Task)state);
            }, task);
        }


        public override void OnRequest(Request request)
        {
            Action<Request> action = (r) =>
            {
                if (existsInSending("OnRequest", r.Id))
                {
                    return;
                }
                addToSending("OnRequest", r.Id);
                try
                {
                    Dictionary<string, string> additionalInfo = null;
                    if (r.AdditionalInfo != null)
                    {
                        additionalInfo = new Dictionary<string, string>(r.AdditionalInfo);
                    }
                    WmsProxyProvider.Proxy.Request(new DC2.WcsRequestInfo
                    {
                        RequestType = r.RequestType,
                        ContainerCodes = r.ContainerCodes.ToArray(),
                        LocationUserCode = r.Source.UserCode,
                        WcsRequestId = r.Id.ToString(),
                        Prop1 = r.Prop1,
                        Prop2 = r.Prop2,
                        Prop3 = r.Prop3,
                        Prop4 = r.Prop4,
                        Prop5 = r.Prop5,
                        AdditionalInfo = additionalInfo,
                        Comments = r.Comments
                    });

                    if (this.Logger != null)
                    {
                        this.Logger.Info(string.Format("{0} OnRequest {1} 成功", this, r), this, r);
                    }

                    DelayedWmsNotifyProcessor.GetInstance().RemoveDelayedWmsNotify(this, "OnRequest", r);

#warning 2013/07/15 wms 方提出通知只要不抛异常就当已正常处理，删除请求。此举会截断任务和请求的关联性，原因是任务下发时永远都不会有请求存在。
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        var loadedRequest = Repository.Get<Request>(unitOfWork, request.Id);
                        if (loadedRequest != null)
                        {
                            loadedRequest.Status = RequestStatus.Processed;
                        }

                        //添加操作流水
                        OperationAccountHelper.Add(unitOfWork, loadedRequest, "{0} 已成功发出通知，准备删除", loadedRequest);
                        //更新为已处理
                        Repository.Update(unitOfWork, loadedRequest);
                        //归档该请求
                        Repository.Delete(unitOfWork, loadedRequest);
                        //归档操作流水
                        OperationAccountHelper.Archive(unitOfWork, loadedRequest);

                        //this.Logger.Info(string.Format("{0} 为纯信号量，已成功发出通知", loadedRequest), this, loadedRequest);
                        unitOfWork.Commit();
                    }

                    //if (request.ProcessedAfterNotify)
                    //{
                    //    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    //    {
                    //        var loadedRequest = Repository.Get<Request>(unitOfWork, request.Id);
                    //        if (loadedRequest != null)
                    //        {
                    //            loadedRequest.Status = RequestStatus.Processed;
                    //        }

                    //        //添加操作流水
                    //        OperationAccountHelper.Add(unitOfWork, loadedRequest, "{0} 为纯信号量，已成功发出通知，准备删除", loadedRequest);
                    //        //更新为已处理
                    //        TaskManager.GetInstance().Update(unitOfWork, loadedRequest);
                    //        //归档该请求
                    //        TaskManager.GetInstance().DeleteEntity(unitOfWork, loadedRequest);
                    //        //归档操作流水
                    //        OperationAccountHelper.Archive(unitOfWork, loadedRequest);

                    //        this.Logger.Info(string.Format("{0} 为纯信号量，已成功发出通知", loadedRequest), this, loadedRequest);
                    //        unitOfWork.Commit();
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    if (this.Logger != null)
                    {
                        //this.Logger.Error(ex.Message, this, ex);
                        //this.Logger.Warning(string.Format("{0} OnRequest {1} 失败.原因：{2}", this, r, ex.Message), this, r);
                        this.Logger.Warning(string.Format("{0} OnRequest {1} 失败.原因：{2}", this, r, ex.ToString()), this, r);
                    }

                    DelayedWmsNotifyProcessor.GetInstance().Delay(this, "OnRequest", r);
                }
                removeFromSending("OnRequest", r.Id);
            };

            System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                action((Request)state);
            }, request);
        }


        public override string Name
        {
            get { return "Wms 通知程序"; }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
