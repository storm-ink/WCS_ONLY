using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs;
namespace Wcs.Framework
{

    public class DeviceWarningEventHandler
    {
        List<IDeviceErrorEventHandler> _handlers;
        Logger _logger;
        public DeviceWarningEventHandler()
        {
            _handlers = new List<IDeviceErrorEventHandler>();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public DeviceWarningEventHandler(IEnumerable<IDeviceErrorEventHandler> handlers):this()
        {
            _handlers.AddRange(handlers);
        }

        public void Add(IDeviceErrorEventHandler hander)
        {
            _handlers.Add(hander);
        }

        public Int32 Count
        {
            get
            {
                return _handlers.Count;
            }
        }

        public void Handle(Device device,DeviceWarningEventArgs args)
        {
            if (_handlers.Count == 0)
            {
                args.Handled = true;
                return;
            }

            try
            {
                using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope())
                {
                    foreach (var handler in _handlers)
                    {
                        try
                        {
                            handler.Handle(device, args);
                        }
                        catch (Exception ex)
                        {
                            args.Handled = false;
                            _logger.Warn1(string.Format("{0} 在处理 {1} 时发生异常 {2}，本次事务未提交", this, handler,ex),this,handler);
                        }

                        if (args.Handled == false)
                        {
                            _logger.Warn1(String.Format("{0} 在处理 {1} 时失败，本次事务未提交", handler, args), this, handler);
                            break;
                        }
                    }

                    if (args.Handled)
                    {
                        tsc.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }
    }
}
