using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZHQXC
{
    // <summary>
    /// 自定义ConfigurationElement 节点对象
    /// </summary>
    public class WebApiAssembliesLoadElement : ConfigurationElement, IConfigurationSectionHandler
    {
        public List<string> dlls = new List<string>();

        public object Create(object parent, object configContext, XmlNode section)
        {
            foreach (XmlNode item in section.ChildNodes)
            {
                dlls.Add(item.Attributes["assemblyName"].Value);
            }
            return this;
        }
    }
}
