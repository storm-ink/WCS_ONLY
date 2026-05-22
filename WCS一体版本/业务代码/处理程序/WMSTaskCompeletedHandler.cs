using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;

namespace BOE
{
    public class WMSTaskCompeletedHandler : WMSTaskCompeletedExternalHandler
    {
        public override void Hand(ref Task task)
        {
            var posNo = task.EndLocation.DeviceCode;
            if (task.EndLocation.UserCode == "00-001-1014"
                || task.EndLocation.UserCode == "00-001-1018"
                || task.EndLocation.UserCode == "00-001-1021"
                || task.EndLocation.UserCode == "00-001-1026"
                || task.EndLocation.UserCode == "00-001-2016"
                || task.EndLocation.UserCode == "00-001-2019")
            {
                ConveyorDevice _conveyor = DeviceConverter.ToDevice<ConveyorDevice>("输送线");
                ShapeCheckTransferObject shapeCheck = _conveyor.ReadStatus<ShapeCheckTransferObject>().FirstOrDefault(x => x.ShapeCheckNO == Convert.ToUInt16(posNo));
                if (shapeCheck == null)
                {
                    if (task.FinishedAt != null)
                    {
                        var _timeSpan = DateTime.Now.Subtract((DateTime)task.FinishedAt).TotalMilliseconds;
                        if (_timeSpan > Convert.ToInt32(Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["外形检测超时设置"]))
                        {
                            string msg = String.Format("外形检测获取超时(设置时间{0})", Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["外形检测超时设置"]);
                            if (!task.AdditionalInfo.ContainsKey(msg))
                                task.AdditionalInfo.Add(msg, "");
                            if (!task.AdditionalInfo.ContainsKey("外形检测"))
                                task.AdditionalInfo.Add("外形检测", "失败");
                            if (!task.AdditionalInfo.ContainsKey("外形检测获取超时"))
                                task.AdditionalInfo.Add("外形检测获取超时", "");
                        }
                        else
                            throw new Exception("等待获取外形检测结果");
                    }
                    else
                        if (!task.AdditionalInfo.ContainsKey("任务完成时间为NULL"))
                        task.AdditionalInfo.Add("任务完成时间为NULL", "");
                    if (!task.AdditionalInfo.ContainsKey("外形检测"))
                        task.AdditionalInfo.Add("外形检测", "失败");
                }
                else
                {
                    Dictionary<string, string> result;
                    Helper.外形检测结果(_conveyor, Convert.ToUInt16(posNo), out result);

                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            if (!task.AdditionalInfo.ContainsKey(item.Key))
                                task.AdditionalInfo.Add(item.Key, item.Value);
                            else
                                task.AdditionalInfo[item.Key] = item.Value;
                        }
                    }
                }
            }
        }
    }
}
