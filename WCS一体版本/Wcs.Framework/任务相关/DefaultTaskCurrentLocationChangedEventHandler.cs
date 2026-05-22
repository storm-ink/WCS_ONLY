using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.Events;
using Wcs.Framework.EventBus;
namespace Wcs.Framework
{
    public sealed class DefaultTaskCurrentLocationChangedEventHandler:ITaskEventHandler<TaskCurrentLocationChangedEventArgs>
    {
        static Logger _logger=LogManager.GetCurrentClassLogger();
        public DefaultTaskCurrentLocationChangedEventHandler()
        {
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCurrentLocationChangedEventArgs args)
        {
            try
            {
                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的位置改变事件...", this, args.EquipmentTaskId), this, args,null,args.EquipmentTaskId);

                List<IEvent> events = new List<IEvent>();
            
                _logger.Trace1(string.Format("获取任务号为 {0} 的物理动作", args.EquipmentTaskId), this, args,null,args.EquipmentTaskId);

                var action = session.Query<EquipmentAction>().SingleOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 当前位置改变信号,但未找到对应的物理动作对象，忽略本信号", device, args.EquipmentTaskId);
                    args.Handled = true;
                    _logger.Warn1(msg, this, args);
                    return;
                }

                _logger.Trace1(string.Format("找到 {0}", action), this, action);

                //session.Lock(action, NHibernate.LockMode.Upgrade);

                if (action.Status == EquipmentActionStatus.Completed)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 当前位置改变信号,但找到的物理动作 {2} 状态标识其已经完成，忽略本信号", device, args.EquipmentTaskId, action);
                    _logger.Warn1(msg, this, action);
                    args.Handled = true;
                    return;
                }
                if (action.Movement.Task.CurrentLocation.UserCode == args.CurrentLocation.UserCode)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 当前位置改变信号,但是任务当前位置已经处于 {2}，忽略本信号", device, args.EquipmentTaskId, args.CurrentLocation.UserCode);
                    _logger.Trace1(msg, this, action.Movement.Task);
                    args.Handled = true;
                    return;
                }


                //Route _route = null;
                //if (action.Movement.RouteId != null)
                //    _route = Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteCollection.Routes.FirstOrDefault(x => x.Id == action.Movement.RouteId);

                //if (_route != null)
                //{
                //    var _currentLocation = LocationConverter.ToLocation(action.Movement.Task.CurrentLocation);
                //    var _currentLocIndex = _route.Locations.FindIndex(x => x.DeviceCode == _currentLocation.DeviceCode || _currentLocation.Synonymous.Any(y => y.DeviceCode == x.DeviceCode));
                //    var _argsLocIndex = _route.Locations.FindIndex(x => x.DeviceCode == args.CurrentLocation.DeviceCode || args.CurrentLocation.Synonymous.Any(y => y.DeviceCode == x.DeviceCode));
                //    if (_currentLocIndex < _argsLocIndex)
                //        action.Movement.Task.CurrentLocation = new LocationInfo(args.CurrentLocation.Device.Name,args.CurrentLocation.DeviceCode,args.CurrentLocation.UserCode);
                //}
                //else
                    action.Movement.Task.CurrentLocation = LocationConverter.ToLocationInfo(args.CurrentLocation);

                _logger.Trace1(string.Format("更新 {0}，CurrentLocation:{1}", action.Movement.Task, action.Movement.Task.CurrentLocation), this, action.Movement.Task);
               

                args.Handled = true;

                args.AddLazyEvent(new TaskCurrentLocationChangedEvent(action.Movement.Task.Id, action.Movement.Task.TaskCode, action.Movement.Task.CurrentLocation));


                _logger.Trace1(string.Format("{0} 已完成对 {1} 的位置改变事件处理过程.", this, args.EquipmentTaskId), this, args,null,args.EquipmentTaskId);
            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 当前位置改变信号,但 {2} 在处理时发生异常，操作已中止", device, args.EquipmentTaskId,this);
                _logger.Error1(new Exception(msg,ex), this,args,null,args.EquipmentTaskId);
                args.Handled = false;
            }
        }
    }
}
