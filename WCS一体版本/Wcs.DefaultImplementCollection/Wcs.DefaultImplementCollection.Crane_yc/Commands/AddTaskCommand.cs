using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个新增任务命令
    /// </summary>
    public class AddTaskCommand : TelexTransferObject, IDeviceCommandAdjudicator
    {
        /// <summary>
        /// 命令所属的堆垛机<br />
        /// 将命令实例限定到具体的设备，在发送前进行验证，防止命令中指定的位置与最终接收命令的设备位置系统不一致，增加安全性。
        /// </summary>
        public CraneDevice CraneDevice { get; private set; }
        /// <summary>
        /// 任务起始位置
        /// </summary>
        public RackLocation StartLocation { get; private set; }
        /// <summary>
        /// 任务结束位置
        /// </summary>
        public RackLocation EndLocation { get; private set; }
        /// <summary>
        /// 命令类型
        /// </summary>
        public AddTaskCommandType CommandType { get; private set; }
        /// <summary>
        /// 任务号<br />
        /// 为兼容旧系统中的 GOT 打头的任务，将此字段设为 <see cref="T:System.String"/> 类型。
        /// </summary>
        public String TaskId { get; private set; }
        /// <summary>
        /// wcs任务号
        /// </summary>
        public UInt32 WcsTaskId { get; private set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public UInt16 StepType { get; private set; }
        /// <summary>
        /// 是否需要校验 false=不需要（发送给堆垛机的值=0） true=需要（发送给堆垛机的值=1）
        /// </summary>
        public bool NeedCheck { get; private set; }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string Barcode { get; private set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="taskId">任务号</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="startLocation">任务起点位置</param>
        /// <param name="endLocation">任务终点位置</param>
        public AddTaskCommand(String taskId, AddTaskCommandType commandType, RackLocation startLocation, RackLocation endLocation, string barcode, bool needCheck = false, UInt32 wcsTaskId = 0, UInt16 stepType = 0)
        {
            //if (string.IsNullOrWhiteSpace(taskId) || taskId.Length != 8)
            //{
            //    throw new ArgumentOutOfRangeException(String.Format("任务号长度必须为 8 位，指定的任务号 “{0}” 长度为 {1}", taskId, taskId == null ? 0 : taskId.Length));
            //}

            if (startLocation == null)
            {
                throw new ArgumentNullException("startLocation");
            }

            if (endLocation == null)
            {
                throw new ArgumentNullException("endLocation");
            }

            if (!startLocation.Enabled)
            {
                throw new InvalidOperationException(string.Format("{0} 已被禁用", startLocation));
            }

            if (!endLocation.Enabled)
            {
                throw new InvalidOperationException(string.Format("{0} 已被禁用", endLocation));
            }

            if (startLocation.Device != endLocation.Device)
            {
                throw new InvalidOperationException(string.Format("{0} 和 {1} 不在同一设备运行范围内", startLocation, endLocation));
            }
            if (commandType == AddTaskCommandType.全自动)
            {
                if (startLocation.ForkDirection == null)
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许伸叉", startLocation));
                }

                if (!startLocation.IsForkActionAllowed(ForkAction.Pickup))
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许货叉的取货动作", startLocation));
                }

                if (endLocation.ForkDirection == null)
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许伸叉", endLocation));
                }

                if (!endLocation.IsForkActionAllowed(ForkAction.Putdown))
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许货叉的放货动作", endLocation));
                }

            }
            else if (commandType == AddTaskCommandType.半自动取货)
            {
                if (startLocation.ForkDirection == null)
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许伸叉", startLocation));
                }

                if (!startLocation.IsForkActionAllowed(ForkAction.Pickup))
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许货叉的取货动作", startLocation));
                }
            }
            else if (commandType == AddTaskCommandType.半自动放货)
            {
                if (endLocation.ForkDirection == null)
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许伸叉", endLocation));
                }

                if (!endLocation.IsForkActionAllowed(ForkAction.Putdown))
                {
                    throw new InvalidOperationException(String.Format("{0} 位置不允许货叉的放货动作", endLocation));
                }
            }
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.CommandType = commandType;
            this.CraneDevice = (CraneDevice)startLocation.Device;
            this.TaskId = taskId;
            this.WcsTaskId = wcsTaskId;
            this.StepType = stepType;
            this.NeedCheck = needCheck;
            if (string.IsNullOrWhiteSpace(barcode))
                this.Barcode = "";//默认20个字节的Ascii码空字符
            else
                this.Barcode = barcode;
        }
        public override byte[] ToTelex()
        {
            //堆垛机协议版本
            var version = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("CraneCommandVersion", "");
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("未配置堆垛机协议版本(配置关键字：CraneCommandVersion)");

            //是否使用用户列代替设备列1234
            if (!Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings.ContainsKey("SendCraneIsUserLine"))
                throw new ArgumentException("未配置堆垛机协议是使用自定义的列或者用户列作为发送源(配置关键字：SendCraneIsUserLine)");

            var _isUserLine = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Boolean>("SendCraneIsUserLine", false);

            var plcType = !Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetAttributeOrDefault<bool>("STKPLCReverse", true);

            //西门子
            if (!plcType)
            {
                List<byte> result = new List<byte>();
                //报文头
                result.AddRange(Prefix);
                if (this.CommandType == AddTaskCommandType.全自动)
                    TypeFlag = "BY";
                else if (this.CommandType == AddTaskCommandType.半自动取货)
                    TypeFlag = "QH";
                else if (this.CommandType == AddTaskCommandType.半自动放货)
                    TypeFlag = "FH";
                else
                    TypeFlag = "YD";

                if (this.CommandType == AddTaskCommandType.全自动)
                {

                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length).Reverse());
                    //功能码2
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号4
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())).Reverse());
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId).Reverse());
                    //任务类型 1 无货行走，2、取货 3、带货行走 4、放货
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType).Reverse());
                    //取货排
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)).Reverse());
                    //result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)).Reverse());
                    //取货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)).Reverse());
                    //取货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)).Reverse());
                    //放货排
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToByte(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToByte(this.EndLocation.UserLine)).Reverse());
                    //放货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)).Reverse());
                    //放货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)).Reverse());
                    //货叉任务类型
                    result.AddRange(new Byte[] { Convert.ToByte(0) });
                    //是否验证条码
                    if (NeedCheck)
                        result.AddRange(BitConverter.GetBytes((UInt16)1).Reverse());
                    else
                        result.AddRange(BitConverter.GetBytes((UInt16)0).Reverse());
                    //条码值
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动取货)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length).Reverse());
                    //功能码
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())).Reverse());
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId).Reverse());
                    //任务类型
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType).Reverse());
                    //取货排
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)).Reverse());
                    //取货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)).Reverse());
                    //取货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)).Reverse());
                    //放货排
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)).Reverse());
                    //放货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)).Reverse());
                    //放货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)).Reverse());
                    //货叉任务类型
                    result.AddRange(BitConverter.GetBytes(Convert.ToInt16(0)));
                    //是否验证条码
                    if (NeedCheck)
                        result.AddRange(BitConverter.GetBytes((UInt16)1).Reverse());
                    else
                        result.AddRange(BitConverter.GetBytes((UInt16)0).Reverse());
                    //托盘条码
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动放货)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length).Reverse());
                    //功能码2
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号4
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())).Reverse());
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId).Reverse());
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType).Reverse());
                    //开始排2
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)).Reverse());
                    //开始列2
                    //result.AddRange(GetColumnsRowLevelData(this.StartLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)).Reverse());
                    //开始层2
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)).Reverse());
                    //目的地排2
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)).Reverse());
                    //目的地列2
                    //result.AddRange(GetColumnsRowLevelData(this.EndLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)).Reverse());
                    //目的地层2
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)).Reverse());
                    //货叉任务类型
                    result.AddRange(new Byte[] { Convert.ToByte(0) });
                    //是否验证条码
                    if (NeedCheck)
                        result.AddRange(BitConverter.GetBytes((UInt16)1).Reverse());
                    else
                        result.AddRange(BitConverter.GetBytes((UInt16)0).Reverse());
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动行走)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length).Reverse());
                    //功能码
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())).Reverse());
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId).Reverse());
                    //任务类型
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType).Reverse());
                    //目的地排
                    if (!_isUserLine)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))).Reverse());
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)).Reverse());
                    //目的地列
                    //result.AddRange(GetColumnsRowLevelData(this.EndLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)).Reverse());
                    //目的地层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)).Reverse());
                    //是否验证条码
                    if (NeedCheck)
                        result.AddRange(BitConverter.GetBytes((UInt16)1).Reverse());
                    else
                        result.AddRange(BitConverter.GetBytes((UInt16)0).Reverse());
                    //条码值
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else
                    throw new Exception($"不支持的命令类型{this.CommandType}");

                return result.ToArray();
            }
            else
            {















                List<byte> result = new List<byte>();
                //报文头
                result.AddRange(Prefix);
                if (this.CommandType == AddTaskCommandType.全自动)
                    TypeFlag = "BY";
                else if (this.CommandType == AddTaskCommandType.半自动取货)
                    TypeFlag = "QH";
                else if (this.CommandType == AddTaskCommandType.半自动放货)
                    TypeFlag = "FH";
                else
                    TypeFlag = "YD";

                if (this.CommandType == AddTaskCommandType.全自动)
                {

                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length));
                    //功能码2
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号4
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())));
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId));
                    //任务类型 1 无货行走，2、取货 3、带货行走 4、放货
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType));
                    //取货排
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)));
                    //result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)) );
                    //取货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)));
                    //取货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)));
                    //放货排
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToByte(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToByte(this.EndLocation.UserLine)));
                    //放货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)));
                    //放货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)));
                    //货叉任务类型
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    //是否验证条码
                    if (NeedCheck)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)0));
                    //条码值
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动取货)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length));
                    //功能码
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())));
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId));
                    //任务类型
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType));
                    //取货排
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)));
                    //取货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)));
                    //取货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)));
                    //放货排
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)));
                    //放货列
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)));
                    //放货层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)));
                    //货叉任务类型
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    //是否需要验证条码
                    if (NeedCheck)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    //托盘条码
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动放货)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length));
                    //功能码2
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号4
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())));
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId));
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType));
                    //开始排2
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.StartLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.StartLocation.UserLine)));
                    //开始列2
                    //result.AddRange(GetColumnsRowLevelData(this.StartLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Column).GetType().Name, Convert.ToInt16(this.StartLocation.Column)));
                    //开始层2
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.StartLocation.Level).GetType().Name, Convert.ToInt16(this.StartLocation.Level)));
                    //目的地排2
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)));
                    //目的地列2
                    //result.AddRange(GetColumnsRowLevelData(this.EndLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)));
                    //目的地层2
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)));
                    //货叉任务类型
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    if (NeedCheck)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)0));
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else if (this.CommandType == AddTaskCommandType.半自动行走)
                {
                    //数据长度
                    result.AddRange(SendconvertNumberToBytes(Length.GetType().Name, Length));
                    //功能码
                    result.AddRange(System.Text.Encoding.Default.GetBytes(TypeFlag));
                    //任务号
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt32(TaskId.Trim()).GetType().Name, Convert.ToInt32(TaskId.Trim())));
                    //WCS任务号
                    result.AddRange(SendconvertNumberToBytes(WcsTaskId.GetType().Name, WcsTaskId));
                    //任务类型
                    result.AddRange(SendconvertNumberToBytes(StepType.GetType().Name, StepType));
                    //目的地排
                    //if (!_isUserLine)
                    //    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0))) );
                    //else
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(((int?)this.EndLocation.ForkDirection).GetValueOrDefault(0)).GetType().Name, Convert.ToInt16(this.EndLocation.UserLine)));
                    //目的地列
                    //result.AddRange(GetColumnsRowLevelData(this.EndLocation.Column.ToString()));
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, Convert.ToInt16(this.EndLocation.Column)));
                    //目的地层
                    result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Level).GetType().Name, Convert.ToInt16(this.EndLocation.Level)));
                    //是否需要验证条码
                    if (NeedCheck)
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)1));
                    else
                        result.AddRange(SendconvertNumberToBytes(Convert.ToInt16(this.EndLocation.Column).GetType().Name, (Int16)0));
                    //条码值
                    result.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Barcode.PadLeft(20, ' ')));
                }
                else
                    throw new Exception($"不支持的命令类型{this.CommandType}");

                return result.ToArray();
            }

        }

        public override string TypeFlag
        {
            get; set;


        }

        public override int Length
        {
            get
            {
             
                if (this.CommandType == AddTaskCommandType.半自动取货
                   || this.CommandType == AddTaskCommandType.半自动放货)
                    return 48;
                else if (this.CommandType == AddTaskCommandType.全自动)
                {
                    return 48;
                }
                else
                    return 40;

            }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool SendSuccess(TaskableDevice taskableDevice, DeviceCommand command)
        {
            CraneDevice craneDevice = (CraneDevice)taskableDevice;
            AddTaskCommand cmd = (AddTaskCommand)command;
            return craneDevice.LastStatus != null
                && craneDevice.LastStatus.TaskId == int.Parse(cmd.TaskId).ToString().Trim();
        }
    }
}
