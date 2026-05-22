using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gensong
{
    /// <summary>
    /// 原堆垛控制代码中提供的一个未被任何地方引用的类型。保留，未删除。
    /// </summary>
    public class Test
    {
        public void SendTest(string CName, EState eState, ETest eTest, ELR eLR, ECR eCR, ELoop eLoop, EFork eFork)
        { 
        
        
        }
    }

    /// <summary>货架 左右排</summary>
    public enum ELR
    {
        /// <summary>左右货架</summary>
        B,
        /// <summary>左货架</summary>
        L,
        /// <summary>右货架</summary>
        R
    }
    /// <summary>循环</summary>
    public enum ELoop
    {
        /// <summary>任务 循环</summary>
        Y,
        /// <summary>任务 不循环</summary>
        N
    }
    /// <summary>运行方向</summary>
    public enum ECR
    {
        /// <summary>先列后层</summary>
        CR,
        /// <summary>先层后列</summary>
        RC
    }
    /// <summary>伸叉</summary>
    public enum EFork
    {
        /// <summary>载货 伸叉</summary>
        Y,
        /// <summary>无货 不伸叉</summary>
        N
    }
    /// <summary> 状态</summary>
    public enum EState
    {
        /// <summary>暂停</summary>
        Pause,
        /// <summary>运行</summary>
        Run,
        /// <summary>终止</summary>
        Stop,
        /// <summary>中断(通讯)</summary>
        Break,
        /// <summary>结束(完成)</summary>
        Finish
    }
    /// <summary>手动操作 任务类型</summary>
    public enum ETest
    {
        /// <summary>层、列 的执行动作</summary>
        RunColRow,

        /// <summary>整列</summary>
        AllCol,
        /// <summary>整层</summary>
        AllRow,
        /// <summary>遍历货位</summary>
        Foreach,
        /// <summary>随机货位</summary>
        Random,
        /// <summary>完成</summary>
        Finish
    }
}
