using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 堆垛机移动盘点任务完成处理程序
    /// 任务类型：盘点任务
    /// </summary>
    public class CraneMovePDTaskCompeletedHandler_Column : ITaskEventHandler<TaskCompletedEventArgs>
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is CraneDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(CraneDevice), this);
                    _logger.Debug1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    return;
                }

                _logger.Trace1(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);

                var action = session.Query<CraneAutomaticTransferWithStepByStepAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(CraneMoveAction));
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;

                    return;
                }
                if (action.Movement.Task.TaskType != "盘点任务" || !action.Movement.Task.AdditionalInfo.ContainsKey("_禁止堆垛机取货"))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但所属主任务非盘点任务，忽略本信号", device, args.EquipmentTaskId);
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;

                    return;
                }

                var _device = (CraneDevice)device;
                if (!_device.IsConnected)
                    throw new Exception(String.Format("设备{0}未连接", _device.Name));
                if (_device.LastStatus == null)
                    throw new Exception(String.Format("设备{0}状态获取失败", _device.Name));

                if (!String.IsNullOrEmpty(_device.LastStatus.ExtendInfo))
                {
                    RackLocation[] _locations;
                    if (!action.Movement.Task.AdditionalInfo.ContainsKey(action.Movement.Task.StartLocation.UserCode) || !action.Movement.Task.AdditionalInfo.ContainsKey(action.Movement.Task.EndLocation.UserCode))
                    {
                        Location startLocation = LocationConverter.ToLocation(action.Movement.StartLocation);
                        RackLocation pickingLocation = (RackLocation)startLocation;
                        Location endLocation = LocationConverter.ToLocation(action.Movement.EndLocation);
                        RackLocation dropingLocation = (RackLocation)endLocation;

                        _locations = _device.Locations
                                                    .Select(x => (RackLocation)x)
                                                    .Where(x => x.Column >= Math.Min(pickingLocation.Column, dropingLocation.Column)
                                                                && x.Column <= Math.Max(pickingLocation.Column, dropingLocation.Column)
                                                                && x.Level >= Math.Min(pickingLocation.Level, dropingLocation.Level)
                                                                && x.Level <= Math.Max(pickingLocation.Level, dropingLocation.Level))
                                                                .OrderBy(x => (Int32)(x.ForkDirection))
                                                                .ThenBy(x => x.Level).ToArray();
                    }
                    else
                        _locations = action.Movement.Task.AdditionalInfo
                            .Where(x => x.Value == "-1")
                            .Select(x => (RackLocation)LocationConverter.UserCodeToLcation(x.Key))
                            .OrderBy(x => (Int32)(x.ForkDirection))
                            .ThenBy(x => x.Level).ToArray();

                    ///排序
                    int[,] _arr = GetResult(_device.LastStatus.ExtendInfo);
                    foreach (var item in _locations)
                    {
                        if (item.ForkDirection == ForkDirection.Left)
                        {
                            if (!action.Movement.Task.AdditionalInfo.ContainsKey(item.UserCode))
                                action.Movement.Task.AdditionalInfo.Add(item.UserCode, _arr[item.Level / 9, (item.Level - 1) % 8].ToString());
                            else
                                action.Movement.Task.AdditionalInfo[item.UserCode] = _arr[item.Level / 9, (item.Level - 1) % 8].ToString();
                        }
                        if (item.ForkDirection == ForkDirection.Right)
                        {
                            if (!action.Movement.Task.AdditionalInfo.ContainsKey(item.UserCode))
                                action.Movement.Task.AdditionalInfo.Add(item.UserCode, _arr[4 + item.Level / 9, (item.Level - 1) % 8].ToString());
                            else
                                action.Movement.Task.AdditionalInfo[item.UserCode] = _arr[4 + item.Level / 9, (item.Level - 1) % 8].ToString();
                        }
                    }
                }

                args.Handled = true;
            }
            catch (Exception ex)
            {
                String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但 {2} 在处理时发生异常{3}，操作已中止", device, args.EquipmentTaskId, this, ex.Message);
                _logger.Error1(new Exception(msg, ex), this, args, null, args.EquipmentTaskId);
                args.Handled = false;
                throw;
            }
        }

        private int[,] GetResult(string extendInfo)
        {
            Int32[,] _arr = new Int32[extendInfo.Length / 2, 8];
            for (int i = 0; i < extendInfo.Length / 2; i++)
            {
                String _str = System.Text.Encoding.ASCII.GetString(System.Text.Encoding.ASCII.GetBytes(extendInfo.Substring(i * 2, 2)).Reverse().ToArray());
                Console.WriteLine(_str);
                byte _byte = byte.Parse(_str, System.Globalization.NumberStyles.HexNumber);
                for (int j = 0; j < 8; j++)
                {
                    _arr[i, j] = _byte >> j & 1;
                }
            }
            return _arr;
        }
    }
}
