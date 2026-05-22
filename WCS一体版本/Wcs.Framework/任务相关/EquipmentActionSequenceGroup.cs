using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wcs.Framework
{
    /// <summary>
    /// 任务序列组，如果任务列表存在组内，将由组对象接管设备调试
    /// </summary>
    public class EquipmentActionSequenceGroup
    {
        Thread _thread=null;
        public String Name { get; private set; }
        List<EquipmentActionSequence> _sequences;
        public Logger Logger { get; private set; }
        public EquipmentActionSequence[] Sequences
        {
            get
            {
                return _sequences.ToArray();
            }
        }
        public EquipmentActionSequenceGroup(String name,LogTarget logTarget)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            _sequences = new List<EquipmentActionSequence>();
            this.Name = name;
            Logger = new Logger(this, logTarget);

            _thread = null;
        }

        public void Add(EquipmentActionSequence sequence)
        {
            lock (this)
            {
                if (_sequences.Contains(sequence))
                {
                    throw new Exception("{0} 已存在于序列组内");
                }

                _sequences.Add(sequence);

                if (_thread == null)
                {
                    _thread = new Thread(Process);
                    _thread.Start();
                }
            }
        }

        protected virtual void Process()
        {
            while (true)
            {
                try
                {
                    foreach (var sequence in _sequences)
                    {
                        sequence.ExecuteNextAction();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.ToString(), this, ex);
                }

                Thread.Sleep(200);
            }
        }
    }
}
