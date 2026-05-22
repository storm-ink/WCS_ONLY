using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wcs.Framework.Cfg;
using System.Data.SqlClient;
using System.Data;
namespace Wcs.Framework
{
    public class EquipmentActionSequenceScheduler
    {
        public Logger Logger { get; protected set; }
        public TaskableDevice Device { get; private set; }
        public EquipmentAction[] Actions { get; private set; }
        public EquipmentActionSchedulerFilter[] ActionFilters { get; private set; }
        public EquipmentActionSequenceScheduler(EquipmentActionSequence sequence,EquipmentActionSchedulerFilter[] actionFilters)
        {
            this.Logger = NLog.LogManager.GetCurrentClassLogger();
            this.Actions = sequence.Actions.ToArray();

            var deviceElement = WcsConfiguration
                .Instance
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .SingleOrDefault(x => string.Equals(x.Device.Name, sequence.DeviceName, StringComparison.CurrentCultureIgnoreCase));
            if (deviceElement == null)
            {
                throw new ArgumentOutOfRangeException(string.Format("未找到设备名为 “{0}” 的设备对象", sequence.DeviceName));
            }
            if (!(deviceElement.Device is TaskableDevice))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} 不是 {1} 类型。请确认是否后期修改了配置文件而未同步数据库数据。", deviceElement.Device,typeof(TaskableDevice)));
            }
            this.Device = (TaskableDevice)deviceElement.Device;

            EquipmentActionSchedulerFilter[] basicActionFilters =
                new EquipmentActionSchedulerFilter[]{
                    new BasicEquipmentActionSequenceSchedulerActionFilter()
                };
            if (actionFilters == null || actionFilters.Length == 0)
            {
                this.ActionFilters = basicActionFilters;
            }
            else
            {
                this.ActionFilters = basicActionFilters.Concat(actionFilters).ToArray();
            }

#warning 应考虑启动的时机
            Proc();
         }

#warning 此方法应该改造
        void Proc()
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (true)
                {
                    try
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

                        var availableActions = GetAvailableActions();
                        if (availableActions == null)
                        {
                            continue;
                        }

                        foreach (var action in availableActions)
                        {
                            Boolean negatory = false;
                            foreach (var filter in this.ActionFilters)
                            {
                                if (!filter.CanSend(this, action))
                                {
                                    negatory = true;
                                    this.Logger.Trace1(string.Format("{0} 被 {1} 否决了发送操作。", action, filter), this, action);
                                    break;
                                }
                            }
                            if (negatory)
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

                            Thread.Sleep(50);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error1(ex, this);
                    }
                }
            });
        }

        /// <summary>
        /// 获取当前可以发送的动作集合（有可能返回 null）
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<EquipmentAction> GetAvailableActions()
        {
            if (!this.Device.AllowConcurrency && this.EquipmentActionSequence.CurrentEquipmentAction!=null)
            {
                if (this.EquipmentActionSequence.CurrentEquipmentAction.Status == EquipmentActionStatus.New)
                {
                    return new EquipmentAction[]{this.EquipmentActionSequence.CurrentEquipmentAction};
                }
                else
                {
                    return null;
                }
            }

            var result = this.EquipmentActionSequence
                            .Actions
                           .OrderByDescending(x => x.Movement.Task.Priority)
                           .ThenBy(x => x.SequenceOrdering)
                           .ThenBy(x => x.Id)
                           .Where(x => x.Id != 0
                               && x.Status == EquipmentActionStatus.New
                               && x.CanPerform()
                               && ActionFilters.All(filter => filter.CanSend(this,x))
                               );

            return result;
        }
    }
}
