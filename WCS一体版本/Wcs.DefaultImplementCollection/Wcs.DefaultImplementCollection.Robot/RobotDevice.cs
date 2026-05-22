using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Wcs.Framework;
using Wcs.Framework.EventBus;
using Wcs.Framework.Events;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手设备
    /// </summary>
    [System.ComponentModel.Description("机械手-Sineva")]
    public class RobotDevice : TcpProtocolTaskableDevice
    {
        /// <summary>
        /// 框架提供的构造函数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="no"></param>
        /// <param name="receiveTimeout"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="sendTimeout"></param>
        /// <param name="ipEndPoint"></param>
        /// <param name="bindEndPoint"></param>
        /// <param name="dataReceiver"></param>
        public RobotDevice(string name, Int32 no, Int32 receiveTimeout, Int32 connectTimeout, Int32 sendTimeout, IPEndPoint ipEndPoint, IPEndPoint bindEndPoint, IDataReceiver dataReceiver)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, false, ipEndPoint, bindEndPoint, dataReceiver)
        {
            //this.DeviceWarningFactory = new DefaultRobotDeviceWarningFactory();
        }

        /// <summary>
        /// RobotCommand外挂程序
        /// </summary>
        public RobotEquipmentActionToAddCommandPlugin EquipmentActionToAddCommandPlugin { get; set; }

        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                if (this.LastState != null && this.LastState.RobotTask.TaskId != 0)
                    return new int[] { (int)this.LastState.RobotTask.TaskId };
                return new int[0];
            }
        }

        public override IsIdleResult IsIdle
        {
            get
            {
                if (this.Locker != null && !this.Locker.IsEmpty)
                    return new IsIdleResult(false, "锁");
                if (this.LastState == null)
                    return new IsIdleResult(false, "状态空");
                if (this.LastState.RobotTask.TaskId != 0)
                    return new IsIdleResult(false, "任务号不为0");
                if (this.LastState.RobotBasicMessage.Mode == RobotModes.AlarmDown
                    || this.LastState.RobotBasicMessage.Mode == RobotModes.Manual
                    || this.LastState.RobotBasicMessage.Mode == RobotModes.UnKnown
                    //|| this.LastState.RobotBasicMessage.Mode == RobotModes.WaitingEnable
                    )
                    return new IsIdleResult(false, "报警，手动或者未知状态");

                return new IsIdleResult(true, "");
            }
        }

        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                if (this.Locker != null && !this.Locker.IsEmpty)
                    result.Add("设备被锁定");
                if (this.LastState == null)
                    result.Add("设备状态未获取");
                else
                {
                    if (this.LastState.RobotBasicMessage.Mode == RobotModes.AlarmDown
                        || this.LastState.RobotBasicMessage.Mode == RobotModes.Manual
                        || this.LastState.RobotBasicMessage.Mode == RobotModes.UnKnown
                        //|| this.LastState.RobotBasicMessage.Mode == RobotModes.WaitingEnable
                        )
                        result.Add($"设备状态处于 {this.LastState.RobotBasicMessage.Mode.GetDescription()} 状态");

                    if (this.LastState.RobotBasicMessage.Alarm != 0)
                        result.Add($"设备报故障");

                    var alarms = this.LastState.AlarmList.Where(x => x != 0);
                    if (alarms.Count() > 0) 
                    {
                        foreach (var item in alarms)
                        {
                            //result.Add(string.Format("处于报警状态 {0}_{1}", item, this.DeviceWarningFactory.Create(this, item.ToString(), null).Description));
                        }
                    }
                       
                       
                   
                }
                return result.ToArray();
            }
        }

        public override void CancelTask(EquipmentAction action)
        {
            if (this.LastState != null)
            {
                if (this.LastState.RobotBasicMessage.Mode == RobotModes.Running)
                    throw new Exception("设备在运行中无法暂停任务");
                base.CancelTask(action);
            }
        }


        public override IDeviceUserInterface CreateUserInterface()
        {
            return new RobotDeviceUserInterface(this);
        }

        public override TState Read<TState>()
        {
            return null;
        }
        public void PublicFireTaskRunningEvent(TaskRunningEventArgs args)
        {
            FireTaskRunningEvent(args);
        }

        //public override void stateUpdateProc(object obj)
        //{

        //}
        public DateTime LastStateAt { get; set; }
        public RobotStatusTelexTransferObject LastState { get; set; }
        public RobotStatusTelexTransferObject LastState_Old { get; set; }

        public int LastFireTaskCompletedEventTaskId = 0;
        protected override void OnDataReceived(NetPacket netPacket, NetTransferObject netTransferObject)
        {
            if (!(netTransferObject is RobotStatusTelexTransferObject))
                return;

            LastStateAt = DateTime.Now;
            LastState = (RobotStatusTelexTransferObject)netTransferObject;

            if (LastState.RobotTask != null && LastState.RobotTask.TaskId != 0&&(LastState.RobotTask.HandShake==HandShake.Clear|| LastState.RobotTask.HandShake == HandShake.FosbDAbort))
            {
                if (LastState.RobotTask.HandShake == HandShake.Clear)
                {
                    if (LastFireTaskCompletedEventTaskId != (int)LastState.RobotTask.TaskId)
                    {
                        _logger.Info1($"收到 {this.Name} 的任务 {LastState.RobotTask.TaskId} 完成信号，即将引发完成事件", this);
                        FireTaskCompletedEvent(new TaskCompletedEventArgs((int)LastState.RobotTask.TaskId));
                        LastFireTaskCompletedEventTaskId = (int)LastState.RobotTask.TaskId;
                    }

                }

                if (LastState.RobotTask.HandShake == HandShake.FosbDAbort)
                {
                    if (LastFireTaskCompletedEventTaskId != (int)LastState.RobotTask.TaskId)
                    {
                        //将任务标记为FOSB校验不通过
                        using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                            if (action != null)
                            {
                                //任务上报WMS，校验不通过则上报取消状态
                                if (!action.Movement.Task.AdditionalInfo.ContainsKey("FOSBABORT"))
                                {
                                    action.Movement.Task.AdditionalInfo.Add("FOSBABORT", "True");
                                    unitOfWork.session.Update(action);
                                }
                               
                            }
                            unitOfWork.Commit();
                            LastFireTaskCompletedEventTaskId = (int)LastState.RobotTask.TaskId;
                        }






                        FireTaskCompletedEvent(new TaskCompletedEventArgs((int)LastState.RobotTask.TaskId));
                        LastFireTaskCompletedEventTaskId = (int)LastState.RobotTask.TaskId;
                    }
                }
               
            }
            //start add by zhengge 20230831 新增ROBOThandshake=8时，自动将此任务取消
            if (LastState.RobotTask != null && LastState.RobotTask.TaskId != 0 && LastState.RobotTask.HandShake == HandShake.FosbDAbort)
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId== (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                    if (action != null) 
                    {
                        Wcs.Framework.TaskHelper.Suspend(action.Movement.Task.Id);
                        //Wcs.Framework.TaskHelper.CancelTask(action.Movement.Task.Id);
                    }
                    unitOfWork.Commit();
                    LastFireTaskCompletedEventTaskId = (int)LastState.RobotTask.TaskId;
                }             
            }
            //end add by zhengge 20230831 新增ROBOThandshake=8时，自动将此任务取消

            //uodate zhengge20240605 新增，当机械手去玩货物后，向WMS上报
            
            if (LastState_Old!=null)
            {
                if (LastState.RobotBasicMessage.Catch == 1 && LastState.RobotBasicMessage.Catch != LastState_Old.RobotBasicMessage.Catch)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                        if (action != null&& !action.Movement.Task.AdditionalInfo.ContainsKey("RBPickOK"))
                        {
                            if (action.Movement.Task.EndLocation.DeviceCode!="2073")
                            {
                                action.Movement.Task.AdditionalInfo.Add("RBPickOK", "1");
                                unitOfWork.session.Update(action);
                                unitOfWork.Commit();
                                EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, TaskStatus.Executing, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
                            }
                          

                        }
                        unitOfWork.Commit();
                    }

                }
                else if (LastState.RobotBasicMessage.Catch == 2 && LastState.RobotBasicMessage.Catch != LastState_Old.RobotBasicMessage.Catch)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                        if (action != null && !action.Movement.Task.AdditionalInfo.ContainsKey("RBPickOK"))
                        {
                            if (action.Movement.Task.EndLocation.DeviceCode == "2073")
                            {
                                action.Movement.Task.AdditionalInfo.Add("RBPickOK", "2");
                                unitOfWork.session.Update(action);
                                unitOfWork.Commit();
                                EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, TaskStatus.Executing, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
                            }
                           

                        }

                        unitOfWork.Commit();

                    }
                }
                LastState_Old = LastState;
            }
            else
            {
                if (LastState.RobotBasicMessage.Catch == 1 )
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                        if (action != null && !action.Movement.Task.AdditionalInfo.ContainsKey("RBPickOK"))
                        {
                            if (action.Movement.Task.EndLocation.DeviceCode != "2073")
                            {
                                action.Movement.Task.AdditionalInfo.Add("RBPickOK", "1");
                                unitOfWork.session.Update(action);
                                unitOfWork.Commit();
                                EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, TaskStatus.Executing, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
                            }


                        }
                        unitOfWork.Commit();
                    }
                }
                else if (LastState.RobotBasicMessage.Catch == 2)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        var action = unitOfWork.session.Query<EquipmentAction>().Where(x => x.EquipmentTaskId == (int)LastState.RobotTask.TaskId).ToList().FirstOrDefault();
                        if (action != null && !action.Movement.Task.AdditionalInfo.ContainsKey("RBPickOK"))
                        {
                            if (action.Movement.Task.EndLocation.DeviceCode == "2073")
                            {
                                action.Movement.Task.AdditionalInfo.Add("RBPickOK", "2");
                                unitOfWork.session.Update(action);
                                unitOfWork.Commit();
                                EventBus.Instance.Publish(new TaskStatusChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, TaskStatus.Executing, action.Movement.Task.BizType, action.Movement.Task.Source, action.Movement.Task.TaskType));
                            }


                        }

                        unitOfWork.Commit();

                    }
                }
                LastState_Old = LastState;

            }
          


        }
        public void AcceptLocation(params RobotLocation[] locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            if (locations.Any(x => x.Device != null && x.Device != this))
            {
                throw new InvalidOperationException("有已被分配给其它设备的位置对象，而这些对象并不允许修改所属设备。");
            }

            foreach (var loc in locations)
            {
                if (_locations.Any(x => x.ToConvertibleCode() == loc.ToConvertibleCode()))
                {
                    throw new InvalidOperationException(String.Format("位置 {0} 在 {1} 中已存在", loc, this));
                }
            }

            _locations.AddRange(locations);
        }
    }
}
