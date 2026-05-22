using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;

namespace Wcs.Framework
{
    /// <summary>
    /// 自定义的消息检查器，用于将本地用户信息添加到请求消息当中
    /// </summary>
    public class ContextSender: IClientMessageInspector
    {
        string nsName = "http://gen-song.net/ns/WmsServiceContext";
        public void AfterReceiveReply(ref Message reply, object correlationState) { }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            request.Headers.Add(MessageHeader.CreateHeader("username", nsName, "(匿名用户)"));
            request.Headers.Add(MessageHeader.CreateHeader("realname", nsName, "Wcs服务器"));

            request.Headers.Add(MessageHeader.CreateHeader("clientip", nsName, Dns.GetHostByName(Dns.GetHostName()).AddressList.Length > 1 ? Dns.GetHostByName(Dns.GetHostName()).AddressList[1].ToString() : Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString()));
            return null;
        }
   }
}
