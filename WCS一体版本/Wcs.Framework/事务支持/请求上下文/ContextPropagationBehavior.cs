using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace Wcs.Framework
{
    /// <summary>
    /// WCF 客户端的终节点行为，用于将本地用户数据传递到服务端
    /// </summary>
    public class ContextPropagationBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ContextSender());
        }

        public void Validate(ServiceEndpoint endpoint) { }

        #region IEndpointBehavior 成员

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            
        }

        #endregion
    }
}
