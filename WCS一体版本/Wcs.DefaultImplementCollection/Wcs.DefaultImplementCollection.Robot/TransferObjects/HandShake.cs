namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// 机械手握手信号
    /// </summary>
    public enum HandShake
    {
        /// <summary>
        /// 未知-无效值
        /// </summary>
        Unknown=0,
        /// <summary>
        /// 新任务
        /// </summary>
        New = 1,
        /// <summary>
        /// 删除请求
        /// </summary>
        Delete = 2,
        /// <summary>
        /// 设备已读
        /// </summary>
        Readed=3,
        /// <summary>
        /// 暂停
        /// </summary>
        Suspend=4,
        /// <summary>
        /// 继续拆垛
        /// </summary>
        Resum =5,
        /// <summary>
        /// 请求清空/完成
        /// </summary>
        Clear = 6,
        /// <summary>
        /// 请求清空/任务中止
        /// </summary>
        Abort = 7,
        /// <summary>
        /// 请求清空/FOSBID校对失败
        /// </summary>
        FosbDAbort = 8
    }
}