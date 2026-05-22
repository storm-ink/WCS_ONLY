using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Wcs.Framework
{
    /// <summary>
    /// 锁信息
    /// </summary>
    public class LockerInfo
    {
        #region Fields
        /// <summary>
        /// 表示一个空锁.
        /// </summary>
        public readonly static LockerInfo Empty = new LockerInfo();
        /// <summary>
        /// 表示一个管理员持有的锁.
        /// </summary>
        public readonly static LockerInfo Adminstrator = new LockerInfo("Admin", "Admin");
        #endregion

        #region Properities
        /// <summary>
        /// 持有者
        /// </summary>
        public virtual string UserName { get; set; }
        /// <summary>
        /// 终端ip
        /// </summary>
        public virtual string IPAddress { get; set; }
        /// <summary>
        /// 指示该对象是否为空
        /// </summary>
        public virtual Boolean IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.UserName)
                    && string.IsNullOrWhiteSpace(this.IPAddress);
            }
            protected set
            {
                //仅供 NHibernate 使用
            }
        }

        #endregion

        /// <summary>
        /// 默认构造函数.
        /// </summary>
        protected LockerInfo()
        {

        }
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="userName"> 持有者. </param>
        /// <param name="ip">       终端 ip 地址. </param>
        public LockerInfo(string userName, string ip):this()
        {
            this.IPAddress = ip;
            this.UserName = userName;
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        public static String GetIpAddress()
        {
            string hostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);

            if (ipEntry.AddressList.Length > 1)
            {
                if (ipEntry.AddressList.Any(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    return ipEntry.AddressList
                        .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .ToString();
                }
                else
                {
                    return ipEntry.AddressList.Last().ToString();
                }
            }
            else
            {
                return ipEntry.AddressList[0].ToString();
            }
        }

        public override string ToString()
        {
            return String.Format("{0} 持有的锁", this.UserName);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is LockerInfo))
            {
                return false;
            }

            LockerInfo b = (LockerInfo)obj;

            return string.Equals(this.UserName, b.UserName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(this.IPAddress, b.IPAddress, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return 7 ^ getNotSafeHashCode(UserName) ^ getNotSafeHashCode(IPAddress);
        }

        int getNotSafeHashCode(String v)
        {
            if (v == null)
            {
                return 0;
            }

            return v.GetHashCode();
        }
    }
}
