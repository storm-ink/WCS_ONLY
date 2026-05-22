using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs.Framework;
using Wcs.Framework.Devices;
using Wcs.Framework.Cfg;
using System.Diagnostics;
namespace Wcs.Framework
{
    /// <summary>
    /// 设备管理器
    /// </summary>
    public class DeviceManager
    {
        static DeviceManager _instance;
        public static DeviceManager GetInstance()
        {
            lock (typeof(DeviceManager))
            {
                if (_instance == null)
                {
                    _instance = new DeviceManager();
                }
            }

            return _instance;
        }

        public void Connect(Device device)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                ((Device)state).Connect();
            }, device);
        }

        public void Disconnect(Device device)
        {
            device.Disconnect();
        }

        public void ConnectAllDevice()
        {
            foreach (var device in Devices)
            {
                Connect(device);
            }
        }

        private DeviceManager()
        {
            foreach (var device in Devices)
            {
                device.Disconnected += (sender, args) =>
                {
                    String msg = String.Format("{0} 由于 {1} 原因，已断开连接", sender,args.Reason);
                    Debug.WriteLine(msg);
                    if (args.Reason == DeviceDisconnectReason.User)
                    {
                        msg = String.Format("{0} 已被用户断开连接", sender);
                        Debug.WriteLine(msg);
                    }
                    else
                    {
                        int connectionRetires = ((Device)sender).ConnectionRetries;

                        //最小 5 秒间隔，最大 60 秒间隔
                        int interval = connectionRetires * 5000;
                        if (interval < 0)
                        {
                            interval = 5000;
                        }

                        if (interval > 5000 * 12)
                        {
                            interval = 5000 * 12;
                        }

                        ((Device)sender).Logger.Warning(String.Format("{0} 已断开连接，{1} 秒钟后将重新尝试连接.", sender, interval/1000), this, sender);
                        Thread.Sleep(interval);
                        ((Device)sender).Connect();
                        //ThreadPool.QueueUserWorkItem((state) =>
                        //{
                        //    ((Device)sender).Logger.Warning(String.Format("{0} 已断开连接，5秒钟后将重新尝试连接.", sender), this, sender);
                        //    Thread.Sleep(5000);
                        //    ((Device)sender).Connect();
                        //});
                   
                    }
                };


                device.Connected += (sender, args) =>
                {
                    String msg = String.Format("{0} 已连接", sender);
                    Debug.WriteLine(msg);
                };
            }
        }
        /// <summary>
        /// 所有设备集合
        /// </summary>
        public Device[] Devices
        {
            get
            {
               return Configuration.Devices;
            }
        }
        /// <summary>
        /// 获取所有空闲的设备
        /// </summary>
        /// <returns></returns>
        public Device[] FindIdleDevices()
        {
            return Devices
                .Where(x => x.IsIdle)
                .ToArray();
        }
    }
}
