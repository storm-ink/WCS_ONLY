using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Diagnostics;

namespace Wcs.Framework
{
    /// <summary>
    /// 一个用于获取对象【锁对象】的扩展集合。
    /// </summary>
    public static class LockerExtentions
    {
        
        static Logger _logger = LogManager.GetCurrentClassLogger();

        static List<String> _lockers = new List<string>();

        /// <summary>
        /// 尝试创建指定任务的锁信息。该操作不会执行LOCK操作，只会创建一个Lock对象。
        /// </summary>
        /// <param name="tsk">要创建锁信息的任务</param>
        /// <returns></returns>
        public static Boolean EnterLock(this Task tsk)
        {
            return tsk.InternalEnterLock(null);
        }

        static Boolean InternalEnterLock(this Task tsk,EquipmentAction act)
        {
            var key = String.Format("{0}#{1}", tsk.GetType().Name, tsk.TaskCode);

            lock (_lockers)
            {
                if (_lockers.Any(x => x == key))
                {
                    if (act != null)
                    {
                        _logger.Warn1(String.Format("Enter Lock {0} Failed.", tsk), typeof(LockerExtentions), act);
                    }
                    else
                    {
                        _logger.Warn1(String.Format("Enter Lock {0} Failed.", tsk), typeof(LockerExtentions), tsk);
                    }
                    return false;
                }

                _lockers.Add(key);

                if (act != null)
                {
                    _logger.Debug1(String.Format("Enter Lock {0} Success.", tsk), typeof(LockerExtentions), act);
                }
                else
                {
                    _logger.Debug1(String.Format("Enter Lock {0} Success.", tsk), typeof(LockerExtentions), tsk);
                }

                return true;
            }
        }

        /// <summary>
        /// 尝试创建指定物理动作的锁信息。该操作不会执行LOCK操作，只会创建一个Lock对象。
        /// </summary>
        /// <param name="act">要创建锁信息的物理动作</param>
        /// <param name="tryLockAggregateRoot">是否深度创建聚合根锁信号</param>
        /// <returns></returns>
        public static Boolean EnterLock(this EquipmentAction act,Boolean tryLockAggregateRoot=true)
        {
            if (tryLockAggregateRoot && act != null && act.Movement != null && act.Movement.Task != null)
            {
                return act.Movement.Task.InternalEnterLock(act);
            }
            else
            {
                var key = String.Format("{0}#{1}#{2}", act.GetType().Name, act.Id, act.EquipmentTaskId);

                lock (_lockers)
                {
                    if (_lockers.Any(x => x == key))
                    {
                        _logger.Warn1(String.Format("Enter Lock {0} Failed.", act), typeof(LockerExtentions), act);
                        return false;
                    }

                    _lockers.Add(key);

                    _logger.Debug1(String.Format("Enter Lock {0} Success.", act), typeof(LockerExtentions), act);

                    return true;
                }
            }
        }

        /// <summary>
        /// 清除指定任务的锁信息
        /// </summary>
        /// <param name="tsk">要清除锁信号的任务</param>
        public static void ExitLock(this Task tsk)
        {
            tsk.InternalExitLock(null);
        }

        static void InternalExitLock(this Task tsk,EquipmentAction act)
        {
            var key = String.Format("{0}#{1}", tsk.GetType().Name, tsk.TaskCode);

            lock (_lockers)
            {
                var item = _lockers.FirstOrDefault(x => x == key);
                if (item == null)
                {
                    if (act != null)
                    {
                        _logger.Warn1(String.Format("Exit Lock {0} Failed.", tsk), typeof(LockerExtentions), act);
                    }
                    else
                    {
                        _logger.Warn1(String.Format("Exit Lock {0} Failed.", tsk), typeof(LockerExtentions), tsk);
                    }
                    return;
                }

                _lockers.Remove(item);

                if (act != null)
                {
                    _logger.Debug1(String.Format("Exit Lock {0} Success.", tsk), typeof(LockerExtentions), act);
                }
                else
                {
                    _logger.Debug1(String.Format("Exit Lock {0} Success.", tsk), typeof(LockerExtentions), tsk);
                }
            }
        }

        /// <summary>
        /// 清除指定物理动作的锁信息
        /// </summary>
        /// <param name="act">要清除锁信号的物理动作</param>
        /// <param name="tryExitAggregateRoot">是否尝试清除聚合根锁信息</param>
        public static void ExitLock(this EquipmentAction act, Boolean tryExitAggregateRoot = true)
        {
            if (tryExitAggregateRoot && act != null && act.Movement != null && act.Movement.Task != null)
            {
                act.Movement.Task.InternalExitLock(act);
            }
            else
            {
                var key = String.Format("{0}#{1}#{2}", act.GetType().Name, act.Id, act.EquipmentTaskId);

                lock (_lockers)
                {
                    var item = _lockers.FirstOrDefault(x => x == key);
                    if (item == null)
                    {
                        _logger.Warn1(String.Format("Exit Lock {0} Failed.", act), typeof(LockerExtentions), act);
                        return;
                    }

                    _lockers.Remove(item);

                    _logger.Debug1(String.Format("Exit Lock {0} Success.", act), typeof(LockerExtentions), act);
                }
            }
        }

        /// <summary>
        /// 尝试创建指定key的锁信息。该操作不会执行LOCK操作，只会创建一个Lock对象。
        /// </summary>
        /// <param name="key">要创建锁信息的key</param>
        /// <param name="args">上下文参数</param>
        /// <returns></returns>
        public static Boolean EnterLock(this String key, Object args)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            lock (_lockers)
            {
                if (_lockers.Any(x => x == key))
                {
                    _logger.Warn1(String.Format("Enter Lock key#{0} Failed.", key), typeof(LockerExtentions), args);
                    return false;
                }

                _lockers.Add(key);

                _logger.Debug1(String.Format("Enter Lock key#{0} Success.", key), typeof(LockerExtentions), args);

                return true;
            }
        }

        /// <summary>
        /// 清除指定key的锁信息
        /// </summary>
        /// <param name="key">要清除锁信号的key</param>
        /// <param name="args">上下文参数</param>
        public static void ExitLock(this String key, Object args)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            lock (_lockers)
            {
                var item = _lockers.FirstOrDefault(x => x == key);
                if (item == null)
                {
                    _logger.Warn1(String.Format("Exit Lock key#{0} Failed.", key), typeof(LockerExtentions), args);
                    return;
                }

                _lockers.Remove(item);

                _logger.Debug1(String.Format("Exit Lock key#{0} Success.", key), typeof(LockerExtentions), args);
            }
        }
    }
}
