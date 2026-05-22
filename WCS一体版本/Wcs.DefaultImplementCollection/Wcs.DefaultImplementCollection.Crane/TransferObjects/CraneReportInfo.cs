using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.Crane
{
    public class CraneReportInfo : TelexTransferObject
    {
		/// <summary> 
		/// 设备编号 
		/// </summary> 
		public virtual UInt16 DeviceNo { get; set; }
		/// <summary> 
		/// 工作模式 
		/// </summary> 
		public virtual CraneWorkModels CraneWorkModel { get; set; }
		/// <summary> 
		/// 任务号 
		/// </summary> 
		public virtual UInt32 EquipmentTaskId { get; set; }
		/// <summary> 
		/// 任务状态 
		/// </summary> 
		public virtual CraneTaskStatus TaskState { get; set; }
		/// <summary> 
		/// 设备状态 
		/// </summary> 
		public virtual CraneStatus DeviceState { get; set; }
		/// <summary> 
		/// 是否有货  
		/// 0-未知，1-有货，2-无货
		/// </summary> 
		public virtual UInt16 IsLoaded { get; set; }
		/// <summary> 
		/// 行走运行方向 
		/// 0-未知，1-停止，2-前进，3-后退
		/// </summary> 
		public virtual UInt16 XRunningDirection { get; set; }
		/// <summary> 
		/// 行走速度 
		/// </summary> 
		public virtual UInt32 XSpeed { get; set; }
		/// <summary> 
		/// 列停准 
		/// 0-未停准，1-已停准
		/// </summary> 
		public virtual UInt16 XStopped { get; set; }
		/// <summary> 
		/// 当前列 
		/// </summary> 
		public virtual UInt16 XColumn { get; set; }
		/// <summary> 
		/// 目标列 
		/// </summary> 
		public virtual UInt16 XTargetColumn { get; set; }
		/// <summary> 
		/// 行走方向实时条码值 
		/// </summary> 
		public virtual UInt32 XPosition { get; set; }
		/// <summary> 
		/// 行走方向目标条码值 
		/// </summary> 
		public virtual UInt32 XTargetPosition { get; set; }
		/// <summary> 
		/// 升降运行方向 
		/// 0-未知，1-停止，2-上升，3-下降，4-取货，5-放货
		/// </summary> 
		public virtual UInt16 YRunningDirection { get; set; }
		/// <summary> 
		/// 升降速度 
		/// </summary> 
		public virtual UInt32 YSpeed { get; set; }
		/// <summary> 
		/// 层停准 
		/// 0-未停准，1-已停准
		/// </summary> 
		public virtual UInt16 YStopped { get; set; }
		/// <summary> 
		/// 当前层 
		/// </summary> 
		public virtual UInt16 YLevel { get; set; }
		/// <summary> 
		/// 载货台位置 
		/// 0-未知，1-低位，2-高位，3-扫码位 一般来说空载默认在低位，满载默认在高位
		/// </summary> 
		public virtual UInt16 YLevelPosition { get; set; }
		/// <summary> 
		/// 目标层 
		/// </summary> 
		public virtual UInt16 YTargetLevel { get; set; }
		/// <summary> 
		/// 升降方向实时条码值 
		/// </summary> 
		public virtual UInt32 YPosition { get; set; }
		/// <summary> 
		/// 升降方向目标条码值 
		/// </summary> 
		public virtual UInt32 YTargetPosition { get; set; }
		/// <summary> 
		/// 货叉运行方向 
		/// 0-未知，1-停止，2-左伸叉，3-左回收，4-右伸叉，5-右回收
		/// </summary> 
		public virtual UInt16 ZRunningDirection { get; set; }
		/// <summary> 
		/// 货叉速度 
		/// </summary> 
		public virtual UInt32 ZSpeed { get; set; }
		/// <summary> 
		/// 当前排 
		/// 0-未知，1-左，2-右，3-左外，4-右外，10-中位
		/// </summary> 
		public virtual UInt16 ZRow { get; set; }
		/// <summary> 
		/// 目标排 
		/// 0-未知，1-左，2-右，3-左外，4-右外
		/// </summary> 
		public virtual UInt16 ZTargetRow { get; set; }
		/// <summary> 
		/// 货叉默认编码器值 
		/// 固定值，中位默认编码器值，有的垛机默认值为0，有的默认值为1000000，因此ZPosition可能是全正值，也可能有负值
		/// </summary> 
		public virtual Int32 ZDefaultPosition { get; set; }
		/// <summary> 
		/// 货叉运行实时条码值 
		/// 如果存在多个编码器则把多个编码器值合一上报
		/// </summary> 
		public virtual Int32 ZPosition { get; set; }
		/// <summary> 
		/// 货叉运行目标条码值 
		/// </summary> 
		public virtual Int32 ZTargetPosition { get; set; }
		/// <summary> 
		/// 扫描条码 
		/// </summary> 
		public virtual string CheckBarcode { get; set; } = "";
		/// <summary> 
		/// 报警列表 
		/// </summary> 
		public virtual Byte[] Alarms { get; set; } = new Byte[0];
		/// <summary>
		/// 错误列表（默认为 ""）
		/// </summary>
		public virtual List<string> ErrorCodeList { get; set; }
		/// <summary> 
		/// 指令 
		/// </summary> 
		public virtual CmdTypes Cmd { get; set; }
		/// <summary> 
		/// 任务类型 
		/// </summary> 
		public virtual CraneTaskTypes TaskType { get; set; }
		/// <summary> 
		/// 设备任务号 
		/// 任务号由Wcs本地生成，由1开始累加。最大值9999999（7位）
		/// </summary> 
		public virtual UInt32 WcsEquipmentTaskId { get; set; }
		/// <summary> 
		/// 取货输送线编号 
		/// </summary> 
		public virtual UInt16 Pick_ConvNo { get; set; }
		/// <summary> 
		/// 放货输送线编号 
		/// </summary> 
		public virtual UInt16 Put_ConvNo { get; set; }
		/// <summary> 
		/// 货叉取货排 
		/// </summary> 
		public virtual UInt16 Fork_Pick_Row { get; set; }
		/// <summary> 
		/// 货叉取货列 
		/// </summary> 
		public virtual UInt16 Fork_Pick_Column { get; set; }
		/// <summary> 
		/// 货叉取货层 
		/// </summary> 
		public virtual UInt16 Fork_Pick_Level { get; set; }
		/// <summary> 
		/// 货叉放货排 
		/// </summary> 
		public virtual UInt16 Fork_Put_Row { get; set; }
		/// <summary> 
		/// 货叉放货列 
		/// </summary> 
		public virtual UInt16 Fork_Put_Column { get; set; }
		/// <summary> 
		/// 货叉放货层 
		/// </summary> 
		public virtual UInt16 Fork_Put_Level { get; set; }
		/// <summary> 
		/// Wcs任务号 
		/// </summary> 
		public virtual UInt32 WcsTaskId { get; set; }
		/// <summary> 
		/// 条码 
		/// </summary> 
		public virtual string WcsBarcode { get; set; } = "";
		/// <summary> 
		/// 是否需要校验条码 
		/// 0-不需要校验，1-需要校验
		/// </summary> 
		public virtual UInt16 IsNeedCheckBarcode { get; set; }
		/// <summary> 
		/// 随机数 
		/// </summary> 
		public virtual UInt16 DataId { get; set; }
		
		public override object this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string TypeFlag
		{
			get
			{
				return "";
			}
		}

        public override int Length
		{
			get
			{
				return 190;
			}
		}

        public override byte[] ToTelex()
        {
			return new byte[0];
        }

        public override string ToString()
        {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
