using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class SequenceGroupElement:ConfigurationElement
    {
        public EquipmentActionSequenceGroup Group { get; private set; }
        public SequenceGroupSelection SequenceGroupSelection { get; private set; }
        public SequenceGroupElement(XmlNode node, SequenceGroupSelection sequenceGroupSelection)
            : base(node,false)
        {
            SequenceGroupSelection = sequenceGroupSelection;
            Deserialize(node);
        }
        protected override void Deserialize(System.Xml.XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.Attributes["type"] == null)
            {
                throw new Exception("未指定 SequenceGroup 节点的 type 属性");
            }

            LogTarget logTarget = null;
            string logTargetName = this.Element.Attributes["logTarget"] == null ? "" : this.Element.Attributes["logTarget"].Value;
            if (!string.IsNullOrWhiteSpace(logTargetName))
            {
                logTarget = Configuration.GetLogTarget(logTargetName);
            }

            string typeName = node.Attributes["type"].Value;
            string name = node.Attributes["name"].Value;
            if (SequenceGroupSelection.SequenceGroupElements.Any(x => x.Group.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new Exception(string.Format("已存在名为 {0} SequenceGroup", name));
            }
            EquipmentActionSequenceGroup group = CreateInstance<EquipmentActionSequenceGroup>(typeName, name,logTarget);
            this.Group = group;
        }
    }
}
