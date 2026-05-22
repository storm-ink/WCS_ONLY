using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using DevExpress.XtraBars.Ribbon;

namespace Wcs
{
    /// <summary>
    /// Wcs上下文对象
    /// </summary>
    public class WcsContext
    {
        /// <summary>
        /// Wcs主应用程序
        /// </summary>
        public IWcsApplication Application { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="application">Wcs主应用程序</param>
        public WcsContext(IWcsApplication application)
        {
            this.Application = application;
        }
        List<WcsPlugin> m_LoadedPlugins = new List<WcsPlugin>();

        /// <summary>
        /// 获取已加载的插件列表
        /// </summary>
        public WcsPlugin[] LoadedPlugins
        {
            get
            {
                return m_LoadedPlugins.ToArray();
            }
        }
        public List<KeyValuePair<Type, WcsPluginInfo>> result;
        /// <summary>
        /// 在指定的目录及其子目录中查找所有插件
        /// </summary>
        /// <param name="dir">指定的目录</param>
        /// <returns></returns>
        public List<KeyValuePair<Type, WcsPluginInfo>> FindPlugins(string dir)
        {
            if (result != null)
                return result;
            result = new List<KeyValuePair<Type, WcsPluginInfo>>();
            foreach (var file in Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    foreach (Type type in Assembly.LoadFile(file).GetTypes())
                    {
                        if (type.IsPublic && type.IsSubclassOf(typeof(WcsPlugin)))
                        {
                            var attr = (WcsPluginInfo)type.GetCustomAttributes(typeof(WcsPluginInfo), false).FirstOrDefault();
                            if (attr == null) attr = new WcsPluginInfo(type, "<未配置>", "<未配置>", "<未配置>", "<未配置>", "<未配置>", "<未配置>", 0, 0, 0);

                            result.Add(new KeyValuePair<Type, WcsPluginInfo>(type, attr));
                        }
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    this.Application.Logger.Error1(badImageFormatException, this);
                    //throw new Exception(String.Format("在加载 {0} 时发生异常", file), badImageFormatException);
                }
                catch (ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    String loaderExceptions = null;
                    if (reflectionTypeLoadException.LoaderExceptions != null && reflectionTypeLoadException.LoaderExceptions.Length != 0)
                    {
                        loaderExceptions = string.Join("\n■ ", reflectionTypeLoadException.LoaderExceptions.Select(x => x.Message).ToArray());
                    }
                    //= reflectionTypeLoadException.LoaderExceptions;
                    throw new Exception(String.Format("在加载 {0} 时发生了 {1} 异常。以下是 LoaderExceptions 清单：\n{2}", file, reflectionTypeLoadException.GetType(), loaderExceptions), reflectionTypeLoadException);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("在加载 {0} 时发生异常", file), ex);
                }
            }

            foreach (var file in Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories))
            {
                try
                {
                    foreach (Type type in Assembly.LoadFile(file).GetTypes())
                    {
                        if (type.IsPublic && type.IsSubclassOf(typeof(WcsPlugin)))
                        {
                            var attr = (WcsPluginInfo)type.GetCustomAttributes(typeof(WcsPluginInfo), false).FirstOrDefault();
                            if (attr == null) attr = new WcsPluginInfo(type, "<未配置>", "<未配置>", "<未配置>", "<未配置>", "<未配置>", "<未配置>", 0, 0, 0);

                            result.Add(new KeyValuePair<Type, WcsPluginInfo>(type, attr));
                        }
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    throw new Exception(String.Format("在加载 {0} 时发生异常", file), badImageFormatException);
                }
            }

            return result.OrderBy(x => x.Value.Priority).ToList();
        }

        /// <summary>
        /// 加载指定的动态链接库或应用程序中的插件对象
        /// </summary>
        /// <param name="path">指定的包含插件对象的动态链接库或应用程序</param>
        public void LoadPlugin(string path)
        {
            if (LoadedPlugins.Any(x => string.Equals(x.PluginInfo.Path, path, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new Exception(string.Format("请勿重复已加载插件 {0}", path));
            }

            if (!File.Exists(path))
            {
                throw new Exception(string.Format("未找到插件 {0}", path));
            }

            lock (m_LoadedPlugins)
            {
                foreach (Type type in Assembly.LoadFile(path).GetTypes())
                {
                    if (type.IsPublic && type.IsSubclassOf(typeof(WcsPlugin)))
                    {
                        try
                        {
                            WcsPlugin wcsPlugin = (WcsPlugin)type.Assembly.CreateInstance(type.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, null, null, null);
                            try
                            {
                                wcsPlugin.Initialization(this);

                                m_LoadedPlugins.Add(wcsPlugin);
                            }
                            catch (Exception ex)
                            {
                                Application.Logger.Error1(new Exception(string.Format("插件 {0} 在初始化时失败", wcsPlugin.GetType().FullName), ex), this);
                            }
                        }
                        catch (Exception ex)
                        {
                            Application.Logger.Error1(ex, this);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 加载指定类型的插件对象
        /// </summary>
        /// <param name="wcsPluginType">插件类型</param>
        public void LoadPlugin(Type wcsPluginType)
        {
            lock (m_LoadedPlugins)
            {
                try
                {
                    WcsPlugin wcsPlugin = (WcsPlugin)wcsPluginType.Assembly.CreateInstance(wcsPluginType.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, null, null, null);

                    try
                    {

                        wcsPlugin.Initialization(this);
                        m_LoadedPlugins.Add(wcsPlugin);
                    }
                    catch (Exception ex)
                    {
                        Application.Logger.Error1(ex, this);
                        Application.Logger.Error1(new Exception(string.Format("插件 {0} 在初始化时失败", wcsPlugin.GetType().FullName), ex), this);
                    }
                }
                catch (Exception ex)
                {
                    Application.Logger.Error1(ex, this);
                }
            }
        }
        /// <summary>
        /// 卸载指定类型的插件
        /// </summary>
        /// <param name="pluginType">指定的插件类型</param>
        public void UnloadPlugin(Type pluginType)
        {
            WcsPlugin plugin = LoadedPlugins.Where(x => x.PluginInfo.PluginType == pluginType).FirstOrDefault();

            if (plugin == null)
            {
                throw new Exception(string.Format("未找到类型为 {0} 的插件对象", pluginType.FullName));
            }

            lock (m_LoadedPlugins)
            {
                m_LoadedPlugins.Remove(plugin);
                plugin.Dispose();
                plugin = null;
            }
        }
    }
}
