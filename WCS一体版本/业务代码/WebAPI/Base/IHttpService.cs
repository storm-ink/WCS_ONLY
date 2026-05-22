using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public interface IHttpService
    {
        IHttpService Initialization(int port);
        Task StartHttpServer();
        Task CloseHttpServer();
        void Dispose();
    }
}
