using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs
{
    /// <summary>
    /// String 扩展
    /// </summary>
    public static class StringExtentions
    {
        /// <summary>
        /// 获取当前字符串的ASCII长度，按汉字算两个字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Int32 GetASCIILength(this String source)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(source);
            Int32 result = bytes.Sum(x => x == 63 ? 2 : 1);

            return result;
        }
        /// <summary>
        /// 获取指定字符串中宽字符的数量
        /// </summary>
        /// <param name="source">源</param>
        /// <returns></returns>
        public static Int32 GetWideCharacterCount(this String source)
        {
            return System.Text.RegularExpressions.Regex.Matches(source, @"[\u4e00-\u9fa5]").Count;
        }
        /// <summary>
        /// 将指定的字符串用字 ASSIC 字符在右边填充到指定长度
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="totalWidth">字符串总长度</param>
        /// <param name="paddingChar">要填充的 ASSIC 字符</param>
        /// <returns></returns>
        public static String ASSICPadRight(this String source, Int32 totalWidth, Char paddingChar)
        {
            return source.PadRight(totalWidth - source.GetWideCharacterCount(), paddingChar);
        }
        /// <summary>
        /// 将指定的字符串用字 ASSIC 字符在左边填充到指定长度
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="totalWidth">字符串总长度</param>
        /// <param name="paddingChar">要填充的 ASSIC 字符</param>
        /// <returns></returns>
        public static String ASSICPadLeft(this String source, Int32 totalWidth, Char paddingChar)
        {
            return source.PadLeft(totalWidth - source.GetWideCharacterCount(), paddingChar);
        }
    }
}
