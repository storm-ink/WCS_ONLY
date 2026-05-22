using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane
{
    /// <summary>
    /// 堆垛机命令
    /// </summary>
    public class CraneCommand : TelexTransferObject, IDeviceCommandAdjudicator
    {
        /// <summary> 
        /// 指令 
        /// </summary> 
        public virtual CmdTypes Cmd { get; private set; } = CmdTypes.Unknown;
        /// <summary> 
		/// 任务类型 
		/// </summary> 
		public virtual CraneTaskTypes TaskType { get; private set; } = CraneTaskTypes.Unknown;
        /// <summary> 
		/// 设备任务号 
		/// </summary> 
		public virtual UInt32 EquipmentTaskId { get; private set; } = 0;
        /// <summary> 
		/// 取货输送线编号 
		/// </summary> 
		public virtual UInt16 PickCVNO { get; private set; } = 0;
        /// <summary> 
		/// 放货输送线编号 
		/// </summary> 
		public virtual UInt16 PutCVNO { get; private set; } = 0;
        /// <summary> 
		/// 货叉取货排 
		/// </summary> 
        public virtual UInt16 ForkPickRow { get; private set; }
        /// <summary> 
		/// 货叉取货列 
		/// </summary> 
		public virtual UInt16 ForkPickColumn { get; private set; } = 0;
        /// <summary> 
		/// 货叉取货层 
		/// </summary> 
		public virtual UInt16 ForkPickLevel { get; private set; } = 0;
        /// <summary> 
		/// 货叉放货排 
		/// </summary> 
        public virtual UInt16 ForkPutRow { get; private set; }
        /// <summary> 
		/// 货叉放货列 
		/// </summary> 
		public virtual UInt16 ForkPutColumn { get; private set; } = 0;
        /// <summary> 
		/// 货叉放货层 
		/// </summary> 
		public virtual UInt16 ForkPutLevel { get; private set; } = 0;
        /// <summary> 
		/// Wcs任务号 
		/// </summary> 
		public virtual UInt32 WcsTaskId { get; private set; } = 0;
        /// <summary> 
		/// 条码 
		/// </summary> 
		public virtual string WcsBarcode { get; private set; } = "";
        /// <summary> 
		/// 是否需要校验条码 
        /// 0-不需要校验，1-需要校验
		/// </summary> 
		public virtual UInt16 IsNeedCheckBarcode { get; private set; } = 0;
        /// <summary> 
		/// 随机数 
		/// </summary> 
		public virtual UInt16 DataId { get; private set; } = (UInt16)new Random().Next(1, UInt16.MaxValue);

        public override string TypeFlag { get; } = "";

        public override int Length { get; } = 0;

        ///// <summary>
        ///// 默认构造函数
        ///// </summary>
        public CraneCommand()
        {
        }

        /// <summary>
        /// 构造函数 - cmd = NewTask 时
        /// </summary>
        /// <param name="pick">取货点,可null</param>
        /// <param name="put">放货点,可null</param>
        /// <param name="equipmentTaskId"></param>
        /// <param name="wcsTaskId"></param>
        /// <param name="wcsBarcode"></param>
        /// <param name="isNeedCheckBarcode"></param>
        /// 总结: Cmd = 1时，除全自动任务和单步取货任务（TaskType=1 和 TaskType=2）需要pick站点外，其余均以 put站点为主
        /// Cmd : 1-新任务，2-清除任务，3-急停 4-取消急停
        /// TaskType: 1-全自动，2-单步取货，3-单步放货，4-行走，5.探货盘存，6.扫码盘检
        /// 1.Cmd=2、3、4时，后续字段为补充字段，根据需要填充0或者特定字符（ASCII码值32）
        /// 2.Cmd=1，TaskType=1时，TaskId、Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level、Fork_Put_Row、Fork_Put_Column、Fork_Put_Level均需要准确值，WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值
        /// 3.Cmd=1，TaskType=2时，TaskId、Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level均需要准确值，Fork_Put_Row、Fork_Put_Column、Fork_Put_Level、WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值，取货时不做高度检测
        /// 4.Cmd=1，TaskType=3时，TaskId、Fork_Put_Row、Fork_Put_Column、Fork_Put_Level均需要准确值，Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level、WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值，放货时垛机应判断货格高度是否和当前检测高度一致，不一致则报警人工处理
        /// 5.Cmd=1，TaskType=4时，TaskId、Fork_Put_Row、Fork_Put_Column、Fork_Put_Level均需要准确值，Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level、WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值
        /// 6.Cmd=1，TaskType=5时，TaskId、Fork_Put_Row、Fork_Put_Column、Fork_Put_Level均需要准确值，Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level、WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值
        /// 7.Cmd=1，TaskType=6时，TaskId、Fork_Put_Row、Fork_Put_Column、Fork_Put_Level均需要准确值，Fork_Pick_Row、Fork_Pick_Column、Fork_Pick_Level、WcsTaskId、TUID、Height、IsNeedCheckBarcode按需填充相关值
        public CraneCommand(CmdTypes cmd, CraneTaskTypes craneTaskType, RackLocation pick, RackLocation put, UInt32 equipmentTaskId, UInt32 wcsTaskId, string wcsBarcode = "")
        {
            Cmd = cmd;
            TaskType = craneTaskType;
            if (pick != null && pick.Synonymous.Count() != 0)
            {
                if (UInt16.TryParse(pick.Synonymous[0].DeviceCode, out UInt16 pickCVNO))
                    PickCVNO = pickCVNO;
            }
            if (put != null && put.Synonymous.Count() != 0)
            {
                if (UInt16.TryParse(put.Synonymous[0].DeviceCode, out UInt16 putCVNO))
                    PutCVNO = putCVNO;
            }

            var _isUserLine = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("CraneCommandIsUserLine", false);
            if (_isUserLine)
            {
                if (pick != null)
                    ForkPickRow = (UInt16)pick.UserLine;
                if (put != null)
                    ForkPutRow = (UInt16)put.UserLine;
            }
            else
            {
                if (pick != null)
                    ForkPickRow = (UInt16)pick.ForkDirection;
                if (put != null)
                    ForkPutRow = (UInt16)put.ForkDirection;
            }

            if (pick != null)
            {
                ForkPickColumn = (UInt16)pick.Column;
                ForkPickLevel = (UInt16)pick.Level;
            }
            if (put != null)
            {
                ForkPutColumn = (UInt16)put.Column;
                ForkPutLevel = (UInt16)put.Level;
            }
            EquipmentTaskId = equipmentTaskId;
            WcsTaskId = wcsTaskId;
            var length = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt16>("CraneCommandBarcodeLength", 20);
            if (string.IsNullOrWhiteSpace(wcsBarcode))
                WcsBarcode = "".PadLeft(length, ' ');
            else
                WcsBarcode = wcsBarcode.PadLeft(length, ' ');
            var isNeedCheckBarcode = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("CraneCommandIsNeedCheckBarcode", false);
            IsNeedCheckBarcode = (UInt16)(isNeedCheckBarcode ? 1 : 0);
        }

        /// <summary>
        /// 构造函数 - cmd != NewTask 时
        /// </summary>
        /// <param name="cmd"></param>
        public CraneCommand(CmdTypes cmd)
        {
            Cmd = cmd;
            var length = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<UInt16>("CraneCommandBarcodeLength", 20);
            WcsBarcode = "".PadLeft(length, ' ');
        }

        byte[] bytes = null;
        static object objLocker = new object();
        public override byte[] ToTelex()
        {
            if (bytes == null)
            {
                lock (objLocker)
                {
                    if (bytes == null)
                    {
                        List<byte> contentList = new List<byte>();
                        contentList.AddRange(SendconvertNumberToBytes(Cmd));
                        contentList.AddRange(SendconvertNumberToBytes(TaskType));
                        contentList.AddRange(SendconvertNumberToBytes(EquipmentTaskId));
                        contentList.AddRange(SendconvertNumberToBytes(PickCVNO));
                        contentList.AddRange(SendconvertNumberToBytes(PutCVNO));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPickRow));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPickColumn));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPickLevel));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPutRow));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPutColumn));
                        contentList.AddRange(SendconvertNumberToBytes(ForkPutLevel));
                        contentList.AddRange(SendconvertNumberToBytes(WcsTaskId));
                        contentList.AddRange(SendconvertNumberToBytes(WcsBarcode));
                        contentList.AddRange(SendconvertNumberToBytes(IsNeedCheckBarcode));
                        contentList.AddRange(SendconvertNumberToBytes(DataId));
                        List<byte> list = new List<byte>();
                        list.AddRange(Prefix);
                        list.AddRange(SendconvertNumberToBytes(contentList.Count()));
                        list.AddRange(contentList);
                        bytes = list.ToArray();
                    }
                }
            }
            return bytes;
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            CraneDevice craneDevice = (CraneDevice)taskableDevice;
            if (craneDevice.LastStatus == null)
                return false;

            return craneDevice.LastStatus.DataId == this.DataId;
        }
    }
}
