using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs
{
    /// <summary>
    /// 插件信息
    /// </summary>
    public class WcsPluginInfo : Attribute
    {
        String m_PluginId;
        int? m_Priority;
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatedBy { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedAt { get; private set; }
        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public Version Version
        {
            get
            {
                return PluginType.Assembly.GetName().Version;
            }
        }
        /// <summary>
        /// 插件所在的完整路径
        /// </summary>
        public string Path
        {
            get
            {
                return PluginType.Module.FullyQualifiedName;
            }
        }
        /// <summary>
        /// 获取插件 Id
        /// </summary>
        public String PluginId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_PluginId))
                {
                    m_PluginId = ((WcsPlugin)PluginType.Assembly.CreateInstance(PluginType.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, null, null, null)).Id;
                }
                return m_PluginId;
            }
        }
        /// <summary>
        /// 获取插件的加载优先级
        /// </summary>
        public int Priority
        {
            get
            {
                if (m_Priority == null)
                {
                    m_Priority = ((WcsPlugin)PluginType.Assembly.CreateInstance(PluginType.FullName, false, System.Reflection.BindingFlags.CreateInstance, null, null, null, null)).Priority;
                }

                return m_Priority.Value;
            }
        }
        /// <summary>
        /// 所属插件对象类型
        /// </summary>
        public Type PluginType { get; private set; }
        /// <summary>
        /// 指示是否为系统核心插件（核心插件无法被卸载）
        /// </summary>
        public bool IsCore { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginType">插件类型</param>
        /// <param name="name">插件名称</param>
        /// <param name="cratedBy">创建人</param>
        /// <param name="createAt">创建时间</param>
        /// <param name="description">描述信息</param>
        /// <param name="isCore">是否是核心插件</param>
        public WcsPluginInfo(Type pluginType, string name, string cratedBy, string createAt, string description, bool isCore, string ribbonPage, string ribbonPageGroup, int ribbonPageIndex, int ribbonPageGroupIndex, int ribbonPageItemIndex)
        {
            this.PluginType = pluginType;
            this.Name = name;
            this.CreatedAt = createAt;
            this.CreatedBy = cratedBy;
            this.Description = description;
            this.IsCore = isCore;
            this.DevexpressRibbonInfo = new DevexpressRibbon() 
            { 
                RibbonPage = ribbonPage, 
                RibbonPageGroup = ribbonPageGroup,
                RibbonPageIndex = ribbonPageIndex,
                RibbonPageGroupIndex = ribbonPageGroupIndex,
                RibbonPageItemIndex = ribbonPageItemIndex 
            };
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginType">插件类型</param>
        /// <param name="name">插件名称</param>
        /// <param name="cratedBy">创建人</param>
        /// <param name="createAt">创建时间</param>
        /// <param name="description">描述信息</param>
        public WcsPluginInfo(Type pluginType, string name, string cratedBy, string createAt, string description, string ribbonPage, string ribbonPageGroup, int ribbonPageIndex, int ribbonPageGroupIndex, int ribbonPageItemIndex)
            : this(pluginType, name, cratedBy, createAt, description, false, ribbonPage, ribbonPageGroup, ribbonPageIndex, ribbonPageGroupIndex, ribbonPageItemIndex)
        {
        }

        /// <summary>
        /// Ribbon 界面属性
        /// </summary>
        public DevexpressRibbon DevexpressRibbonInfo { get; private set; }
    }
}
