
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace ZHQXC
{
    public class BarcodeInfomationDataObject : NetTransferObject
    {
        public String Barcode { get; set; }
        public UInt16 DataID { get; set; }



        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Barcode":
                        return this.Barcode;
                    case "DataID":
                        return this.DataID;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                  
                    case "Barcode":
                        this.Barcode = TrimGarbage((value ?? string.Empty).ToString());
                        break;
                    case "DataID":
                        this.DataID = Convert.ToUInt16(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        //static string TrimGarbage(string src)
        //{
        //    if (string.IsNullOrEmpty(src)) return src;

        //    int start = 0;
        //    // 1. 找到第一个非 \0 字符
        //    while (start < src.Length && src[start] == '\0') start++;

        //    if (start == src.Length) return string.Empty; // 全是 \0

        //    int end = src.IndexOf('\0', start);           // 2. 再找后续第一个 \0
         /* 2. 再去可能的前导 ('  */
        //if (cleaned.StartsWith("('"))
        //    cleaned = cleaned.Substring(2);
        //    return end < 0 ? src.Substring(start)        // 右边没有 \0，直接取到尾
        //                   : src.Substring(start, end - start);
        //}
        private static readonly char[] KeepChars = { '-' };   // 额外允许的特殊符号

        private static string TrimGarbage(string src)
        {
            if (string.IsNullOrEmpty(src)) return src;

            var sb = new System.Text.StringBuilder(src.Length);
            foreach (char c in src)
            {
                if (char.IsLetterOrDigit(c) || KeepChars.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }


}
