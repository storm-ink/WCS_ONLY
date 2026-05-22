using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.Client
{
    public class HistoryTaskQueryCriteria
    {
        public HistoryTaskQueryCriteria() { }
        public string TaskCode { get; set; }
        public string ContainerCodes { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string TaskType { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Total { get; set; }
        public int ActionId { get; set; }

        public bool? FromWMS { get; set; }

    }

    public class TaskInformationQueryCriteria
    {
        public int TaskId { get; set; }

        public int MovementId { get; set; }

        public int ActionId { get; set; }

        public string CurrentUserCode { get; set; }
    }
    public class ChangeTaskPriorityInfo
    {
        public int TaskId { get; set; }

        public string TaskCode { get; set; }

        public int Priority { get; set; }
    }

    public class ClientRouteHead
    {
        public int RouteId { get; set; }
        public int RouteNo { get; set; }
        public string DeviceName { get; set; }
        public bool AllowAllowStartFromMidway { get; set; }
        public List<WcsLocation> Items { get; set; }
    }
}
