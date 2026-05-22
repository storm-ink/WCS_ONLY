using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace ZHQXC
{
    public class WebApiAssembliesLoadSection : ConfigurationSection
    {
        public WebApiAssembliesLoadElementCollection WebApiAssemblies
        {
            get
            {
                return (WebApiAssembliesLoadElementCollection)this["webApiAssemblies"];
            }
        }
    }
}
