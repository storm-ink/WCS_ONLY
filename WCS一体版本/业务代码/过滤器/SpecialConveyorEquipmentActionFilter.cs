using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using ZHQXC.WebAPI;

namespace ZHQXC
{
    /// <summary>
    /// 专机任务是否处理完成，影响输送线下一段子任务是否要开始执行
    /// </summary>  
    public class SpecialConveyorEquipmentActionFilter : EquipmentActionSchedulerFilter
    {
        string[] SpecialConveyor = { "2010","2011","2013", "2014", "2015", "2016", "2017", "2018"};
        public override ActionSchedulerFilterResult Filter(EquipmentActionScheduler equipmentActionScheduler, EquipmentAction action)
        {
            if (!(equipmentActionScheduler.Device is ConveyorDevice))
                return new ActionSchedulerFilterResult(false, "非输送线任务");
            var start = LocationConverter.ToLocation(action.Movement.StartLocation);
            if (!SpecialConveyor.Contains($"{start}") )
            {
                return new ActionSchedulerFilterResult(false, "非专机处理任务");
            }
            ///读取专机PLC信号，是否完成操作
            ///未完成情况下，过滤器过滤任务，下发至设备
            //var isDown = ReqeusSpecialPLCHelper.SpecialResultCheck(start.UserCode);
            //var isDown = true;

            //if (!isDown)
            //{
            //    return new ActionSchedulerFilterResult(true, "专机未完成处理");
            //}
            return new ActionSchedulerFilterResult(false, "");
        }
    }
}
