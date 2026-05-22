using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Wcs.Framework
{
    /// <summary>
    /// Ioc容器
    /// </summary>
    public class IocContainer
    {
        static UnityContainer _container;
        static IocContainer()
        {
            _container = new UnityContainer();

            UnityConfigurationSection config = ConfigurationManager.GetSection("unity") as UnityConfigurationSection;
            config.Configure(_container);
        }

        public static UnityContainer Container
        {
            get
            {
                return new UnityContainer();
            }
        }
        
        public static T Resolve<T>(params ResolverOverride[] overrides)
        {
            return Container.Resolve<T>(overrides);
        }       
    }
}
