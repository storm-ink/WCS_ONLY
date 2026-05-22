using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;
namespace Wcs.Framework
{
    /// <summary>
    /// WCF 客户端的终节点行为节点类型
    /// </summary>
    public class ContextPropagationBehaviorElement : BehaviorExtensionElement
    {
       public override Type BehaviorType
        {
            get { return typeof(ContextPropagationBehavior); }
        }
     
        protected override object CreateBehavior()
        {
          return new ContextPropagationBehavior();
      }
   }
}
