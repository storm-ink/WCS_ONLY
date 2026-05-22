
using System;
using System.Runtime.Serialization;

namespace Wcs.Framework.CraneControl
{
    /// <summary>回原点</summary>
    [Serializable]
    [DataContract]
    public sealed class HP : RequestTelex
    {
        private HP()
        {
            this.Head = "HP";
        }

        private static HP hp = new HP();
        /// <summary>回原点实例</summary>
        public static HP Instance
        {
            get
            {
                return hp;
            }
        }
    }
}
