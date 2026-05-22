using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcs.Framework
{
    /// <summary>
    /// 表示一个具体的状态
    /// </summary>
    public abstract class AbstractState
    {
        public Logger _logger = LogManager.CreateNullLogger();
        public Random _random = new Random();
        /// <summary>
        /// 上下文构造函数
        /// </summary>
        /// <param name="context"></param>
        public AbstractState(AbstractStateManager context)
        {
            this.Context = context;
        }

        String _stepSerialNo = "";
        /// <summary>
        /// 步骤序列号
        /// </summary>
        public String StepSerialNo
        {
            get
            {
                return _stepSerialNo;
            }
            set
            {
                _stepSerialNo = value;
            }
        }

        /// <summary>
        /// 获取此状态关联的上下文
        /// </summary>
        public AbstractStateManager Context { get; private set; }

        /// <summary>
        /// 状态名称
        /// </summary>
        public abstract String Name { get; }
        /// <summary>
        /// 指示当前的状态是否可以执行
        /// </summary>
        /// <returns></returns>
        public abstract CanPerformResult CanPerform();

        /// <summary>
        /// 指示当前的状态是否已完成
        /// </summary>
        /// <returns></returns>
        public abstract IsCompeltedResult IsCompleted();
        /// <summary>
        /// 立即执行该状态操作
        /// </summary>
        /// <param name="context">状态上下文</param>
        public abstract void Perform();
        public override string ToString()
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return this.GetType().Name;
            }
            else
            {
                return String.Format("AbsoluteState#{0}", Name);
            }
        }
    }
}
