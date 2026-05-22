using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class SinevaAGVTaskCompeltedHandler : ITaskEventHandler<TaskCompletedEventArgs>
    {
        Logger _logger = LogManager.CreateNullLogger();

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is SinevaAGVDevice))
                {
                    args.Handled = true;
                    return;
                }

                SinevaAGVDevice _device = (SinevaAGVDevice)device;
                SinevaAGVDatabaseHand.UpdateSynchro(_device._完成处理中的任务[args.EquipmentTaskId], EquipmentTaskStatus.已完成, Synchro.WCS处理完成);
                _device._完成处理中的任务.Remove(args.EquipmentTaskId);

                var _action = session.Query<SinevaAGVSubSystemAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (_action != null)
                    SinevaAGVDatabaseHand.DeleteCompeletedTask(_action.SendAGVTaskId);

                args.Handled = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, this);
            }
        }
    }
}
