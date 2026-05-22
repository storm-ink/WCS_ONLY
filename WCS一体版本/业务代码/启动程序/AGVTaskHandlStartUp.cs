using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wcs;
using Wcs.DefaultImplementCollection.Conveyor;
using Wcs.Framework;
using Wcs.Framework.Cfg;
using ZHQXC.WebAPI.Entity;

namespace ZHQXC
{
    public class AGVTaskHandlStartUp : IApplicationStartup
    {
        static Thread _thread;

        Logger _logger;
        Location _start;

        Int32 _interval;
        Location _end;
        String _taskType;
        Wcs.Framework.Cfg.StartupElement _element;

        public void Initialize(Wcs.Framework.Cfg.StartupElement element)
        {
            _logger = LogManager.CreateNullLogger();
            _element = element;
            _interval = element.GetAttributeOrDefault<Int32>("interval", 2000);
        }

        public void Run(IWcsApplication application)
        {
            ParameterizedThreadStart start = new ParameterizedThreadStart(check);
            _thread = new Thread(start);
            _thread.IsBackground = true;
            _thread.Start();

            _logger.Info1($"{this} 线程已经启动", this);
        }

        public void check(object obj)
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("NLog.config");


            while (true)
            {
                Thread.Sleep(5000);
                try
                {
                    //var list = DbContext.LocalDb.Queryable<t_interface>().Where(a => a.state.Trim() == "1").ToList();
                    //Console.WriteLine("!!!!!!!!!!!!!!");

                    //var asm = Assembly.Load("Wcs.DefaultImplementCollection.AGV");
                    //foreach (var name in asm.GetManifestResourceNames())
                    //    Console.WriteLine(name);

                    //var asm1 = Assembly.Load("Wcs.DefaultImplementCollection.Robot");
                    //foreach (var name in asm1.GetManifestResourceNames())
                    //    Console.WriteLine(name);

                    //Console.WriteLine("!!!!!!!!!!!!!!");


                    //foreach (var item in list)
                    //{
                    //    if (result.IctResult)
                    //    {
                    //        DbContext.LocalDb.Updateable<t_interface>().SetColumns(it => new t_interface() { state = "4" }).Where(a => a.intercode == item.intercode).ExecuteCommand();
                    //        logger.Debug($"创建AGVTask:{item.intercode},From:{item.begionloc},To:{item.endloc}");
                    //    }
                    //    else
                    //    {
                    //        logger.Debug($"创建AGVTask失败:{item.intercode},From:{item.begionloc},To:{item.endloc},msg:{result.IctMsg}");
                    //    }
                    //}
                }
                catch (Exception ex)
                {

                }
            }
        }

    }

}