using Newtonsoft.Json;
using ZHQXC.WebAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Wcs.Framework;
using Wcs;
using NHibernate.Linq;
using ZHQXC.AlarmPool;

namespace ZHQXC.WebAPI
{
    public class MessageController : ApiController
    {
        [HttpGet]
        public object Page()
        {
            ResponseDto responseDto = new ResponseDto();
            try
            {
                int index = 0;
                responseDto.Success = true;
                responseDto.Status = 0;
                responseDto.Error = "操作成功";
                List<MessageDto> messageDtos = new List<MessageDto>();
                var warnings = BuildWarningImage();
                if(warnings != null)
                {
                    foreach (AlarmRecord warning in warnings)
                    {
                        var type = warning.DeviceType;
                        index = index + 1;
                        TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks - warning.BeginingAt.Ticks);
                        string warningValue;
                        if (warning.Device != warning.OwnerDevice) 
                            warningValue = $" {warning.Device}@{warning.OwnerDevice}#{type}#{warning.AlarmName}:持续{System.Math.Abs(timeSpan.TotalMinutes).ToString("0.0") ?? ""}分钟，请尽快处理";
                        else
                            warningValue = $" {warning.Device}#{type}#{warning.AlarmName}:持续{System.Math.Abs(timeSpan.TotalMinutes).ToString("0.0") ?? ""}分钟，请尽快处理";

                        messageDtos.Add(new MessageDto() { Index = index, Text = warningValue });
                    }
                    responseDto.Data = JsonConvert.SerializeObject(messageDtos);
                }
               
                return responseDto;

            }
            catch (Exception m)
            {

                responseDto.Error = $"{m.Message}";
                responseDto.Success = false;
                responseDto.Status = 1;
                return responseDto;
            }
        }


        private List<dynamic> BuildWarningImage()
        {
            try
            {
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
                {
                    var warnings = unitOfWork.session.Query<AlarmPool.AlarmRecord>().Where(x => x.EndingAt == null).OrderBy(x => x.Id);
                    return warnings.Cast<dynamic>().ToList();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
