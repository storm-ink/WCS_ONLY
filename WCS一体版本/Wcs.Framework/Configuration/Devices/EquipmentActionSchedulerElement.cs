using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class EquipmentActionSchedulerElement:ConfigurationElement
    {
        public Type Type { get; private set; }
        public EquipmentActionScheduler EquipmentActionScheduler { get; private set; }
        public EquipmentActionSchedulerFilterElement[] FilterElements { get; private set; }
        public Device Device { get; private set; }
        public EquipmentActionSchedulerElement(XmlNode node, WcsConfiguration configuration) : base(node, configuration, false)
        {
            Deserialize();
        }
        public EquipmentActionSchedulerElement(Device device,XmlNode node,WcsConfiguration configuration):base(node,configuration,false)
        {
            this.Device = device;

            Deserialize();
        }

        protected override void Deserialize()
        {
            var typeName = GetAttributeOrDefault<String>("type",typeof(EquipmentActionScheduler).FullName);
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("未找到 {0} 节点 {1} 属性值 “{2}” 标识的类型", path, "type", typeName));
            }

            if (!type.IsSubclassOf(typeof(EquipmentActionScheduler)) && type != typeof(EquipmentActionScheduler))
            {
                var path = this.Node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 标识的类型不是 {3} 的子类", path, "type", typeName, typeof(EquipmentActionScheduler)));
            }

            this.Type = type;

            List<EquipmentActionSchedulerFilterElement> filterElements = new List<EquipmentActionSchedulerFilterElement>();
            foreach (XmlNode filterNode in Node.SelectNodes("filter"))
            {
                filterElements.Add(new EquipmentActionSchedulerFilterElement(filterNode,this.WcsConfiguration));
            }
            this.FilterElements = filterElements.ToArray();

            this.EquipmentActionScheduler=CreateScheduler();
        }

        protected virtual EquipmentActionScheduler CreateScheduler()
        {
            if (this.EquipmentActionScheduler != null)
            {
                throw new InvalidOperationException("EquipmentActionScheduler 已被创建，不允许重复调用该方法。");
            }

            this.EquipmentActionScheduler=ReflectionHelper.CreateInstance<EquipmentActionScheduler>(this.Type, this.Device,this.FilterElements.Select(x=>x.EquipmentActionSchedulerFilter).ToArray());
            
            this.EquipmentActionScheduler._FixUnknowEquipmentTaskOnStatusChanged = this.GetAttributeOrDefault<Boolean>("_fxuet", true);
            
            return this.EquipmentActionScheduler;
        }
    }
}
