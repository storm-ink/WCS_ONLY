using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NLog;
using Wcs.Framework.EventBus;
namespace Wcs.Framework
{
    public sealed class DefaultTaskRequestEventHandler : ITaskEventHandler<TaskRequestEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public DefaultTaskRequestEventHandler()
        {

        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskRequestEventArgs args)
        {
            try
            {
                List<IEvent> events = new List<IEvent>();

                _logger.Trace1(string.Format("{0} 开始处理位置 {1} 的请求事件...", this, args.Location), this, args);


                _logger.Trace1(string.Format("检查位置 {0} 是否已存在未使用的 Request 对象", args.Location), this, args);

                var request = session.Query<Request>().SingleOrDefault(x => x.Source == LocationConverter.ToLocationInfo(args.Location));
                if (request != null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务请求信号,但发现本地已存在一个同义对象 {2}，忽略本信号", device, args, request);
                    _logger.Warn1(msg, this, args);
                    args.Handled = true;
                    return;
                }
                _logger.Trace1(string.Format("检查成功，未发现未使用的 Request 对象."), this, args);
                request = new Request();
                request.Source = LocationConverter.ToLocationInfo(args.Location);

                foreach (var requestProcess in Wcs.Framework.Cfg.WcsConfiguration.Instance.RequestProcessesElement.RequestProcesses)
                {
                    RequestProcessResult processResult = new RequestProcessResult();
                    requestProcess.Process(request, session, processResult);

                    if (processResult.Result == RequestProcessResultStatus.Abort)
                    {
                        args.Handled = false;
                        _logger.Warn1(string.Format("{0} 在对 {1} 进行加工时要求中止操作.备注：{2}。", requestProcess, request, processResult.Description), this, args);
                        return;
                    }
                }

                session.Save(request);


                events.Add(new Events.RequestAddedEvent(request));

                _logger.Trace1(string.Format("{0} 位置 {1} 新的请求 {2} 被创建", device, args.Location, request), this, request);


                _logger.Trace1(string.Format("{0} 已完成对位置 {1} 的请求事件处理过程.", this, args.Location), this, args);

                args.Handled = true;

                args.AddLazyEvents(events);
            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的位置 {1} 任务请求信号,但 {2} 在处理时发生异常，操作已中止", device, args.Location, this);
                _logger.Error1(new Exception(msg, ex), this, args);
                args.Handled = false;
            }
        }
    }
}
