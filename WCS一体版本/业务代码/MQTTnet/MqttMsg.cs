using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public class MqttMsg
    {
        public MqttMsg()
        {
            SendAt = DateTime.Now;
        }
        public string Msg { get; set; }

        public DateTime SendAt { get; set; }
    }
}
