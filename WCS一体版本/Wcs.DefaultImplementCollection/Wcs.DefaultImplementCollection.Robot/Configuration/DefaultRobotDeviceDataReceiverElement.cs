using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Robot
{
    public sealed class DefaultRobotDeviceDataReceiverElement : DataReceiverElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public DefaultRobotDeviceDataReceiverElement(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {
        }

        public override IDataReceiver CreateDataReceiver(string deviceName)
        {
            return new DefaultTcpRobotDataReceiver(this.Name);
        }

    }
}
