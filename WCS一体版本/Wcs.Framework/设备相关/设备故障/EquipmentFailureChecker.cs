using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;

namespace Wcs.Framework
{
    public class EquipmentFailureChecker:IDisposable
    {
        protected Boolean _disposing;
        protected Logger _logger = LogManager.GetCurrentClassLogger();
        static List<EquipmentFailureChecker> _checkers = new List<EquipmentFailureChecker>();
        static Thread _thread;
        Boolean _isRunning = false;
        static EquipmentFailureChecker()
        {
            _thread = new Thread(Check);
            _thread.IsBackground = true;
            _thread.Name = "设备故障记录器";

            _thread.StartAndManaged();
        }

        public EquipmentFailureChecker(Device device)
        {
            this.Device = device;
        }

        public Device Device { get; private set; }

        public Boolean IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public virtual void Dispose()
        {
            RemoveChecker(this);

            _disposing = true;

            _isRunning = false;
        }

        public void Start()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("正在运行中，请勿重复操作.");
            }

            AddChecker(this);
        }

        public override string ToString()
        {
            return String.Format("{0}的故障记录器", Device);
        }

        protected virtual void Proc()
        {
            try
            {
                var failures = this.Device.EquipmentFailures;
                foreach (var f in failures)
                {
                    if (f.IsOverdued)
                    {
                        _logger.Trace1(string.Format("{0} 已过时.", f), this);

                        f.FinishedAt = DateTime.Now;
                        this.Device.RemoveFailure(f);

                        _logger.Trace1(string.Format("{0} 已移除.", f), this);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
            }
        }

        static void AddChecker(EquipmentFailureChecker checker)
        {
            lock (_checkers)
            {
                if (_checkers.Any(x => x == checker))
                {
                    return;
                }

                _checkers.Add(checker);
            }
        }

        static void Check()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(300);
                lock (_checkers)
                {
                    foreach (var checker in _checkers)
                    {
                        checker.Proc();
                    }
                }
            }
        }
        static void RemoveChecker(EquipmentFailureChecker checker)
        {
            lock (_checkers)
            {
                if (!_checkers.Any(x => x == checker))
                {
                    return;
                }

                _checkers.Remove(checker);
            }
        }
    }
}
