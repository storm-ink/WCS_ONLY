using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Robot
{
    /// <summary>
    /// Robot上报的Task信息
    /// </summary>
    [DisplayName("ROBOT取放货信号")] 
    public class RoboPickAndPutransferObject
    {
        public RoboPickAndPutransferObject()
        {
           
        }
        public RoboPickAndPutransferObject(byte[] bytes)
        {
            this.RobotToCVSingle1_Pick = convertBytesToBooleanValue(bytes.Take(1).First(), 0);
            this.RobotToCVSingle2_Pick = convertBytesToBooleanValue(bytes.Take(1).First(), 1);
            this.RobotToCVSingle3_Pick = convertBytesToBooleanValue(bytes.Take(1).First(), 2);
            this.RobotToCVSingle4_Pick = convertBytesToBooleanValue(bytes.Take(1).First(), 3);
            this.RobotToCVSingle5_Pick = convertBytesToBooleanValue(bytes.Take(1).First(), 4);
            this.RobotToCVSingle6_Pick = BitConverter.ToUInt16(bytes.Skip(1).Take(2).ToArray(), 0);

            this.CVToRobotSingle1_Pick= convertBytesToBooleanValue (bytes.Skip(3).Take(1).First(), 0);
            this.CVToRobotSingle2_Pick = convertBytesToBooleanValue(bytes.Skip(3).Take(1).First(), 1);
            this.CVToRobotSingle3_Pick = convertBytesToBooleanValue(bytes.Skip(3).Take(1).First(), 2);
            this.CVToRobotSingle4_Pick = convertBytesToBooleanValue(bytes.Skip(3).Take(1).First(), 3);
            this.CVToRobotSingle5_Pick = BitConverter.ToUInt16(bytes.Skip(4).Take(2).ToArray(), 0);



            this.RobotToCVSingle1_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 0);
            this.RobotToCVSingle2_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 1);
            this.RobotToCVSingle3_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 2);
            this.RobotToCVSingle4_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 3);
            this.RobotToCVSingle5_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 4);
            this.RobotToCVSingle6_Put = convertBytesToBooleanValue(bytes.Skip(6).Take(1).First(), 5);
            this.RobotToCVSingle7_Put = BitConverter.ToUInt16(bytes.Skip(7).Take(2).ToArray(), 0);

            this.CVToRobotSingle1_Put = convertBytesToBooleanValue(bytes.Skip(9).Take(1).First(), 0);
            this.CVToRobotSingle2_Put = convertBytesToBooleanValue(bytes.Skip(9).Take(1).First(), 1);
            this.CVToRobotSingle3_Put = convertBytesToBooleanValue(bytes.Skip(9).Take(1).First(), 2);
            this.CVToRobotSingle4_Put = convertBytesToBooleanValue(bytes.Skip(9).Take(1).First(), 3);
            this.CVToRobotSingle5_Put = convertBytesToBooleanValue(bytes.Skip(9).Take(1).First(), 4);
            this.CVToRobotSingle6_Put = BitConverter.ToUInt16(bytes.Skip(10).Take(2).ToArray(), 0);






        }
        /// <summary>
        /// 请求抓取0.0
        /// </summary>
        [DisplayName("请求抓取")]
        public bool RobotToCVSingle1_Pick { get; set; }
        /// <summary>
        /// 抓取完成0.1
        /// </summary>
        [DisplayName("抓取完成")]
        public bool RobotToCVSingle2_Pick { get; set; }
        /// <summary>
        /// 待机标志0.2
        /// </summary>
        [DisplayName("待机标志")]
        public bool RobotToCVSingle3_Pick { get; set; }
        /// <summary>
        /// 干涉区域0.3
        /// </summary>
        [DisplayName("干涉区域")]
        public bool RobotToCVSingle4_Pick { get; set; }
        /// <summary>
        /// 时钟信号0.4
        /// </summary>
        [DisplayName("时钟信号")]
        public bool RobotToCVSingle5_Pick { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        [DisplayName("当前位置")]
        public UInt16 RobotToCVSingle6_Pick { get; set; }
        /// <summary>
        /// 允许抓取
        /// </summary>
        [DisplayName("允许抓取")]
        public bool CVToRobotSingle1_Pick { get; set; }
        /// <summary>
        /// 确认抓取完成
        /// </summary>
        [DisplayName("确认抓取完成")]
        public bool CVToRobotSingle2_Pick { get; set; }
        /// <summary>
        /// 待机标志
        /// </summary>
        [DisplayName("待机标志")]
        public bool CVToRobotSingle3_Pick { get; set; }

        /// <summary>
        /// 时钟信号
        /// </summary>
        [DisplayName("时钟信号")]
        public bool CVToRobotSingle4_Pick { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        [DisplayName("当前位置")]
        public UInt16 CVToRobotSingle5_Pick { get; set; }






        /// <summary>
        /// 请求放货
        /// </summary>
        [DisplayName("请求放货")]
        public bool RobotToCVSingle1_Put { get; set; }
        /// <summary>
        /// 放货完成
        /// </summary>
        [DisplayName("放货完成")]
        public bool RobotToCVSingle2_Put { get; set; }
        /// <summary>
        /// 码垛完成
        /// </summary>
        [DisplayName("码垛完成")]
        public bool RobotToCVSingle3_Put { get; set; }
        /// <summary>
        /// 待机标志
        /// </summary>
        [DisplayName("待机标志")]
        public bool RobotToCVSingle4_Put { get; set; }
        /// <summary>
        /// 干涉区域
        /// </summary>
        [DisplayName("干涉区域")]
        public bool RobotToCVSingle5_Put { get; set; }
        /// <summary>
        /// 时钟信号
        /// </summary>
        [DisplayName("时钟信号")]
        public bool RobotToCVSingle6_Put { get; set; }

        /// <summary>
        /// 当前位置
        /// </summary>
        [DisplayName("当前位置")]
        public UInt16 RobotToCVSingle7_Put { get; set; }
        /// <summary>
        /// 允许放货
        /// </summary>
        [DisplayName("允许放货")]
        public bool CVToRobotSingle1_Put { get; set; }
        /// <summary>
        /// 确认放货完成
        /// </summary>
        [DisplayName("确认放货完成")]
        public bool CVToRobotSingle2_Put { get; set; }
        /// <summary>
        /// 待机标志
        /// </summary>
        [DisplayName("待机标志")]
        public bool CVToRobotSingle3_Put { get; set; }

        /// <summary>
        /// 时钟信号强制出盘
        /// </summary>
        [DisplayName("时钟信号")]
        public bool CVToRobotSingle4_Put { get; set; }

        /// <summary>
        /// 强制出盘
        /// </summary>
        [DisplayName("强制出盘")]
        public bool CVToRobotSingle5_Put { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        [DisplayName("当前位置")]
        public UInt16 CVToRobotSingle6_Put { get; set; }


        private bool convertBytesToBooleanValue(byte b, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7)
            {
                throw new ArgumentOutOfRangeException("bitIndex:大于等于 0,小于等于 7");
            }         
            var shifted = b >> bitIndex;

            return shifted % 2 != 0;
        }

    }
}