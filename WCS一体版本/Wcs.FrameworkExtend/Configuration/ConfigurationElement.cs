using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.FrameworkExtend.Cfg
{
    /// <summary>
    /// 配置节点，配置文件中所有需要解析的节点都必须继承此类。
    /// </summary>
    public abstract class ConfigurationElement
    {
        /// <summary>
        /// XmlFile 属性的属性名
        /// </summary>
        public const String XmlFileAttributeName = "xmlFile";
        /// <summary>
        /// 获取配置节点
        /// </summary>
        public XmlNode Node {get;private set;}
        public WcsConfiguration WcsConfiguration { get; private set; }
        /// <summary>
        /// 获取配置文件完整路径<br />
        /// 配置文件节点允许将节点映射到另一个独立的配置文件中<br />
        /// 如果配置了该属性，将直接从属性中指定的文件中读取配置内容
        /// </summary>
        public String XmlFile { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public ConfigurationElement(XmlNode node,WcsConfiguration configuration)
            : this(node,configuration, true)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deserialize">是否立即解析</param>
        public ConfigurationElement(XmlNode node, WcsConfiguration configuration, Boolean deserialize)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.WcsConfiguration = configuration;
            Node = GetNodeOrLoadXmlFile(node);
            if (deserialize)
            {
                Deserialize();
            }
        }
        /// <summary>
        /// 执行解析。该方法将在构造函数完成后被调用。
        /// </summary>
        /// <param name="node">配置节点</param>
        protected abstract void Deserialize();

        /// <summary>
        /// 分析指定的节点是否需要重新从外部文件加载，并返回加载后的节点对象
        /// </summary>
        /// <param name="node">
        /// 指定的节点<br />
        /// 如果 node 节点存在 xmlFile 属性，并且不为空，则将使用属性值指向的文件加载新的XmlDocument，并返回其 DocumentNode 对象。<br />
        /// 路径说明：如果是绝对路径将直接读取其指向的文件；否则，读取应用程序所在目录的相对路径指向的文件。
        /// </param>
        /// <returns></returns>
        public XmlNode GetNodeOrLoadXmlFile(XmlNode node)
        {
            String xmlFile = GetAttributeOrDefault<String>(ConfigurationElement.XmlFileAttributeName, node);
            if (!string.IsNullOrWhiteSpace(xmlFile))
            {
                WcsConfiguration._logger.Trace1(string.Format("{0} 节点被重定向到了 {1} 文件", node.GetXPath(), xmlFile),this);
                if (!Path.IsPathRooted(xmlFile))
                {
                    var path = new Uri(typeof(Wcs.Framework.Cfg.ConfigurationElement).Assembly.CodeBase).LocalPath;
                    xmlFile = Path.Combine(Path.GetDirectoryName(path), xmlFile);
                }

                XmlDocument xml = new XmlDocument();
                xml.Load(xmlFile);
                node = xml.DocumentElement;
            }

            return node;
        }

        protected XmlNode GetOrGenerateNode(XmlNode parent, String nodeName)
        {
            XmlNode node = parent.SelectSingleNode(nodeName);
            if (node == null)
            {
                node = parent.OwnerDocument.CreateElement(nodeName);
                parent.AppendChild(node);
            }

            return node;
        }

        /// <summary>
        /// 从配置节点中获取指定名称的属性,不存在时将引发异常
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">指定名称的属性不存在或属性值无法转换为 T 类型</exception>
        /// <returns></returns>
        public T GetAttribute<T>(string attrName,XmlNode node)
        {
            var attr = node.Attributes[attrName];
            if (attr == null)
            {
                var path = node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点未找到 {1} 属性", path, attrName),node);
            }
            try
            {
                return ConvertTo<T>(attr.Value);
            }
            catch (Exception ex)
            {
               var path = node.GetXPath();
               throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 无法转换为 {3} 类型", path, attrName,attr.Value,typeof(T)),ex,node);
            }
        }
 
        /// <summary>
        /// 从配置节点中获取指定名称的属性,不存在时将引发异常
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">指定名称的属性不存在或属性值无法转换为 T 类型</exception>
        /// <returns></returns>
        public T GetAttribute<T>(string attrName)
        {
            return GetAttribute<T>(attrName, this.Node);
        }

        /// <summary>
        /// 从配置节点中获取指定名称的属性,不存在时将返回指定的默认值
        /// </summary>
        /// <typeparam name="T">属性名称</typeparam>
        /// <param name="attrName"></param>
        /// <param name="defaultValue">属性不存在时返回的默认值</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">属性值无法转换为 T 类型</exception>
        /// <returns></returns>
        public T GetAttributeOrDefault<T>(string attrName, T defaultValue=default(T))
        {
            return GetAttributeOrDefault<T>(attrName,this.Node, defaultValue);
        }

        /// <summary>
        /// 从配置节点中获取指定名称的属性,不存在时将返回指定的默认值
        /// </summary>
        /// <typeparam name="T">属性名称</typeparam>
        /// <param name="attrName"></param>
        /// <param name="defaultValue">属性不存在时返回的默认值</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">属性值无法转换为 T 类型</exception>
        /// <returns></returns>
        public T GetAttributeOrDefault<T>(string attrName, XmlNode node, T defaultValue = default(T))
        {
            var attr = node.Attributes[attrName];
            if (attr == null)
            {
                return defaultValue;
            }

            try
            {
                return ConvertTo<T>(attr.Value);
            }
            catch (Exception ex)
            {
                var path = node.GetXPath();
                throw new System.Configuration.ConfigurationErrorsException(string.Format("{0} 节点 {1} 属性值 “{2}” 无法转换为 {3} 类型", path, attrName, attr.Value, typeof(T)), ex, node);
            }
        }
        

        /// <summary>
        /// 将指定的数据转为指定类型
        /// </summary>
        /// <typeparam name="T">    泛型类型参数. </typeparam>
        /// <param name="value">    要转换的数据. </param>
        T ConvertTo<T>(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return default(T);

            Type conversionType = typeof(T);
            if (conversionType.IsGenericType &&
                conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                System.ComponentModel.NullableConverter nullableConverter
                    = new System.ComponentModel.NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;

                //如果是枚举
                if (typeof(T).GetGenericArguments()[0].IsEnum)
                {
                    if (value != null && !Convert.IsDBNull(value))
                    {
                        return (T)Enum.Parse(typeof(T).GetGenericArguments()[0], Convert.ToString(value));
                    }
                }
            }
            else
            {
                if (typeof(T).IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), Convert.ToString(value));
                }
            }


            return (T)Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        /// 获取当前节点所属的配置文件
        /// </summary>
        public String ConfigurationFileName
        {
            get
            {
                String fileName = this.Node.BaseURI;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var fileAttr = this.Node.GetType().GetField("_filename", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    fileName = (String)fileAttr.GetValue(this.Node);
                }
                else
                {
                    fileName = new Uri(fileName).LocalPath;
                }

                return fileName;
            }
        }
    }
}
