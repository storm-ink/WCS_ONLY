
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>查询堆垛机状态</summary>
    [Serializable]
    [DataContract]
    public sealed class HA : RequestTelex
    {
        private HA()
        {
            this.Head = "HA";
        }

        private static HA ha = new HA();
        /// <summary>查询堆垛机状态实例</summary>
        public static HA Instance
        {
            get
            {
                return ha;
            }
        }
    }
}
