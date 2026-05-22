using MQTTnet;
using MQTTnet.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcs;

namespace ZHQXC
{
    public class WCSMqttClient_WCS : Wcs.Framework.ThreadRunningLog
    {
        Logger _logger;

        public static WCSMqttClient_WCS Instance;

        static object objLock = new object();
        public static WCSMqttClient_WCS CreateInstance()
        {
            lock (objLock)
            {
                if (Instance == null)
                    Instance = new WCSMqttClient_WCS("WCSMqttClient_WCS");

                return Instance;
            }
        }

        private MqttFactory factory;
        public IMqttClient client;
        private MqttClientOptions options;
        private List<string> topics;

        private bool AddToTopics(string topic)
        {
            lock (topics)
            {
                if (!topics.Contains(topic))
                {
                    topics.Add(topic);
                    return true;
                }
                else
                    return false;
            }
        }
        public WCSMqttClient_WCS(string name = null)
        {
            if (name == null)
                this.Init("WCSMqttClient_WCS");
            else
                this.Init($"WCSMqttClient_WCS_{name}");

            _logger = LogManager.GetCurrentClassLogger();
            topics = new List<string>();
            ConnectAsync();
        }

        public async Task ReconnectAsync()
        {
            try
            {
                //Console.WriteLine("重连MQTTServer成功");
                this.Log($"WCSMqttClient_WCS 开始重连MQTTServer");
                await client.ReconnectAsync();
                this.Log($"WCSMqttClient_WCS 重连MQTTServer成功");
            }
            catch (Exception ex)
            {
                _logger.Error1(ex, this);
                this.Log($"WCSMqttClient_WCS 重连MQTTServer失败，异常消息{ex}");
            }
        }

        const string server = "server";
        const string port = "port";
        const string username = "username";
        const string password = "password";

        private async Task ConnectAsync()
        {
            factory = new MqttFactory();
            client = factory.CreateMqttClient();
            var mqttTcpServerInfo = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("mqttTcpClient_wcs", "server:127.0.0.1,port:1883");
            var serverInfos = mqttTcpServerInfo.Split(',').ToArray();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in serverInfos)
            {
                var arrs = item.Split(':').ToArray();
                if (arrs.Length == 2)
                    dic.Add(arrs[0], arrs[1]);
                else
                {
                    this.Log($"{item}无法解析");
                }
            }

            MqttClientOptionsBuilder mqttClientOptionsBuilder;
            if (dic.ContainsKey(server))
            {
                if (dic.ContainsKey(port))
                    mqttClientOptionsBuilder = new MqttClientOptionsBuilder().WithTcpServer(dic[server], int.Parse(dic[port]));
                else
                    mqttClientOptionsBuilder = new MqttClientOptionsBuilder().WithTcpServer(dic[server]);
            }
            else
            {
                this.Log($"WCSMqttClient_WCS mqtt配置错误");
                throw new AggregateException("WCSMqttClient_WCS mqtt配置错误");
            }
            if (dic.ContainsKey(username) && dic.ContainsKey(password) && !string.IsNullOrWhiteSpace(dic[username]) && !string.IsNullOrWhiteSpace(dic[password]))
                mqttClientOptionsBuilder.WithCredentials(dic[username], dic[password]);

            options = mqttClientOptionsBuilder.Build();

            client.ConnectingAsync += Client_ConnectingAsync;
            client.ConnectedAsync += Client_ConnectedAsync;
            client.DisconnectedAsync += Client_DisconnectedAsync;
            client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;

            await client.ConnectAsync(options);
            Console.WriteLine($"WCSMqttClient_WCS 服务端连接成功，连接信息：\r\n{options}");
            this.Log($"WCSMqttClient_WCS 服务端连接成功，连接信息：\r\n{options}");
        }

        public bool IsConnected
        {
            get
            {
                if (client == null || !client.IsConnected)
                    return false;
                else
                    return true;
            }
        }

        private async Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                var receivedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                //Console.WriteLine($"---------------{receivedAt}---------------");
                var payloadText = string.Empty;
                if (arg.ApplicationMessage.PayloadSegment.Count > 0)
                {
                    payloadText = Encoding.UTF8.GetString(
                        arg.ApplicationMessage.PayloadSegment.Array,
                        arg.ApplicationMessage.PayloadSegment.Offset,
                        arg.ApplicationMessage.PayloadSegment.Count);
                }

                this.Log($"---------------{receivedAt}---------------\r\n### RECEIVED APPLICATION MESSAGE ###\r\n+ Topic = {arg.ApplicationMessage.Topic}\r\n+ Payload = {payloadText}\r\n+ QoS = {arg.ApplicationMessage.QualityOfServiceLevel}\r\n+ Retain = {arg.ApplicationMessage.Retain}");
                //Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                //Console.WriteLine($"+ Topic = {arg.ApplicationMessage.Topic}");
                //Console.WriteLine($"+ Payload = {payloadText}");
                //Console.WriteLine($"+ QoS = {arg.ApplicationMessage.QualityOfServiceLevel}");
                //Console.WriteLine($"+ Retain = {arg.ApplicationMessage.Retain}");
            }
            catch (Exception ex)
            {
                this.Log($"WCSMqttClient_WCS 接收失败，异常消息:{ex}");
                _logger.Error1(ex, this);
            }
            //return null;
        }

        private async Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            try
            {
                Console.WriteLine($"WCSMqttClient_WCS 由于 {arg.Reason} 已断开连接");
                System.Diagnostics.Debug.WriteLine($"WCSMqttClient_WCS 由于 {arg.Reason} 已断开连接");
                // 你可以在这里实现重连逻辑
                // 例如，在一段时间后尝试重新连接
                Task.Delay(1000).Wait(); // 等待5秒
                ReconnectAsync().Wait();
            }
            catch (Exception ex)
            {
                this.Log($"WCSMqttClient_WCS 已断开连接，发生异常{ex}");
                _logger.Error1(ex, this);
                //Thread.Sleep(3000);
            }
        }

        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            try
            {
                Console.WriteLine("WCSMqttClient_WCS 已连接MQTTServer");
                this.Log($"WCSMqttClient_WCS 已连接MQTTServer");
                //_logger.Info1("已连接MQTTServer", this);
            }
            catch (Exception ex)
            {
                this.Log($"WCSMqttClient_WCS 连接失败，异常消息：{ex}");
                _logger.Error1(ex, this);
            }
        }

        private async Task Client_ConnectingAsync(MqttClientConnectingEventArgs arg)
        {
            try
            {
                Console.WriteLine("WCSMqttClient_WCS 开始连接MQTTServer");
                this.Log($"WCSMqttClient_WCS 开始连接MQTTServer");
                //_logger.Info1("开始连接MQTTServer", this);
            }
            catch (Exception ex)
            {
                this.Log($"WCSMqttClient_WCS 连接失败，异常消息：{ex}");
                _logger.Error1(ex, this);
            }
        }

        object mqttSendLocker = new object();
        public async Task SendMqttMsgAsync(string topic, string mqttMsg, bool RetainedMessage = false)
        {
            if (this.client.IsConnected)
            {
                await client.PublishStringAsync(topic, mqttMsg, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, RetainedMessage);
                this.Log($"WCSMqttClient_WCS 发布异步消息成功：\r\n{topic}\r\n{mqttMsg}");
            }
            else
            {
                this.Log($"WCSMqttClient_WCS 发布异步消息失败：mqttClient未连接");
                throw new InvalidOperationException("WCSMqttClient_WCS mqttClient未连接");
            }
        }
    }
}
