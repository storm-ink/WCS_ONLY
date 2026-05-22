using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    public class BasicEquipmentActionSequenceSchedulerActionFilter : EquipmentActionSequenceSchedulerActionFilter
    {
        public override bool CanSend(EquipmentActionSequenceScheduler equipmentActionSequenceScheduler,EquipmentAction action)
        {
#warning 此处应该考虑去掉，完全是为了后期纠错。
            if (equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction != null 
                && (equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction.Status == EquipmentActionStatus.Completed
                    || equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction.Status == EquipmentActionStatus.Cancelled
                ))
            {
                this.Logger.Warn1(string.Format("{0} 已处于 {1} 状态，但仍然存在于 {2}。将自动移除。",
                    equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction,
                    equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction.Status.GetDescription(),
                    equipmentActionSequenceScheduler), this, action);

                equipmentActionSequenceScheduler.EquipmentActionSequence.Pop(action);
                return false;
            }

            if (!equipmentActionSequenceScheduler.Device.IsConnected)
            {
                return false;
            }

            if (!equipmentActionSequenceScheduler.Device.IsIdle)
            {
                return false;
            }

            if (equipmentActionSequenceScheduler.Device.AllowConcurrency
                && equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction!=null
                && equipmentActionSequenceScheduler.EquipmentActionSequence.CurrentEquipmentAction.EquipmentTaskId!=action.EquipmentTaskId
                )
            {
                return false;
            }

            if (!equipmentActionSequenceScheduler.Device.AllowConcurrency 
                && equipmentActionSequenceScheduler
                .EquipmentActionSequence
                .Actions
                .Any(x => x.Status == EquipmentActionStatus.Error 
                    || x.Status == EquipmentActionStatus.Executing 
                    || x.Status == EquipmentActionStatus.Suspend))
            {
                return false;
            }

            if (!action.CanPerform())
            {
                return false;
            }

            return true;
        }
    }
}
