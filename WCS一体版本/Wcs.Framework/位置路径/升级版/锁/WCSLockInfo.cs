namespace Wcs.Framework.Lock
{
    /// <summary>
    /// WCS物理动作锁信息
    /// </summary>
    public class WCSLockInfo
    {
        /// <summary>
        /// 一级锁
        /// </summary>
        public ParentLockTypes ParentLockType { get; set; }
        /// <summary>
        /// 二级锁
        /// </summary>
        public ChildLockTypes ChildLockType { get; set; }
    }
}