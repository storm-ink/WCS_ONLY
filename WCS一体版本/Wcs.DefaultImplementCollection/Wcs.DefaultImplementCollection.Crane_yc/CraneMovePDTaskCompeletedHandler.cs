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
    public class CraneMovePDTaskCompeletedHandler : ITaskEventHandler<TaskCompletedEventArgs>
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

                var action = session.Query<CraneMoveAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(CraneMoveAction));
                    _logger.Warn1(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;

                    return;
                }
                if (action.Movement.Task.TaskType != "盘点任务")
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

                if (!action.Movement.Task.AdditionalInfo.ContainsKey(action.Movement.EndLocation.UserCode))
                {
                    if (_device.LastStatus.TaskId.StartsWith("PD", StringComparison.CurrentCultureIgnoreCase)
                        && Convert.ToInt32(_device.LastStatus.TaskId.Substring(2, 6)) == action.EquipmentTaskId
                        && (_device.LastStatus.State == CraneStatus.右探无货
                        || _device.LastStatus.State == CraneStatus.右探有货
                        || _device.LastStatus.State == CraneStatus.左探无货
                        || _device.LastStatus.State == CraneStatus.左探有货
                        || _device.LastStatus.State == CraneStatus.左无右无
                        || _device.LastStatus.State == CraneStatus.左无右有
                        || _device.LastStatus.State == CraneStatus.左有右无
                        || _device.LastStatus.State == CraneStatus.左有右有)
                        )
                    {
                        var end = (RackLocation)LocationConverter.ToLocation(action.Movement.EndLocation);
                        switch (_device.LastStatus.State)
                        {
                            case CraneStatus.左探有货:
                            case CraneStatus.右探有货:
                                action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                break;
                            case CraneStatus.左探无货:
                            case CraneStatus.右探无货:
                                action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                break;
                            case CraneStatus.左有右有:
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Left)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Right);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                }
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Right)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Left);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                }
                                break;
                            case CraneStatus.左有右无:
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Left)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Right);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                }
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Right)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Left);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                }
                                break;
                            case CraneStatus.左无右有:
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Left)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Right);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                }
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Right)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Left);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultOK).ToString());
                                }
                                break;
                            case CraneStatus.左无右无:
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Left)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Right);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                }
                                if (end.ForkDirection != null && end.ForkDirection == ForkDirection.Right)
                                {
                                    action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                    var _end = _device.Locations.Select(x => (RackLocation)x).FirstOrDefault(x => x.Column == end.Column && x.Level == end.Level && x.ForkDirection != null && x.ForkDirection == ForkDirection.Left);
                                    if (_end != null)
                                        action.Movement.Task.AdditionalInfo.Add(_end.UserCode, ((Int32)CranePDResult._getResultFailed).ToString());
                                }
                                break;
                            default:
                                action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultError).ToString());
                                break;
                        }
                    }
                    else
                    {
                        action.Movement.Task.AdditionalInfo.Add(action.Movement.EndLocation.UserCode, ((Int32)CranePDResult._getResultError).ToString());
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
    }
}
