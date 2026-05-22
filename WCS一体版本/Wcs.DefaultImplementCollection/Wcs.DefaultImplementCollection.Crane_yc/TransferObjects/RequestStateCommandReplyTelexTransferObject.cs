using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    /// <summary>
    /// 表示一个堆垛机状态回报报文
    /// </summary>
    public class RequestStateCommandReplyTelexTransferObject : TelexTransferObject
    {
        protected String _telex;
        /// <summary>
        /// 堆垛机状态
        /// </summary>
        [DataMember]
        public virtual CraneStatus State { get; set; }
        /// <summary>
        /// 当前所在排
        /// </summary>
        [DataMember]
        public virtual Int32 Line { get; set; }
        /// <summary>
        /// 当前所在列
        /// </summary>
        [DataMember]
        public virtual Int32 Column { get; set; }
        /// <summary>
        /// 列编码值
        /// </summary>
        public virtual uint ColumnCodeValue { get; set; }
        /// <summary>
        /// 当前所在层
        /// </summary>
        [DataMember]
        public virtual Int32 Level { get; set; }
        /// <summary>
        /// 层编码值
        /// </summary>
        public virtual uint LevelCodeValue { get; set; }
        /// <summary>
        /// 货叉水平位置状态
        /// </summary>
        [DataMember]
        public virtual ForkHorizontalPosition ForkHorizontalPosition { get; set; }
        /// <summary>
        /// 货叉Z1编码值
        /// </summary>
        public virtual Int32 ForkCodeValue_Z1 { get; set; }
        /// <summary>
        /// 货叉Z2编码值
        /// </summary>
        public virtual Int32 ForkCodeValue_Z2 { get; set; }
        /// <summary>
        /// 货叉上下位置状态
        /// </summary>
        [DataMember]
        public virtual ForkVerticalPosition ForkVerticalPosition { get; set; }
        /// <summary>
        /// 指示堆垛机当前是否在站点位置
        /// </summary>
        [DataMember]
        public virtual Boolean AtStation { get; set; }
        /// <summary>
        /// 错误码（默认为 0）
        /// </summary>
        [DataMember]
        public virtual string ErrorCode
        {
            get
            {
                if (ErrorCodeList == null || ErrorCodeList.Count == 0)
                    return "";
                return ErrorCodeList.First();
            }
        }

        /// <summary>
        /// 错误列表（默认为 ""）
        /// </summary>
        [DataMember]
        public virtual List<string> ErrorCodeList { get; set; }
        /// <summary>
        /// 堆垛机事件
        /// </summary>
        [DataMember]
        public virtual CraneEvent Event { get; set; }
        /// <summary>
        /// 当前任务号
        /// </summary>
        [DataMember]
        public virtual String TaskId { get; set; }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string Barcode { set; get; }
        /// <summary>
        /// 验证条码
        /// </summary>
        public string Check_Barcode { set; get; }
        /// <summary>
        /// 堆垛机行走方向(未知=0，停止=1，前进=2，后退=3)
        /// </summary>
        public UInt16 WalkDirection { set; get; }
        /// <summary>
        /// 堆垛机载货台升降方向(未知=0，停止=1，上升=2，下降=3)
        /// </summary>
        public UInt16 LiftDirection { set; get; }
        /// <summary>
        /// 堆垛机货叉运行方向(未知=0，左1=1，右1=2，左2=3，右2=4，停止=5)
        /// </summary>
        public UInt16 ForkDirection { set; get; }

        public RequestStateCommandReplyTelexTransferObject()
            : base()
        {
        }

        public RequestStateCommandReplyTelexTransferObject(string telex) : base()
        {
            telex = telex.Replace('\0', '0');
            if (!ValidateType(telex))
            {
                throw new InvalidTelexTransferObjectException<RequestStateCommandReplyTelexTransferObject>(telex);
            }
            try
            {
                this.State = Utils.ConvertTo<CraneStatus>(telex.Substring(3, 2));
                this.Column = Convert.ToInt32(telex.Substring(5, 3));
                this.Level = Convert.ToInt32(telex.Substring(8, 3));
                this.ForkHorizontalPosition = Utils.ConvertTo<ForkHorizontalPosition>(telex.Substring(11, 1));
                this.ForkVerticalPosition = Utils.ConvertTo<ForkVerticalPosition>(telex.Substring(12, 1));
                this.AtStation = !Convert.ToBoolean(Convert.ToInt32(telex.Substring(13, 1)));//在站点是 0，不在站点是 1
                this.ErrorCodeList = new List<string>() { telex.Substring(13, 4) };
                this.Event = Utils.ConvertTo<CraneEvent>(telex.Substring(18, 1));
                this.TaskId = telex.Substring(19, 8);
                if (this.ExtendLength != 0)
                    this.ExtendInfo = telex.Substring(27, this.ExtendLength);
                else
                    this.ExtendInfo = "";
                _telex = telex;
            }
            catch (Exception ex)
            {
                throw new InvalidTelexTransferObjectException<RequestStateCommandReplyTelexTransferObject>(telex, ex);
            }
        }

        public override byte[] ToTelex()
        {
            return new byte[0];
        }

        const Int32 _oldLength = 28;

        /// <summary>
        /// 扩展报文长度
        /// </summary>
        public Int32 ExtendLength
        {
            get
            {
                try
                {
                    return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<Int32>("CraneRequestStateCommandReplyTelexTransferObjectExtendLength", 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 0;
                }
            }
        }
        /// <summary>
        /// 扩展信息
        /// </summary>
        [DataMember]
        public virtual String ExtendInfo { get; protected set; }

        public override int Length
        {
            get
            {
                return _oldLength + ExtendLength;
            }
        }


        public override string TypeFlag
        {
            get; set;
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }

        }

        public String GetInfo()
        {
            StringBuilder _builder = new StringBuilder();
            _builder.Append("报文：" + this.ToString());
            _builder.Append("状态：" + State);
            _builder.Append("事件：" + Event);
            _builder.Append("列：" + Column);
            _builder.Append("层：" + Level);
            _builder.Append("货叉水平位置:" + ForkHorizontalPosition);
            _builder.Append("货叉上下位置:" + ForkVerticalPosition);
            _builder.Append("是否在站点：" + AtStation);
            _builder.Append("错误码：" + ErrorCode);
            _builder.Append("任务号：" + TaskId);
            _builder.Append("扩展信息：" + ExtendInfo);
            return _builder.ToString();
        }
    }
}
