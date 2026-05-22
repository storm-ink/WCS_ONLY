using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
   public class t_interface_history
    {
        /// <summary>
        ///  CVCS历史任务记录
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public string intercode { get; set; }
        public string begionloc { get; set; }
        public string begionloc_level { get; set; }
        public string endloc { get; set; }
        public string endloc_level { get; set; }
        public string intertype { get; set; }
        public string state { get; set; }
        public string createdatetime { get; set; }
        public string usedatetime { get; set; }
        public string finishdatetime { get; set; }
        public string remark { get; set; }
        public string grade { get; set; }
        public string category { get; set; }
        public string sort { get; set; }
        public string agvId { get; set; }
        public string salverType { get; set; }
        public string traycode { get; set; }
        public string additionalAttr { get; set; }
        public string taskCode { get; set; }
        public string currentLocation { get; set; }
    }
}
