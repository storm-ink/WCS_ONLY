
using System;
using System.Runtime.Serialization;
namespace Wcs.Framework.CraneControl
{
    /// <summary>发任务 GOT -&gt; CRN  &lt;HB 起点排列层(8) + 终点排列层(8) + 动作命令(2) + 任务号(8)&gt;</summary>
    [Serializable]
    [DataContract]
    public sealed class HB : RequestTelex
    {
        /// <summary>跑任务</summary>
        public HB(SHB sHB)
        {
            this.SHB = sHB; this.CMD = HBCommand.E11; this.TaskId = sHB.TaskId;

            foreach (var v in Config.Crans)
            {
                if (String.Equals(v.CName,SHB.CName,StringComparison.CurrentCultureIgnoreCase))
                {
                    v.Shelf.ParseUTMColRow(this);

                    this.Head = "HB";
                    this.Body = string.Format("{0:00}{1:000}{2:000}{3:00}{4:000}{5:000}{6:00}{7}",
                        (int)LR1, P1.MCol, P1.MRow, (int)LR2, P2.MCol, P2.MRow, (int)CMD, TaskId);

                    return;
                }
            }

            throw new Exception(string.Format("无 {0} 的配置信息...", SHB.CName));
        }
        /// <summary>跑站点</summary>
        public HB(Position P2)
        {
            this.LR1 = EForkLR.L; this.P1 = new Position();
            this.LR2 = EForkLR.L; this.P2 = P2;
            this.CMD = HBCommand.E00; this.TaskId = Task.GetGotId;

            this.Head = "HB";
            this.Body = string.Format("{0:00}{1:000}{2:000}{3:00}{4:000}{5:000}{6:00}{7}",
                (int)LR1, P1.MCol, P1.MRow, (int)LR2, P2.MCol, P2.MRow, (int)CMD, TaskId);
        }

        /// <summary>跑站点</summary>
        public HB(Position P2,string destination,string taskId,string craneName)
        {
            this.LR1 = EForkLR.L; this.P1 = new Position();
            this.LR2 = EForkLR.L; this.P2 = P2;
            this.CMD = HBCommand.E00;
            this.SHB = new SHB(craneName, destination, destination, taskId);

            this.Head = "HB";
            if (string.IsNullOrEmpty(taskId))
            {
                this.TaskId = Task.GetGotId;
            }
            else
            {
                if (taskId.Length < 8)
                {
                    taskId = taskId.PadRight(8 - taskId.Length, '0');
                }
                this.TaskId = taskId;
            }

            foreach (var v in Config.Crans)
            {
                if (String.Equals(v.CName, craneName, StringComparison.CurrentCultureIgnoreCase))
                {
                    v.Shelf.ParseUTMColRow(this);

                    this.Head = "HB";
                    this.Body = string.Format("{0:00}{1:000}{2:000}{3:00}{4:000}{5:000}{6:00}{7}",
                        (int)LR1, P1.MCol, P1.MRow, (int)LR2, P2.MCol, P2.MRow, (int)CMD, TaskId);

                    return;
                }
            }

            throw new Exception(string.Format("无 {0} 的配置信息...", SHB.CName));
        }
        /// <summary>取放货</summary>
        public HB(EForkLR LR2, HBCommand CMD)
        {
            this.LR1 = EForkLR.L; this.P1 = new Position();
            this.LR2 = LR2; this.P2 = this.P1;
            this.CMD = CMD; this.TaskId = Task.GetGotId;

            this.Head = "HB";
            this.Body = string.Format("{0:00}{1:000}{2:000}{3:00}{4:000}{5:000}{6:00}{7}",
                (int)LR1, P1.MCol, P1.MRow, (int)LR2, P2.MCol, P2.MRow, (int)CMD, TaskId);
        }

        /// <summary>最后一次发送的报文信息</summary>
        [DataMember]
        public SHB SHB { get; private set; }

        /// <summary>起点 排</summary>
        [DataMember]
        public EForkLR LR1 { get; internal set; }
        /// <summary>起点 位置</summary>
        [DataMember]
        public Position P1 { get; internal set; }

        /// <summary>终点 排</summary>
        [DataMember]
        public EForkLR LR2 { get; internal set; }
        /// <summary>终点 位置</summary>
        [DataMember]
        public Position P2 { get; internal set; }

        /// <summary>动作命令</summary>
        [DataMember]
        public HBCommand CMD { get; private set; }

        /// <summary>任务号</summary>
        [DataMember]
        public string TaskId { get; set; }
    }

    /// <summary>任务信息</summary>
    [DataContract]
    public class SHB
    {
        /// <summary></summary>
        public SHB() { }
        /// <summary></summary>
        public SHB(string CName, string Position1, string Position2, string TaskId)
        {
            this.CName = CName; this.Position1 = Position1; this.Position2 = Position2; this.TaskId = TaskId;
        }

        /// <summary>堆垛机名</summary>
        [DataMember]
        public string CName;
        /// <summary>起点 00-000-000</summary>
        [DataMember]
        public string Position1;
        /// <summary>终点 00-000-000</summary>
        [DataMember]
        public string Position2;
        /// <summary>任务号</summary>
        [DataMember]
        public string TaskId;

        /// <summary></summary>
        public override string ToString()
        {
            return string.Format("{0}  {1}  {2}", 
                TaskId == null ? "00000000" : TaskId,
                Position1 == null ? "00-000-000" : Position1,
                Position2 == null ? "00-000-000" : Position2);
        }
    }

    /// <summary>动作命令</summary>
    public enum HBCommand
    {
        /// <summary>半自动 行走</summary>
        E00,
        /// <summary>半自动 取货</summary>
        E01,
        /// <summary>半自动 放货</summary>
        E02,

        /// <summary>全自动</summary>
        E11 = 11,

        //00、半自动 行走
        //01、半自动 取货 
        //02、半自动 放货 

        //03、手动   左伸叉 
        //04、手动   右伸叉 
        //05、手动   前进 
        //06、手动   后退 
        //07、手动   上 
        //08、手动   下 
        //09、手动   取消任务 
        //10、手动   回叉中位 

        //11、全自动
        //12、全自动 取消

        //13、手操盒 左伸叉
        //14、手操盒 右伸叉
        //15、手操盒 前进 
        //16、手操盒 后退 
        //17、手操盒 上 
        //18、手操盒 下 
    }

    /// <summary>单步动作</summary>
    public enum EStep
    {
        /// <summary>堆垛机 前进</summary>
        Go,
        /// <summary>堆垛机 后退</summary>
        Back,
        /// <summary>堆垛机 上升</summary>
        Up,
        /// <summary>堆垛机 下降</summary>
        Down
    }
}
