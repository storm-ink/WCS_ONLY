using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.RailGuidedVehicle
{
    public class AlarmCollection : ConfigurationElement
    {
        public AlarmElement[] AlarmElements { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public AlarmCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration)
        {

        }

        protected override void Deserialize()
        {
            List<AlarmElement> alarmElements = new List<AlarmElement>();
            foreach (XmlNode alarmElement in this.Node.SelectNodes("alarm"))
            {
                alarmElements.Add(new AlarmElement(alarmElement, this.WcsConfiguration));
            }

            this.AlarmElements = alarmElements.ToArray();
        }

    }
}
