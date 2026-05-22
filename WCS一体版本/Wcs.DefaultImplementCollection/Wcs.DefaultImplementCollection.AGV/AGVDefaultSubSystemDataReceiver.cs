using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Wcs.Framework;
using System.Text.RegularExpressions;
using Wcs;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class AGVDefaultSubSystemDataReceiver:IDataReceiver
    {
        List<byte> _bytes = new List<byte>();
        Logger _logger { get; set; }
        public string Name { get; private set; }

        public string DeviceName { get; set; }

        public TNetTransferObject ConvertToNetTransferObject<TNetTransferObject>(NetPacket netPacket) where TNetTransferObject : NetTransferObject
        {
            throw new NotImplementedException();
        }
        //netTransferObject强制转化为AGVSubSystemNetPacket
        public NetPacket ConvertToNetPacket<TNetTransferObject>(TNetTransferObject netTransferObject) where TNetTransferObject : NetTransferObject
        {
            if (!(netTransferObject is AGVSubSystemTelexTransferObject))
            {
                throw new InvalidOperationException(string.Format("只支持 {0} 类型的转换", typeof(AGVSubSystemTelexTransferObject)));
            }

            AGVSubSystemTelexTransferObject telexTransferObject = netTransferObject as AGVSubSystemTelexTransferObject;
            return new AGVSubSystemNetPacket(telexTransferObject.GetBytes());
        }

        public void AddBytes(byte[] bytes)
        {
            _bytes.AddRange(bytes);
            //获取完整的报文对象 telexObjects为一个完整的报文对象
            var telexObjects = FetchTelexTransferObject();

            if (telexObjects!=null && telexObjects.Count > 0 && DataReceived != null)
            {
                foreach (var item in telexObjects)
                {
                    DataReceived(this, new DataReceiverReceivedEventArgs(null, item));
                }
            }
        }
        //移除所有元素
        public void Clear()
        {
            _bytes.Clear();
        }

        public event EventHandler<DataReceiverReceivedEventArgs> DataReceived;

        Type[] TelexTypes { get; set; }

        public Device Device => throw new NotImplementedException();

        //agv默认子系统数据接收器
        public AGVDefaultSubSystemDataReceiver(string name)
        {
            this.Name = name;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 尝试从当前已接收的数据区获取一个完整的报文对象
        /// </summary>
        /// <returns></returns>
        List<AGVSubSystemTelexTransferObject> FetchTelexTransferObject()
        {
            if (_bytes.Count < 2)
            {
                return null;
            }

            var telexValues = System.Text.Encoding.ASCII.GetString(_bytes.ToArray());

            MatchCollection mc = Regex.Matches(telexValues.ToString(), string.Format(@"\u{0:d4}([^\u{0:d4}\u{1:d4}]*)\u{1:d4}",Convert.ToInt32(AGVSubSystemTelexTransferObject.Prefix),Convert.ToInt32(AGVSubSystemTelexTransferObject.Suffix)));

            List<AGVSubSystemTelexTransferObject> result = new List<AGVSubSystemTelexTransferObject>();

            foreach (Match m in mc)
            {
                string telex = m.Groups[0].Value;
                if (string.IsNullOrEmpty(telex)) continue;

                var telexObject = ParseTelexTransferObject(telex);

                if (telexObject == null)
                {
                    continue;
                }

                result.Add(telexObject);
            }

            if (mc.Count > 0)
            {
                Match m = mc.Cast<Match>().Last();
                telexValues = telexValues.Substring(m.Index + m.Value.Length);

                _bytes = System.Text.Encoding.ASCII.GetBytes(telexValues).ToList();
            }

            return result;
        }
        //解析报文
        AGVSubSystemTelexTransferObject ParseTelexTransferObject(String telex)
        {
            telex = telex.Replace((char)0, '0');

            Console.WriteLine(telex);
            System.Diagnostics.Debug.WriteLine(telex);
            foreach (var type in TelexTypes)
            {
                var instance = ReflectionHelper.CreateInstance<AGVSubSystemTelexTransferObject>(type);
                if (instance.ValidateType(telex))
                {
                    instance = ReflectionHelper.CreateInstance<AGVSubSystemTelexTransferObject>(type, telex);
                    return instance;
                }
            }

            this._logger.Warn1(String.Format("未能识别的报文内容 {0}", telex), this, _bytes);

            return null;
        }

        public void Log(string msg)
        {
            throw new NotImplementedException();
        }
    }
}
