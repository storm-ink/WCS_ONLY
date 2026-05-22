using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImplementCollection.RailGuidedVehicle;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 表示一个具体的状态
    /// </summary>
    public abstract class AbstractState2
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public Random _random = new Random();

        public StateManagerRestoreInfo _context;
        public EquipmentAction _action;
        public RailGuidedVehicleDevice railGuidedVehicleDevice;
        public SingleLocationDoubleVehicleSubSystemLocation destinationLocation;
        public bool _giveAwayMark;
        public SingleLocationDoubleVehicleSubSystemEquipmentActionScheduler2 _scheduler;
        public string lastSendTaskId = "";

        /// <summary>
        /// 状态名称
        /// </summary>
        public abstract String Name { get; }
        /// <summary>
        /// 指示当前的状态是否可以执行
        /// </summary>
        /// <returns></returns>
        public abstract CanPerformResult CanPerform();

        /// <summary>
        /// 指示当前的状态是否已完成
        /// </summary>
        /// <returns></returns>
        public abstract IsCompeltedResult IsCompleted();
        /// <summary>
        /// 立即执行该状态操作
        /// </summary>
        /// <param name="context">状态上下文</param>
        public abstract void Perform();
        public override string ToString()
        {
            if (String.IsNullOrWhiteSpace(Name))
                return this.GetType().Name;
            else
                return $"AbsoluteState#actionId:{0}_stateName:{Name}_device:{railGuidedVehicleDevice.Name}_destination:{destinationLocation.UserCode}_lastSendTaskId:{lastSendTaskId}";
        }
    }
}
