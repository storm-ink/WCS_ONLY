using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;

namespace ZHQXC
{
    public static class WebApiSelfHostHelper
    {
        public static List<IHttpService> _services = new List<IHttpService>();
        /// <summary>
        /// Initialiazation and start
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        public async static void StartWebApi<T>(int port) where T : IHttpService, new()
        {
            if (_services.Any(x => x is T))
                return;

            /**
             * initialize http service.
             */
            T _http = new T();
            _http = (T)_http.Initialization(port);

              await _http.StartHttpServer();
            _services.Add(_http);
        }
        public async static void EndWebApi<T>() where T : IHttpService
        {
            if (!_services.Any(x => x is T))
                return;

            var _http = _services.FirstOrDefault(x => x is T);
            await _http.CloseHttpServer();
            _http.Dispose();
            _services.Remove(_http);
        }
    }
}