using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sineva.WMS.Dto.WCSDto.ReplyDto;
using Sineva.WMS.Dto.WCSDto.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Wcs.DefaultImplementCollection.Business;
using Wcs.Framework;
using Wcs;
using NHibernate.Linq;
using NHibernate.Cfg.MappingSchema;
using System.Collections;
using NLog;
using System.Net.Http;
using System.Net;
using System.Web;
using Wcs.DefaultImplementCollection.Crane;
using ZHQXC.Client;
using System.Windows.Forms;
using ZHQXC.PreTaskHand;
using Wcs.FrameworkExtend;

namespace ZHQXC
{
    [RoutePrefix("API/WCSWebApi")]

    public class WCSController : ApiController
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Test 获取服务器时间
        /// </summary>
        /// <returns></returns>
        [Route("test")]
        [HttpGet]
        public IHttpActionResult GetServerTime()
        {
            return Json(new { Message = $"This is Port Test Message from QDBOE.TestController, Now is {DateTime.Now.ToString("yyyyy-MM-dd HH:mm:ss.ffff")}" });
        }

        #region Client Hand
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("Devices")]
        [HttpPost]
        public IHttpActionResult GetDevices([FromBody] JObject msg)
        {
            Dictionary<string, string> devices = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection
                           .SelectMany(x => x.DeviceElements)
                           .ToDictionary(x => x.Name, x => x.Device.GetDeviceType());
            return Json(devices);
        }

        [Route("Actions")]
        [HttpPost]
        public IHttpActionResult GetActions([FromBody] JObject msg)
        {
            var device = msg["deviceName"].ToString();
            List<WCSEquipmentAction> wcsActions = new List<WCSEquipmentAction>();
            List<EquipmentAction> actions;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                actions = unitOfWork.session.Query<EquipmentAction>().Where(x => x.DeviceName == device).ToList();
                unitOfWork.Commit();
            }
            actions.ForEach(x => wcsActions.Add(new WCSEquipmentAction(x)));

            return Json(wcsActions);
        }

        [Route("MethodDescriptorTree")]
        [HttpPost]
        public IHttpActionResult GetMethodDescriptorTree([FromBody] JObject msg)
        {
            var deviceName = msg["deviceName"].ToString();
            var device = DeviceConverter.ToDevice<TaskableDevice>(deviceName);
            var node = device.EquipmentActionScheduler._methodDescriptorTree.AllMethodDescriptors.FirstOrDefault(x => x.AccessResult == false);
            string result = "";
            if (node != null)
            {
                result = string.Format(@"节点名称：{0}
说   明：{1}
访问时间：{2}
访问结果：{3}
访问说明：{4}", node.Name, node.Description, node.AccessAt, node.AccessResult, node.AccessDescription);

            }
            return Json(result);

            //return Json(device.EquipmentActionScheduler._methodDescriptorTree);
        }

        [Route("PreTaskSchedulerFilter")]
        [HttpPost]
        public IHttpActionResult GetPreTaskSchedulerFilter([FromBody] JObject msg)
        {
            var taskcode = msg["taskCode"].ToString();
            ActionSchedulerFilterResult result = new ActionSchedulerFilterResult(false, "");
            if (PreTaskHandHelper.lastPreTaskSchedulerFilterResult.ContainsKey(taskcode))
                result = PreTaskHandHelper.lastPreTaskSchedulerFilterResult[taskcode];

            return Json(result);
        }

        [Route("AbstractStateManager")]
        [HttpPost]
        public IHttpActionResult GetAbstractStateManager([FromBody] JObject msg)
        {
            var equipmentTaskId = int.Parse(msg["equipmentTaskId"].ToString());
            var manager = Wcs.Framework.AbstractStateManager.Contexts.FirstOrDefault(x => x.EquipmentAction.EquipmentTaskId == equipmentTaskId);
            if (manager == null || manager._IsDisposing)
                return Json("");
            else
            {
                return Json(new
                {
                    step = manager.CurrentState.Name,
                    isCompeleted = manager.ContextIsCompelted.CreateAt.ToString("yyyy-mm-dd HH:MM:ss.ffff") + " " + manager.ContextIsCompelted.Result.ToString() + " " + manager.ContextIsCompelted.Information,
                    canSend = manager.ContextCanPerform.CreateAt.ToString("yyyy-mm-dd HH:MM:ss.ffff") + " " + manager.ContextCanPerform.Result.ToString() + " " + manager.ContextCanPerform.Information
                });
            }
        }

        /// <summary>
        /// 锁定、解锁(最高权限) 设备
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("LockDevice")]
        [HttpPost]
        public IHttpActionResult LockDevice([FromBody] JObject msg)
        {
            var hand = JsonConvert.DeserializeObject<DeviceLockerHand>(msg.ToString());
            if (hand == null)
                throw new ArgumentNullException("参数不可为空");

            var device = Wcs.Framework.Cfg.WcsConfiguration.Instance.DeviceCollection.ParticularDeviceCollection
                          .SelectMany(x => x.DeviceElements)
                          .Where(x => x.Device is TaskableDevice)
                          .Select(x => x.Device as TaskableDevice)
                          .FirstOrDefault(x => x.Name == hand.Device);
            if (device == null)
            {
                hand.Result = false;
                hand.Message = "未找到对应可执行任务的设备，请确认后重试";
                return Json(hand);
            }
            if (hand.Lock)
            {
                if (device.Locker.IsEmpty)
                {
                    try
                    {
                        device.Lock(new LockerInfo(System.Environment.MachineName, LockerInfo.GetIpAddress()));
                        hand.Result = true;
                        hand.Message = $"锁定成功";
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        hand.Result = false;
                        hand.Message = $"锁定时发生异常，异常消息{ex}";
                    }
                }
                else
                {
                    hand.Result = false;
                    hand.Message = "已锁定";
                }
            }
            else
            {
                if (device.Locker.IsEmpty)
                {
                    hand.Result = false;
                    hand.Message = "已解锁";
                }
                else
                {
                    try
                    {
                        device.Unlock(LockerInfo.Adminstrator);
                        hand.Result = true;
                        hand.Message = $"解锁成功";
                    }
                    catch (Exception ex)
                    {
                        _logger.Error1(ex, this);
                        hand.Result = false;
                        hand.Message = $"解锁时发生异常，异常消息{ex}";
                    }
                }
            }

            return Json(hand);
        }

        /// <summary>
        /// 获取当前任务列表
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("PreTasks")]
        [HttpPost]
        public IHttpActionResult GetPreTasks([FromBody] JObject msg)
        {
            List<WCSPreTask> wcsPreTasks = new List<WCSPreTask>();
            List<PreTask> tasks;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                tasks = unitOfWork.session.Query<PreTask>().ToList();
                unitOfWork.Commit();
            }
            wcsPreTasks = tasks.Select(x => new WCSPreTask(x)).ToList();

            return Json(wcsPreTasks);
        }

        /// <summary>
        /// 获取指定taskId任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("PreTask")]
        [HttpPost]
        public IHttpActionResult GetPreTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());
            PreTask task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<PreTask>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSPreTask wcsPreTask = null;
            if (task != null)
                wcsPreTask = new WCSPreTask(task);

            return Json(wcsPreTask);
        }

        /// <summary>
        /// 获取当前任务列表
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("HistoryPreTasks")]
        [HttpPost]
        public IHttpActionResult GetHistoryPreTasks([FromBody] JObject msg)
        {
            try
            {
                var htqc = JsonConvert.DeserializeObject<HistoryTaskQueryCriteria>(msg.ToString());
                var tasks = loadHistoryPreTasks(htqc);
                //List<WCSTask> wcsTasks = tasks.Select(x => new WCSTask(x)).ToList();
                return Json(tasks);
            }
            catch (Exception ex)
            {
                return Json("500");
            }
        }

        /// <summary>
        /// 获取当前任务列表
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("Tasks")]
        [HttpPost]
        public IHttpActionResult GetTasks([FromBody] JObject msg)
        {
            List<WCSTask> wcsTasks = new List<WCSTask>();
            List<Task> tasks;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                tasks = unitOfWork.session.Query<Task>().ToList();
                unitOfWork.Commit();
            }
            wcsTasks = tasks.Select(x => new WCSTask(x)).ToList();

            return Json(wcsTasks);
        }

        /// <summary>
        /// 获取指定taskId任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("Task")]
        [HttpPost]
        public IHttpActionResult GetTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());
            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 手工任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("AddPreTask")]
        [HttpPost]
        public IHttpActionResult AddPreTask([FromBody] JObject msg)
        {
            var tsk = JsonConvert.DeserializeObject<WCSTask>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");

            PreTask preTask = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(tsk.TaskCode))
                    tsk.TaskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();

                string taskCode = tsk.TaskCode;
                var start = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(tsk.StartLocation.UserCode));
                var end = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(tsk.EndLocation.UserCode));
                preTask = new PreTask(taskCode, start, end);
                if (tsk.ContainerCodes == null || tsk.ContainerCodes.Count() == 0)
                    preTask.ContainerCodes = "";
                else
                    preTask.ContainerCodes = JsonConvert.SerializeObject(tsk.ContainerCodes);
                if (tsk.AdditionalInfo == null || tsk.AdditionalInfo.Count() == 0)
                    preTask.AdditionalInfo = "";
                else
                    preTask.AdditionalInfo = JsonConvert.SerializeObject(tsk.AdditionalInfo);

                preTask.Source = (TaskSource)Enum.Parse(typeof(TaskSource), tsk.Source);
                preTask.TaskType = tsk.TaskType;
                preTask.Description = tsk.Description;

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    unitOfWork.session.Save(preTask);
                    unitOfWork.Commit();
                }

                Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.FrameworkExtend.Events.PreTaskAddedEvent>(new Wcs.FrameworkExtend.Events.PreTaskAddedEvent(preTask));
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            return Json(preTask);
        }
        /// <summary>
        /// 手工任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("AddTask")]
        [HttpPost]
        public IHttpActionResult AddTask([FromBody] JObject msg)
        {
            var tsk = JsonConvert.DeserializeObject<WCSTask>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");

            Task task = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(tsk.TaskCode))
                    tsk.TaskCode = Wcs.Framework.SerialNumberFactory.GenerateManualTaskCode();

                string taskCode = tsk.TaskCode;
                var start = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(tsk.StartLocation.UserCode));
                var end = LocationConverter.ToLocationInfo(LocationConverter.UserCodeToLcation(tsk.EndLocation.UserCode));
                task = new Task(taskCode, start, end);
                task.ContainerCodes.AddAll(tsk.ContainerCodes);
                task.AdditionalInfo = tsk.AdditionalInfo;
                task.Source = (TaskSource)Enum.Parse(typeof(TaskSource), tsk.Source);
                task.TaskType = tsk.TaskType;
                task.Description = tsk.Description;

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    unitOfWork.session.Save(task);
                    unitOfWork.Commit();
                }

                Wcs.Framework.EventBus.EventBus.Instance.Publish<Wcs.Framework.Events.TaskAddedEvent>(new Wcs.Framework.Events.TaskAddedEvent(task));
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("SuspendTask")]
        [HttpPost]
        public IHttpActionResult SuspendTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.Suspend(tiqc.TaskId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CancellTask")]
        [HttpPost]
        public IHttpActionResult CancelTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.CancelTask(tiqc.TaskId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 强制完成任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CompleteTask")]
        [HttpPost]
        public IHttpActionResult CompleteTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.Complete(tiqc.TaskId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                var exception = new HttpResponseException(message);
                throw exception;
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 继续执行任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("ResumeTask")]
        [HttpPost]
        public IHttpActionResult ResumeTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                Location currentLocation = null;
                if (!string.IsNullOrWhiteSpace(tiqc.CurrentUserCode))
                    currentLocation = LocationConverter.UserCodeToLcation($"{tiqc.CurrentUserCode}");

                TaskHelper.Resume(tiqc.TaskId, currentLocation);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 获取继续执行路线（目前主要是输送线任务）
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("GetTaskResumeAtRoute")]
        [HttpPost]
        public IHttpActionResult GetTaskResumeAtRoute([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            var stopRoute = TaskHelper.GetTaskResumeAtRoute(tiqc.TaskId);
            ClientRouteHead clientRouteHead = null;
            if (stopRoute != null)
            {
                if (stopRoute.AllowStartFromMidway)
                {
                    var locations = stopRoute.Details.Select(x => LocationConverter.ConvertibleCodeToLcation(x.Path + "@" + x.Device)).ToArray();
                    clientRouteHead = new ClientRouteHead()
                    {
                        RouteId = stopRoute.Id,
                        DeviceName = stopRoute.Device,
                        RouteNo = stopRoute.No,
                        AllowAllowStartFromMidway = stopRoute.AllowStartFromMidway,
                        Items = locations.Select(x => new WcsLocation()
                        {
                            UserCode = x.UserCode,
                            DeviceCode = x.DeviceCode,
                            UnifiedCode = x.UnifiedCode,
                            DeviceName = x.Device.Name
                        }).ToList()
                    };
                }
                else
                    clientRouteHead = new ClientRouteHead()
                    {
                        RouteId = stopRoute.Id,
                        DeviceName = stopRoute.Device,
                        RouteNo = stopRoute.No,
                        AllowAllowStartFromMidway = stopRoute.AllowStartFromMidway
                    };
            }

            return Json(clientRouteHead);
        }

        /// <summary>
        /// 归档任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("ArchiveTask")]
        [HttpPost]
        public IHttpActionResult ArchiveTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.Archive(tiqc.TaskId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 强制完成逻辑动作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CompleteMovement")]
        [HttpPost]
        public IHttpActionResult CompleteMovement([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.CompleteMovement(tiqc.MovementId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 强制完成物理动作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CompleteAction")]
        [HttpPost]
        public IHttpActionResult CompleteAction([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.CompleteAction(tiqc.ActionId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 取消逻辑动作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CancleLogicMovement")]
        [HttpPost]
        public IHttpActionResult CancleLogicMovement([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.CancleLogicMovement(tiqc.MovementId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 取消物理动作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("CancleEquipmentAction")]
        [HttpPost]
        public IHttpActionResult CancleEquipmentAction([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());

            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                TaskHelper.CancleEquipmentAction(tiqc.ActionId);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }

            Task task;
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        /// <summary>
        /// 获取历史任务列表
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("HistoryTasks")]
        [HttpPost]
        public IHttpActionResult GetHistoryTasks([FromBody] JObject msg)
        {
            try
            {
                var htqc = JsonConvert.DeserializeObject<HistoryTaskQueryCriteria>(msg.ToString());
                var tasks = loadHistoryTasks(htqc);
                //List<WCSTask> wcsTasks = tasks.Select(x => new WCSTask(x)).ToList();
                return Json(tasks);
            }
            catch (Exception ex)
            {
                return Json("500");
            }
        }

        /// <summary>
        /// 获取指定taskId历史任务
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("HistoryTask")]
        [HttpPost]
        public IHttpActionResult GetHistoryTask([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<TaskInformationQueryCriteria>(msg.ToString());
            Task task;
            using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                task = unitOfWork.session.Get<Task>(tiqc.TaskId);
                unitOfWork.Commit();
            }
            WCSTask wcsTask = null;
            if (task != null)
                wcsTask = new WCSTask(task);

            return Json(wcsTask);
        }

        const string NullTaskType = "【空字符串】";
        IList<Hashtable> loadHistoryTasks(HistoryTaskQueryCriteria htqc)
        {
            IList<Hashtable> list;
            using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                Dictionary<string, object> pars = new Dictionary<string, object>();

                var sql = string.Format(@"SELECT top {0} t.*,StartLocation_UserCode as StartLocation,EndLocation_UserCode as EndLocation,CurrentLocation_UserCode as CurrentLocation
                                            FROM [Tasks] t where 1 =1", htqc.Total);

                sql += " and t.CreatedAt>=:startDate";
                pars.Add("startDate", htqc.Start);
                sql += " and t.CreatedAt<:endDate";
                pars.Add("endDate", htqc.End);

                if (!string.IsNullOrWhiteSpace(htqc.TaskCode))
                {
                    sql += " and t.TaskCode like :taskCode";
                    pars.Add("taskCode", '%' + htqc.TaskCode.Trim() + '%');
                }

                if (!string.IsNullOrWhiteSpace(htqc.TaskType))
                {
                    var v = htqc.TaskType.Trim();
                    if (v == NullTaskType)
                    {
                        sql += " and (t.TaskType = '' or t.TaskType is null)";
                    }
                    else
                    {
                        sql += " and t.TaskType like :taskType";
                        pars.Add("taskType", '%' + v + '%');
                    }
                }

                if (!string.IsNullOrWhiteSpace(htqc.From))
                {
                    sql += " and (t.StartLocation_UserCode like :startLocation or t.StartLocation_DeviceCode like :startLocation)";
                    pars.Add("startLocation", '%' + htqc.From.Trim() + '%');
                }

                if (!string.IsNullOrWhiteSpace(htqc.To))
                {
                    sql += " and (t.EndLocation_UserCode like :endLocation or t.EndLocation_DeviceCode like :endLocation)";
                    pars.Add("endLocation", '%' + htqc.To.Trim() + '%');
                }

                if (htqc.FromWMS != null)
                {
                    if (htqc.FromWMS == true)
                    {
                        sql += " and t.Source=:source";
                        pars.Add("source", (int)TaskSource.Wms);
                    }
                    else
                    {
                        sql += " and t.Source<>:source";
                        pars.Add("source", (int)TaskSource.Wms);
                    }
                }

                if (htqc.ActionId > 0)
                {
                    sql += @" and exists(
			            select m.Id from LogicMovements m
			            join EquipmentActions a on m.Id=a.LogicMovementId
			            where a.EquipmentTaskId=:equipmentTaskId
	               )";
                    pars.Add("equipmentTaskId", htqc.ActionId);
                }

                if (!String.IsNullOrWhiteSpace(htqc.ContainerCodes))
                {
                    sql += " and exists(select 1 from TaskContainerCodes where TaskId=t.Id and Value =:containerCode)";
                    pars.Add("containerCode", htqc.ContainerCodes.Trim());
                }

                sql = string.Format(@"select *,STUFF(
  (select ','+tc.value from [TaskContainerCodes] tc where tc.TaskId=tmp.id  FOR XML PATH('')),1,1,''
    ) as [ContainerCode]
    ,
 STUFF(
  (select ','+tai.[Key] + '='+tai.value from [TaskAdditionalInfo] tai where tmp.Id=tai.TaskId  FOR XML PATH('')),1,1,''
    ) as [AdditionalInfo]
 from (
	  {0} order by t.Id desc
  ) tmp order by Id desc", sql);

                var q2 = unitOfWork.session.CreateSQLQuery(sql);

                foreach (var p in pars)
                {
                    if (p.Value == null)
                    {
                        continue;
                    }
                    q2.SetParameter(p.Key, p.Value);
                }

                q2.SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap);

                list = q2.List<Hashtable>();

                unitOfWork.Commit();
            }
            return list;
        }
        IList<Hashtable> loadHistoryPreTasks(HistoryTaskQueryCriteria htqc)
        {
            IList<Hashtable> list;
            using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                Dictionary<string, object> pars = new Dictionary<string, object>();

                var sql = string.Format(@"SELECT top {0} t.*,StartLocation_UserCode as StartLocation,EndLocation_UserCode as EndLocation
                                            FROM [PreTasks] t where 1 =1", htqc.Total);

                sql += " and t.CreatedAt>=:startDate";
                pars.Add("startDate", htqc.Start);
                sql += " and t.CreatedAt<:endDate";
                pars.Add("endDate", htqc.End);

                if (!string.IsNullOrWhiteSpace(htqc.TaskCode))
                {
                    sql += " and t.TaskCode like :taskCode";
                    pars.Add("taskCode", '%' + htqc.TaskCode.Trim() + '%');
                }

                if (!string.IsNullOrWhiteSpace(htqc.TaskType))
                {
                    var v = htqc.TaskType.Trim();
                    if (v == NullTaskType)
                    {
                        sql += " and (t.TaskType = '' or t.TaskType is null)";
                    }
                    else
                    {
                        sql += " and t.TaskType like :taskType";
                        pars.Add("taskType", '%' + v + '%');
                    }
                }

                if (!string.IsNullOrWhiteSpace(htqc.From))
                {
                    sql += " and (t.StartLocation_UserCode like :startLocation or t.StartLocation_DeviceCode like :startLocation)";
                    pars.Add("startLocation", '%' + htqc.From.Trim() + '%');
                }

                if (!string.IsNullOrWhiteSpace(htqc.To))
                {
                    sql += " and (t.EndLocation_UserCode like :endLocation or t.EndLocation_DeviceCode like :endLocation)";
                    pars.Add("endLocation", '%' + htqc.To.Trim() + '%');
                }

                if (htqc.FromWMS != null)
                {
                    if (htqc.FromWMS == true)
                    {
                        sql += " and t.Source=:source";
                        pars.Add("source", (int)TaskSource.Wms);
                    }
                    else
                    {
                        sql += " and t.Source<>:source";
                        pars.Add("source", (int)TaskSource.Wms);
                    }
                }

                if (!String.IsNullOrWhiteSpace(htqc.ContainerCodes))
                {
                    sql += " and t.ContainerCode like :containerCode";
                    pars.Add("containerCode", '%' + htqc.ContainerCodes.Trim() + '%');
                }

                sql = string.Format(@"select * from ( {0} order by t.Id desc ) tmp order by Id desc", sql);

                var q2 = unitOfWork.session.CreateSQLQuery(sql);

                foreach (var p in pars)
                {
                    if (p.Value == null)
                    {
                        continue;
                    }
                    q2.SetParameter(p.Key, p.Value);
                }

                q2.SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap);

                list = q2.List<Hashtable>();

                unitOfWork.Commit();
            }
            return list;
        }

        /// <summary>
        /// 获取历史任务类型
        /// </summary>
        /// <returns></returns>
        [Route("HistoryTaskTypes")]
        [HttpGet]
        public IHttpActionResult GetHistoryTaskTypes()
        {
            try
            {
                List<string> taskTypes = new List<string>();
                using (NHBackupServerUnitOfWork unitOfWork = new NHBackupServerUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    taskTypes = unitOfWork.session.Query<Task>()
                        .GroupBy(x => x.TaskType)
                        .Select(x => x.Key)
                        .ToList();

                    unitOfWork.Commit();
                }
                return Json(taskTypes);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
        }

        /// <summary>
        /// 添加手工任务时 获取参考数据
        /// </summary>
        /// <returns></returns>
        [Route("AddTaskProvideInfos")]
        [HttpPost]
        public IHttpActionResult GetAddTaskProvideInfos()
        {
            AddTaskProvideInfos infos = new AddTaskProvideInfos();
            try
            {
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey("ManualTaskTasktypes"))
                    infos.TaskTypes = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["ManualTaskTasktypes"].Split(',').Distinct().ToList();
                if (Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey("ManualAddtionalInfos"))
                    infos.TaskAdditionalInfos = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["ManualAddtionalInfos"].Split('|').Distinct().ToList();

                infos.Sources = Enum.GetValues(typeof(TaskSource)).Cast<TaskSource>().ToList();
                infos.BizTypes = Wcs.EnumExtentions.ToKeyValueList<TaskBizType>().Select(x => x.Value).ToList();
                var locations = Wcs.Framework.Cfg.WcsConfiguration.Instance.LocationCollection.Locations.Where(x => !(x is Wcs.Framework.ILocationWildcard));
                infos.WcsLocations = locations.Select(x => new WcsLocation(x)).ToList();

                return Json(infos);
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };

                throw new HttpResponseException(message);
            }
        }

        /// <summary>
        /// PalletLablePrint
        /// </summary>
        /// <returns></returns>
        [Route("TaskRequest")]
        [HttpPost]
        public ReplyBase EquipmentTaskChangeRequest([FromBody] JObject msg)
        {
            ReplyBase replyBase = new ReplyBase();
            //EquipmentTaskChangeRequest equipmentTaskChangeRequest;
            //try
            //{
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error1(ex, this);
            //    replyBase.IctResult = false;
            //    replyBase.IctCode = "500";
            //    replyBase.IctMsg = "操作执行异常（执行请求失败）";
            //    replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //    replyBase.Data = ex.Message;
            //    RequestWMSHelper.Log($"WMS-->WCS 收到WMS调用 EquipmentTaskChangeRequest 接口，解析数据时发生异常，返回消息\r\n{JsonConvert.SerializeObject(replyBase)}\r\n异常消息：{ex}");
            //    return replyBase;
            //}
            //replyBase.IctDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //replyBase.IctMsg = "成功";
            //replyBase.IctResult = true;
            return replyBase;
        }

        /// <summary>
        /// 优先级接口
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Route("ChangeTaskPriority")]// ChangeTaskPriority

        [HttpPost]
        public IHttpActionResult ChangeTaskPriority([FromBody] JObject msg)
        {
            var tiqc = JsonConvert.DeserializeObject<ChangeTaskPriorityInfo>(msg.ToString()); //传入的是集合
            string _msg = "";
            if (!RemoteHandTaskHelper.Hold(_msg))
                throw new Exception($"未获取更新权限，请稍后再试！");
            try
            {
                try
                {
                    Task tsk = null;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        tsk = unitOfWork.session.Query<Task>().Where(x => x.Id == tiqc.TaskId || x.TaskCode == tiqc.TaskCode).FirstOrDefault();
                        unitOfWork.Commit();
                    }
                    if (tsk == null)
                        throw new Exception($"未找到指定任务号 {tiqc.TaskCode} 的任务");

                    if (tsk.Status == TaskStatus.Cancelled || tsk.Status == TaskStatus.Completed || tsk.Status == TaskStatus.Executing)
                        throw new Exception($"指定任务号 {tiqc.TaskCode} 的任务已完成或者取消或者执行中");

                    TaskHelper.ChangePriority(tiqc.Priority, new List<int> { tsk.Id }.ToArray());
                    Task task;
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        //task = unitOfWork.session.Get<Task>(v.TaskId);
                        task = unitOfWork.session.Get<Task>(tsk.Id);
                        unitOfWork.Commit();
                    }
                    WCSTask wcsTask = null;
                    if (task != null)
                        wcsTask = new WCSTask(task);
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = "InternalServerError"
                };
                throw new HttpResponseException(message);
            }
            finally
            {
                RemoteHandTaskHelper.Holder = string.Empty;
            }
            return Json(msg);
        }
        #endregion
   
        /// <summary>
        /// 堆垛机远程操作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        #region Device Hand
        [Route("SingleForkCraneHand")]
        [HttpPost]
        public IHttpActionResult SingleForkCraneHand([FromBody] JObject msg)
        {
            SingleForkCraneRemoteHand singleForkCraneRemoteHand = JsonConvert.DeserializeObject<SingleForkCraneRemoteHand>(msg.ToString());
            if (singleForkCraneRemoteHand.HandType == HandTypes.Unknow)
            {
                singleForkCraneRemoteHand.Result = false;
                singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 操作类型不可为 {singleForkCraneRemoteHand.HandType}";
                return Json(singleForkCraneRemoteHand);
            }

            var device = DeviceConverter.ToDevice<CraneDevice>(singleForkCraneRemoteHand.Device);
            if (device == null)
            {
                singleForkCraneRemoteHand.Result = false;
                singleForkCraneRemoteHand.Message = $"未找到堆垛机设备{singleForkCraneRemoteHand.Device}";
                return Json(singleForkCraneRemoteHand);
            }
            if (!device.IsConnected)
            {
                singleForkCraneRemoteHand.Result = false;
                singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 未链接，无法发送指令";
                return Json(singleForkCraneRemoteHand);
            }
            else
            {
                try
                {
                    switch (singleForkCraneRemoteHand.HandType)
                    {
                        case HandTypes.CancleTask:
                            device.CancleTask();
                            singleForkCraneRemoteHand.Result = true;
                            singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 执行 {singleForkCraneRemoteHand.HandType} 指令成功";
                            break;
                        case HandTypes.ResetAlarm:
                            device.ResetWarn(device.Locker);
                            singleForkCraneRemoteHand.Result = true;
                            singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 执行 {singleForkCraneRemoteHand.HandType} 指令成功";
                            break;
                        case HandTypes.RemoteEmergency:
                            device.EmergencyStop();
                            singleForkCraneRemoteHand.Result = true;
                            singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 执行 {singleForkCraneRemoteHand.HandType} 指令成功";
                            break;
                        case HandTypes.CancleRemoteEmergency:
                            device.CancelEmergencyStop(device.Locker);
                            singleForkCraneRemoteHand.Result = true;
                            singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 执行 {singleForkCraneRemoteHand.HandType} 指令成功";
                            break;
                        case HandTypes.Unknow:
                        default:
                            singleForkCraneRemoteHand.Result = false;
                            singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 未执行 {singleForkCraneRemoteHand.HandType} 指令";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);
                    singleForkCraneRemoteHand.Result = false;
                    singleForkCraneRemoteHand.Message = $"设备 {singleForkCraneRemoteHand.Device} 执行 {singleForkCraneRemoteHand.HandType} 指令时发生异常，异常消息{ex}";
                }
            }
            return Json(singleForkCraneRemoteHand);
        }
        #endregion
    }

    public static class RemoteHandTaskHelper
    {
        static object objLocker = new object();
        public static string Holder;
        public static bool Hold(string holerMsg)
        {
            lock (objLocker)
            {
                if (string.IsNullOrWhiteSpace(Holder))
                {
                    Holder = holerMsg;
                    return true;
                }
                else if (holerMsg == Holder)
                    return true;

                return false;
            }
        }
    }

    public class AddTaskProvideInfos
    {
        public List<TaskSource> Sources { get; set; } = new List<TaskSource>();
        public List<string> BizTypes { get; set; } = new List<string>();
        public List<string> TaskTypes { get; set; } = new List<string>();
        public List<string> TaskAdditionalInfos { get; set; } = new List<string>();

        public List<WcsLocation> WcsLocations { get; set; } = new List<WcsLocation>();
    }
}