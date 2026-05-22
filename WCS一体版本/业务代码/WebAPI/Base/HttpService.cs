using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
//using Swashbuckle.Application;

namespace ZHQXC
{
    public class HttpService : IHttpService
    {
        /// <summary>
        /// HTTP self hosting.
        /// </summary>
        private readonly HttpSelfHostServer _server;

        public HttpService() { }

        public HttpService(int port)
        {
            var config = new HttpSelfHostConfiguration($"http://0.0.0.0:{port}");
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultWebApi", "api/{controller}/{action}");
            config.Services.Replace(typeof(IAssembliesResolver), new UserResolver());
            
            //ConfigureSwagger(config);
            _server = new HttpSelfHostServer(config);
        }
        //private static void ConfigureSwagger(HttpConfiguration config)
        //{
        //    //var thisAssembly = typeof(Startup).Assembly;
        //    //var parentPath = Path.GetDirectoryName(thisAssembly.Location);
        //    config.EnableSwagger(action =>
        //    {
        //        action.SingleApiVersion("v1", "AGVC For HSMS WebAPI");

        //        foreach (var item in config.Services.GetAssembliesResolver().GetAssemblies())
        //        {
        //            action.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}\{item.GetName().Name}.XML");
        //        }

        //    }).EnableSwaggerUi(action =>
        //    {
        //        action.DocumentTitle("AGVController API Document");
        //        //action.SetValidatorUrl("127.0.0.1:888");
        //    });
        //}

        public IHttpService Initialization(int port)
        {
            return new HttpService(port);
        }

        #region HTTP Service

        /// <summary>
        /// start HTTP server.
        /// </summary>
        public Task StartHttpServer()
        {
            return _server.OpenAsync();
        }

        /// <summary>
        /// Close HTTP server.
        /// </summary>
        public Task CloseHttpServer()
        {
            return _server.CloseAsync();
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
