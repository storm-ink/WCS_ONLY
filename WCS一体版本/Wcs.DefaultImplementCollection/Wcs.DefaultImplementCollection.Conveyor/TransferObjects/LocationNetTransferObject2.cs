using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;
using Newtonsoft.Json;


namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 输送线货位状态对象.
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.LocationNetTransferObject, Wcs.DefaultImpls" blockBytes="4" itemCount="1">
    ///     <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    ///     <property name="Status" index="2" size="2" type="UInt16" />
    ///   </collection>
    /// </summary>
    [Description("输送线货位状态")]
    [JsonObject]
    public class LocationNetTransferObject2 : NetTransferObject
    {
        /// <summary>
        /// 货位号.
        /// </summary>
        /// <value>
        /// 位置在设备中的编码形式.
        /// </value>
        [Description("货位号")]
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 状态.
        /// </summary>
        [Description("状态")]
        public LocationNetTransferObjectStatus Status { get; set; }

        /// <summary>
        /// 前保护
        /// </summary>
        [Description("前保护")]
        public bool 前保护 { get; set; }
        /// <summary>
        /// 前到位
        /// </summary>
        [Description("前到位")]
        public bool 前到位 { get; set; }
        /// <summary>
        /// 前减速
        /// </summary>
        [Description("前减速")]
        public bool 前减速 { get; set; }
        /// <summary>
        /// 后保护
        /// </summary>
        [Description("后保护")]
        public bool 后保护 { get; set; }
        /// <summary>
        /// 后到位
        /// </summary>
        [Description("后到位")]
        public bool 后到位 { get; set; }
        /// <summary>
        /// 后减速
        /// </summary>
        [Description("后减速")]
        public bool 后减速 { get; set; }
        /// <summary>
        /// 高位
        /// </summary>
        [Description("高位")]
        public bool 高位 { get; set; }
        /// <summary>
        /// 低位.
        /// </summary>
        [Description("低位")]
        public bool 低位 { get; set; }
        /// <summary>
        /// 高减速
        /// </summary>
        [Description("高减速")]
        public bool 高减速 { get; set; }
        /// <summary>
        /// 低减速.
        /// </summary>
        [Description("低减速")]
        public bool 低减速 { get; set; }
        /// <summary>
        /// 载荷.
        /// </summary>
        [Description("载荷光电")]
        public bool 载荷光电 { get; set; }
        /// <summary>
        /// 预留1.
        /// </summary>
        [Description("预留1")]
        public bool 预留1 { get; set; }
        /// <summary>
        /// 预留2.
        /// </summary>
        [Description("预留2")]
        public bool 预留2 { get; set; }
        /// <summary>
        /// 预留3.
        /// </summary>
        [Description("预留3")]
        public bool 预留3 { get; set; }
        /// <summary>
        /// 预留4.
        /// </summary>
        [Description("预留4")]
        public bool 预留4 { get; set; }

        /// <summary>
        /// 手动
        /// </summary>
        [Description("手动")]
        public bool 手动 { get; set; }
        // <summary>
        /// 光电异常
        /// </summary>
        [Description("光电异常")]
        public bool 光电异常 { get; set; }
        // <summary>
        /// 运行超时
        /// </summary>
        [Description("运行超时")]
        public bool 运行超时 { get; set; }
        // <summary>
        /// 占位超时
        /// </summary>
        [Description("占位超时")]
        public bool 占位超时 { get; set; }
        // <summary>
        /// 有任务无货
        /// </summary>
        [Description("有任务无货")]
        public bool 有任务无货 { get; set; }
        // <summary>
        /// 残留任务故障
        /// </summary>
        [Description("残留任务故障")]
        public bool 残留任务故障 { get; set; }
        // <summary>
        ///  XY断路器
        /// </summary>
        [Description(" XY断路器")]
        public bool XY断路器 { get; set; }
        // <summary>
        ///  XY变频器
        /// </summary>
        [Description("XY变频器")]
        public bool XY变频器 { get; set; }
        // <summary>
        /// XY接触器
        /// </summary>
        [Description("XY接触器")]
        public bool XY接触器 { get; set; }
        // <summary>
        /// Lift断路器
        /// </summary>
        [Description("Lift断路器")]
        public bool Lift断路器 { get; set; }
        // <summary>
        /// Lift变频器
        /// </summary>
        [Description("Lift变频器")]
        public bool Lift变频器 { get; set; }
        // <summary>
        /// Lift接触器
        /// </summary>
        [Description("Lift接触器")]
        public bool Lift接触器 { get; set; }
        // <summary>
        /// 预留5
        /// </summary>
        [Description("预留5")]
        public bool 预留5 { get; set; }
        // <summary>
        /// 预留5
        /// </summary>
        [Description("预留6")]
        public bool 预留6 { get; set; }
        // <summary>
        /// Estop
        /// </summary>
        [Description("Estop")]
        public bool Estop { get; set; }


        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "Status":
                        return this.Status;
                    case "前保护":
                        return this.前保护;
                    case "前到位":
                        return this.前到位;
                    case "前减速":
                        return this.前减速;
                    case "后保护":
                        return this.后保护;
                    case "后到位":
                        return this.后到位;
                    case "后减速":
                        return this.后减速;
                    case "高位":
                        return this.高位;
                    case "低位":
                        return this.低位;
                    case "高减速":
                        return this.高减速;
                    case "低减速":
                        return this.低减速;
                    case "载荷光电":
                        return this.载荷光电;
                    case "预留1":
                        return this.预留1;
                    case "预留2":
                        return this.预留2;
                    case "预留3":
                        return this.预留3;
                    case "预留4":
                        return this.预留4;
                    case "手动":
                        return this.手动;
                    case "光电异常":
                        return this.光电异常;
                    case "运行超时":
                        return this.运行超时;
                    case "占位超时":
                        return this.占位超时;
                    case "有任务无货":
                        return this.有任务无货;
                    case "残留任务故障":
                        return this.残留任务故障;
                    case "XY断路器":
                        return this.XY断路器;
                    case "XY变频器":
                        return this.XY变频器;
                    case "XY接触器":
                        return this.XY接触器;
                    case "Lift断路器":
                        return this.Lift断路器;
                    case "Lift变频器":
                        return this.Lift变频器;
                    case "Lift接触器":
                        return this.Lift接触器;
                    case "预留5":
                        return this.预留5;
                    case "预留6":
                        return this.预留6;
                    case "Estop":
                        return this.Estop;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "Status":
                        this.Status = (LocationNetTransferObjectStatus)Convert.ToInt16(value);
                        break;
                    case "前保护":
                        this.前保护 = Convert.ToBoolean(value);
                        break;
                    case "前到位":
                        this.前到位 = Convert.ToBoolean(value);
                        break;
                    case "前减速":
                        this.前减速 = Convert.ToBoolean(value);
                        break;
                    case "后保护":
                        this.后保护 = Convert.ToBoolean(value);
                        break;
                    case "后到位":
                        this.后到位 = Convert.ToBoolean(value);
                        break;
                    case "后减速":
                        this.后减速 = Convert.ToBoolean(value);
                        break;
                    case "高位":
                        this.高位 = Convert.ToBoolean(value);
                        break;
                    case "低位":
                        this.低位 = Convert.ToBoolean(value);
                        break;
                    case "高减速":
                        this.高减速 = Convert.ToBoolean(value);
                        break;
                    case "低减速":
                        this.低减速 = Convert.ToBoolean(value);
                        break;
                    case "载荷光电":
                        this.载荷光电 = Convert.ToBoolean(value);
                        break;
                    case "预留1":
                        this.预留1 = Convert.ToBoolean(value);
                        break;
                    case "预留2":
                        this.预留2 = Convert.ToBoolean(value);
                        break;
                    case "预留3":
                        this.预留3 = Convert.ToBoolean(value);
                        break;
                    case "预留4":
                        this.预留4 = Convert.ToBoolean(value);
                        break;
                    case "手动":
                        this.手动 = Convert.ToBoolean(value);
                        break;
                    case "光电异常":
                        this.光电异常 = Convert.ToBoolean(value);
                        break;
                    case "运行超时":
                        this.运行超时 = Convert.ToBoolean(value);
                        break;
                    case "占位超时":
                        this.占位超时 = Convert.ToBoolean(value);
                        break;
                    case "有任务无货":
                        this.有任务无货 = Convert.ToBoolean(value);
                        break;
                    case "残留任务故障":
                        this.残留任务故障 = Convert.ToBoolean(value);
                        break;
                    case "XY断路器":
                        this.XY断路器 = Convert.ToBoolean(value);
                        break;
                    case "XY变频器":
                        this.XY变频器 = Convert.ToBoolean(value);
                        break;
                    case "XY接触器":
                        this.XY接触器 = Convert.ToBoolean(value);
                        break;
                    case "Lift断路器":
                        this.Lift断路器 = Convert.ToBoolean(value);
                        break;
                    case "Lift接触器":
                        this.Lift接触器 = Convert.ToBoolean(value);
                        break;
                    case "Lift变频器":
                        this.Lift变频器 = Convert.ToBoolean(value);
                        break;
                    case "预留5":
                        this.预留5 = Convert.ToBoolean(value);
                        break;
                    case "预留6":
                        this.预留6 = Convert.ToBoolean(value);
                        break;
                    case "Estop":
                        this.Estop = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public string[] GetAlarms()
        {
            List<string> list = new List<string>();
            if (this.手动) list.Add("手动");
            if (this.光电异常) list.Add("光电异常");
            if (this.运行超时) list.Add("运行超时");
            if (this.占位超时) list.Add("占位超时");
            if (this.有任务无货) list.Add("有任务无货");
            if (this.残留任务故障) list.Add("残留任务故障");
            if (this.XY断路器) list.Add("XY断路器");
            if (this.XY变频器) list.Add("XY变频器");
            if (this.XY接触器) list.Add("XY接触器");
            if (this.Lift断路器) list.Add("Lift断路器");
            if (this.Lift变频器) list.Add("Lift变频器");
            if (this.Lift接触器) list.Add("Lift接触器");
            if (this.预留5) list.Add("预留5");
            if (this.预留6) list.Add("预留6");
            if (this.Estop) list.Add("Estop");
            return list.ToArray();
        }

        public override ReceivedDataLog ToLogData(Device device)
        {
            return null;
        }
    }
}
