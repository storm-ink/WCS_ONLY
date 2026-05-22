using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AGV.Component.Tools;
using NLog;
using ZHQXC.WebAPI.Base;
using Wcs;

namespace ZHQXC
{
    public static class WebApiRequestHelper
    {
        public static Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>  
        /// 调用api返回xml  
        /// </summary>  
        /// <param name="url">api地址</param>  
        /// <param name="xmlstr">接收参数</param>  
        /// <param name="type">Get/Post</param>  
        /// <returns></returns>  
        public static string HttpApiWithXmlStr(string url, string xmlstr, string type = "post")
        {
            _logger.Info1($"准备调用HSMS WebAPI接口{url},调用参数\r\n{XElement.Parse(xmlstr)}", typeof(WebApiRequestHelper));
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址  
            request.Accept = "text/html,application/xhtml+xml,*/*";
            request.ContentType = "text/xml";
            request.Method = type.ToUpper().ToString();//get或者post  
            request.Timeout = Timeout;
            if (type == "post")
            {
                byte[] buffer;
                if (string.IsNullOrWhiteSpace(xmlstr))
                {
                    request.ContentLength = 0;
                    buffer = new byte[0];
                }
                else
                {
                    buffer = buffer = encoding.GetBytes(xmlstr);
                    request.ContentLength = buffer.Length;
                }
                Util.SetCertificatePolicy();
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
            }
            //request.ContentLength = buffer.Length;
            //request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse _response = (HttpWebResponse)request.GetResponse();
            string reply;
            using (StreamReader reader = new StreamReader(_response.GetResponseStream(), Encoding.UTF8))
            {
                reply = reader.ReadToEnd();
                reader.Close();
            }
            _logger.Info1($"收到调用 WebAPI接口{url} 的回复,回复消息\r\n{XElement.Parse(reply)}", typeof(WebApiRequestHelper));
            return reply;
        }

        /// <summary>  
        /// 调用api返回json  
        /// </summary>  
        /// <param name="url">api地址</param>  
        /// <param name="jsonstr">接收参数</param>  
        /// <param name="type">Get/Post</param>  
        /// <returns></returns>  
        public static string HttpApiWithJsonStr(string url, string jsonstr, string type = "post")
        {
            //_logger.Info1($"准备调用WebApi{url},调用参数\r\n{jsonstr}", typeof(WebApiRequestHelper));
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址  
            request.Accept = "text/html,application/xhtml+xml,*/*";
            request.ContentType = "application/json";
            request.Method = type.ToUpper().ToString();//get或者post  
            request.Timeout = Timeout;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (type == "post")
            {
                byte[] buffer;
                if (string.IsNullOrWhiteSpace(jsonstr))
                {
                    request.ContentLength = 0; 
                    buffer = new byte[0];
                }
                else
                {
                    buffer = encoding.GetBytes(jsonstr);
                    request.ContentLength = buffer.Length;
                }
                Util.SetCertificatePolicy();
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
            }

            HttpWebResponse _response = (HttpWebResponse)request.GetResponse();
            string reply;
            using (StreamReader reader = new StreamReader(_response.GetResponseStream(), Encoding.UTF8))
            {
                reply = reader.ReadToEnd();
                reader.Close();
            }
            //_logger.Info1($"收到调用 WebAPI接口{url} 的回复,回复消息\r\n{reply}", typeof(WebApiRequestHelper));
            return reply;
        }

        public static int Timeout
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<int>("rqeustWmsWebApiTimeOut", 30000);
            }
        }
    }
}
