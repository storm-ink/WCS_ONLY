
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>取消停机</summary>
    [Serializable]
    [DataContract]
    public sealed class HC : RequestTelex
    {
        private HC()
        {
            this.Head = "HC";
        }

        private static HC hc = new HC();
        /// <summary>取消急停实例</summary>
        public static HC Instance
        {
            get
            {
                return hc;
            }
        }
    }
}
