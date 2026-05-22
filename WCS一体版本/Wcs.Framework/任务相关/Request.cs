using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.Devices;
using Wcs.Framework;

namespace Wcs.Framework
{
    /// <summary>
    /// 占位请求
    /// </summary>
    public class Request : Comparable<Request>, IComparer<Request>
    {
        public Request()
        {
            ContainerCodes = new Iesi.Collections.Generic.HashedSet<String>();
            CreatedAt = DateTime.Now;
            this.Status = RequestStatus.New;
            this.HoldSignalInfo = new HoldSignalInfo();
            this.AdditionalInfo = new Dictionary<String, String>();
        }
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public virtual LocationInfo Source { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual RequestStatus Status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 失效时间
        /// </summary>
        public virtual DateTime? InvalidAt { get; set; }
        /// <summary>
        /// 容器集合
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<String> ContainerCodes { get; protected set; }
        /// <summary>
        /// 请求原始数据
        /// </summary>
        public virtual HoldSignalInfo HoldSignalInfo { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public virtual String RequestType { get; set; }
        /// <summary>
        /// 备注信息.
        /// </summary>
        public virtual String Comments { get; set; }
        /// <summary>
        /// 附加属性集合
        /// </summary>
        public virtual IDictionary<String, String> AdditionalInfo { get; set; }
        /// <summary>
        /// 在通知执行后需要更新为已处理.
        /// </summary>
        public virtual Boolean ProcessedAfterNotify { get; set; }

        public virtual int Compare(Request x, Request y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.Id != y.Id
             || x.CreatedAt != y.CreatedAt
             || x.Source.Compare(x.Source, y.Source) != 0
             || x.InvalidAt != y.InvalidAt
             || x.Status != y.Status
                )
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            if (this.ContainerCodes.Count > 0)
            {
                return String.Format("{0}上的占位#{1}，容器编码：{2}", this.Source.UserCode, this.Id, this.ContainerCodes);
            }
            else
            {
                return String.Format("{0}上的占位#{1}", this.Source.UserCode, this.Id);
            }
        }
    }
}
