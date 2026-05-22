using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public class WebApiAssembliesLoadElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WebApiAssembliesLoadElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            WebApiAssembliesLoadElement _webApiAssembly = element as WebApiAssembliesLoadElement;
            if (_webApiAssembly == null)
                throw new ArgumentNullException("element is not WebApiAssembliesLoadElement");
            return _webApiAssembly;
        }

        public WebApiAssembliesLoadElement Get(string name)
        {
            return (WebApiAssembliesLoadElement)BaseGet(name);
        }
    }
}