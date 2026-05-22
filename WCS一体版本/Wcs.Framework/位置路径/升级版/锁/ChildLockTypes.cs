namespace Wcs.Framework.Lock
{
    /// <summary>
    /// 二级锁类型
    /// </summary>
    public enum ChildLockTypes
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// 类型排他锁
        /// </summary>
        TypeExclusiveLock,
        /// <summary>
        /// 内部排他锁
        /// </summary>
        InnerExclusiveLock
    }
}