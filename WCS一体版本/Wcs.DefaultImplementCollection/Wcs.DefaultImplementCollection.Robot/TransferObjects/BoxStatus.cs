namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Box状态（0-初始化，1-未完成，2-自动完成，3-手动完成，4-异常，5-执行中，6-WCS完成）
    /// </summary>
    public enum BoxStatus
    {
        初始化 = 0,
        未完成 = 1,
        自动完成 = 2,
        手动完成 = 3,
        异常 = 4,
        执行中 = 5,
        WCS完成 = 6
    }
}