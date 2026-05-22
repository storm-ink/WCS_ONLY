using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ZHQXC
{
    [RoutePrefix("API/Test")]
    public class TestController : ApiController
    {
        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [Route("test")]
        [HttpGet]
        public IHttpActionResult GetServerTime()
        {
            return Json(new { Message = $"This is Port Test Message from ZHQXC.TestController, Now is {DateTime.Now.ToString("yyyyy-MM-dd HH:mm:ss.ffff")}" });
        }
    }
}