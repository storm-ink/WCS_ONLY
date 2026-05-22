using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace BOE
{
    public class TaskChooseEndLocation : TaskChooseEndLocationHandler
    {
        public override bool Hand(Task task, Location currentLocation)
        {
            using (WMSServices.WmsServices.WcfWmsServiceForWcsClient client = new WMSServices.WmsServices.WcfWmsServiceForWcsClient())
            {
                try
                {
                    var loc = new WMSServices.WmsServices.WcsRequestCurrentLocation();
                    loc.TaskCode = task.TaskCode;
                    loc.TaskType = task.TaskType;
                    loc.LocationUserCode = task.StartLocation.UserCode;
                    loc.CurrentLocationLocationUserCode = task.CurrentLocation.UserCode;
                    loc.ContainerCodes = task.ContainerCodes.ToArray();

                    var result = client.TaskErrorAndRequestNewEndPoint(loc);

                    client.Close();

                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                               new Wcs.Framework.MessageBoard.Messages.TipMessage(
                                   Wcs.Framework.MessageBoard.MessageLevel.Info, "任务终点调整", String.Format("任务 {0} 终点调整接口调用成功，接口返回:成功（{1}）消息（{2}）", task, result.IsSuccessful, result.Msg), ""
                                   ));
                    return result.IsSuccessful;
                }
                catch (Exception ex)
                {
                    _logger.Error1(ex, this);

                    Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                        new Wcs.Framework.MessageBoard.Messages.ProduceMessage(
                            Wcs.Framework.MessageBoard.MessageLevel.Error,
                            "任务终点调整", 
                            String.Format("任务{0}尝试调试重点是失败，异常信息：{1}", 
                            task, 
                            ex.Message),
                            null, null));

                    if (client != null)
                        client.Abort();
                    return false;
                }
            }
        }
    }
}
