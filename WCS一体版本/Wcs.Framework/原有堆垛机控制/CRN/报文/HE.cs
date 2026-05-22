
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>紧急停机</summary>
    [Serializable]
    [DataContract]
    public sealed class HE : RequestTelex
    {
        private HE()
        {
            this.Head = "HE";
        }

        private static HE he = new HE();
        /// <summary>紧急停机实例</summary>
        public static HE Instance
        {
            get
            {
                return he;
            }
        }
    }
}
