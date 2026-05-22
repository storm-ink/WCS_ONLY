using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.DefaultImpls.Crane
{
    /// <summary>
    /// 表示一个堆垛机状态回报报文
    /// </summary>
    public class LATelexTransferObject : TelexTransferObject
    {
        String _telex;
        /// <summary>
        /// 堆垛机状态
        /// </summary>
        public CraneStatus State { get; private set; }
        /// <summary>
        /// 当前所在列
        /// </summary>
        public Int32 Column { get; private set; }
        /// <summary>
        /// 当前所在层
        /// </summary>
        public Int32 Level { get; private set; }
        /// <summary>
        /// 货叉水平位置状态
        /// </summary>
        public ForkHorizontalPosition ForkHorizontalPosition { get; private set; }
        /// <summary>
        /// 货叉上下位置状态
        /// </summary>
        public ForkVerticalPosition ForkVerticalPosition { get; private set; }
        /// <summary>
        /// 指示堆垛机当前是否在站点位置
        /// </summary>
        public Boolean AtStation { get; private set; }
        /// <summary>
        /// 错误码（默认为 0）
        /// </summary>
        public Int32 ErrorCode { get; private set; }
        /// <summary>
        /// 堆垛机事件
        /// </summary>
        public CraneEvent Event { get; private set; }
        /// <summary>
        /// 当前任务号
        /// </summary>
        public String TaskId { get; private set; }

        public LATelexTransferObject(string telex)
        {
            if (!ValidateType(telex))
            {
                throw new InvalidTelexTransferObjectException<LATelexTransferObject>(telex);
            }
            try
            {
                this.State = Utils.ConvertTo<CraneStatus>(telex.Substring(3, 2));
                this.Column = Convert.ToInt32(telex.Substring(5, 3));
                this.Level = Convert.ToInt32(telex.Substring(8, 3));
                this.ForkHorizontalPosition = Utils.ConvertTo<ForkHorizontalPosition>(telex.Substring(11, 1));
                this.ForkVerticalPosition = Utils.ConvertTo<ForkVerticalPosition>(telex.Substring(12, 1));
                this.AtStation = Convert.ToBoolean(Convert.ToInt32(telex.Substring(13, 1)));
                this.ErrorCode = Convert.ToInt32(telex.Substring(14, 4));
                this.Event = Utils.ConvertTo<CraneEvent>(telex.Substring(18, 1));
                this.TaskId = telex.Substring(19, 8);
                _telex = telex;
            }
            catch (Exception ex)
            {
                throw new InvalidTelexTransferObjectException<LATelexTransferObject>(telex, ex);
            }
        }

        public override string ToTelex()
        {
            return _telex;
        }

        public override int Length
        {
            get { return 28; }
        }


        public override string TypeFlag
        {
            get { return "LA"; }
        }

        public override object this[string name]
        {
            set { throw new NotImplementedException(); }
        }

    }
}
