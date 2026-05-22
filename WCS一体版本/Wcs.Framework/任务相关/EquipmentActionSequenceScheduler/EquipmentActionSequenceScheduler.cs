using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wcs.Framework.Cfg;

namespace Wcs.Framework
{
    public class EquipmentActionSequenceScheduler
    {
        public Logger Logger { get; protected set; }
        public EquipmentActionSequence EquipmentActionSequence { get; private set; }
        public TaskableDevice Device { get; private set; }
        public EquipmentActionSequenceSchedulerActionFilter[] EquipmentActionSequenceSchedulerActionFilters { get; private set; }
        public EquipmentActionSequenceScheduler(EquipmentActionSequence equipmentActionSequence,EquipmentActionSequenceSchedulerActionFilter[] actionFilters)
        {
            this.Logger = NLog.LogManager.GetCurrentClassLogger();
            this.EquipmentActionSequence = equipmentActionSequence;

            var deviceElement = WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .SingleOrDefault(x => string.Equals(x.Device.Name, equipmentActionSequence.DeviceName, StringComparison.CurrentCultureIgnoreCase));
            if (deviceElement == null)
            {
                throw new ArgumentOutOfRangeException(string.Format("未找到设备名为 “{0}” 的设备对象", equipmentActionSequence.DeviceName));
            }
            if (!(deviceElement.Device is TaskableDevice))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} 不是 {1} 类型。请确认是否后期修改了配置文件而未同步数据库数据。", deviceElement.Device,typeof(TaskableDevice)));
            }
            this.Device = (TaskableDevice)deviceElement.Device;

            EquipmentActionSequenceSchedulerActionFilter[] basicActionFilters =
                new EquipmentActionSequenceSchedulerActionFilter[]{
                    new BasicEquipmentActionSequenceSchedulerActionFilter()
                };
            if (actionFilters == null || actionFilters.Length == 0)
            {
                this.EquipmentActionSequenceSchedulerActionFilters = basicActionFilters;
            }
            else
            {
                this.EquipmentActionSequenceSchedulerActionFilters = basicActionFilters.Concat(actionFilters).ToArray();
            }
         }

        void Proc()
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(50);
                        if (!this.Device.IsConnected)
                        {
                            continue;
                        }

                        if (!this.Device.IsIdle)
                        {
                            continue;
                        }

                        var action = GetNextEquipmentAction();
                        if (action == null)
                        {
                            continue;
                        }

                        if (this.EquipmentActionSequenceSchedulerActionFilters.Any(x => !x.CanSend(this, action)))
                        {
                            continue;
                        }

                        try
                        {
                            Device.SendTask(action);
                            action.Status = EquipmentActionStatus.Executing;

                            try
                            {
                                 using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                                {
                                    var act = unitOfWork.session.Get<EquipmentAction>(action.Id);
                                    act.Status = EquipmentActionStatus.Executing;
                                    unitOfWork.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Logger.Error1(ex, this, action);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error1(ex, this, action);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error1(ex, this);
                }
            });
        }

        public virtual EquipmentAction GetNextEquipmentAction()
        {
            if (this.EquipmentActionSequence.CurrentEquipmentAction != null 
                && this.EquipmentActionSequence.CurrentEquipmentAction.Status==EquipmentActionStatus.New)
            {
                return this.EquipmentActionSequence.CurrentEquipmentAction;
            }

            var action = this.EquipmentActionSequence
                            .Actions
                           .OrderByDescending(x => x.Movement.Task.Priority)
                           .ThenBy(x => x.SequenceOrdering)
                           .ThenBy(x => x.Id)
                           .FirstOrDefault(x => x.Id != 0
                               && x.Status == EquipmentActionStatus.New
                               && x.CanPerform()
                               && EquipmentActionSequenceSchedulerActionFilters.All(filter => filter.CanSend(this,x))
                               );

            return action;
        }
    }
}
