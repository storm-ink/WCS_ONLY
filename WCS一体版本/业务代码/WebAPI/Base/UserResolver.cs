using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace ZHQXC
{
    public class UserResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            List<Assembly> assemblies = new List<Assembly>();
            var assembly = AppDomain.CurrentDomain.BaseDirectory;

            var _config = ConfigurationManager.GetSection("webApiAssemblies") as WebApiAssembliesLoadElement;
            foreach (var item in _config.dlls)
            {
                string path = assembly + "\\" + item;
                var controllersAssembly = Assembly.LoadFrom(path);
                assemblies.Add(controllersAssembly);
            }
            return assemblies;
        }
    }
}
