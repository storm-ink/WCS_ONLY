using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class SettingCollection:ConfigurationElement
    {
        public Dictionary<String, String> _settings = new Dictionary<string, string>();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public SettingCollection(XmlNode node, WcsConfiguration configuration)
            : base(node,configuration, true)
        {

        }


        protected override void Deserialize()
        {
            foreach (XmlNode node in this.Node.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                _settings[node.Name] = node.InnerText;
            }
        }

        /// <summary>
        /// 通过 <see cref="T:Wcs.Framework.Cfg.Configuration.GetSetting"/> 方法获取配置数据缓存，用于减少 IO 读取.<br />
        /// 缓存内数据值在 <see cref="T:Wcs.Framework.Cfg.Configuration.SetSetting"/> 时将被更新.
        /// </summary>
        static Dictionary<string, dynamic> settingsCahce = new Dictionary<string, dynamic>();
        /// <summary>
        /// 从 settings 节获取指定名称节点的 InnerText 值
        /// </summary>
        /// <param name="settingName">  节点名. </param>
        /// <param name="defaultValue"> 在节点为 null 时，指定返回的默认值. </param>
        public T GetSetting<T>(string settingName, T defaultValue=default(T))
        {
            lock (settingsCahce)
            {
                if (settingsCahce.ContainsKey(settingName))
                {
                    return settingsCahce[settingName];
                }

                var formatSettingName= FormatXPath(settingName);

                if (this.Node.SelectNodes(formatSettingName).Count > 1)
                {
                    var path = this.Node.GetXPath();
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点存在多个 {1} 子节点，系统将以第一个 {1} 节点配置为准", path, formatSettingName), this);
                }

                T result;
                var node = this.Node.SelectSingleNode(formatSettingName);
                if (node == null)
                {
                    SetSetting<T>(settingName, defaultValue);
                    result = defaultValue;
                }
                else
                {
                    result = Utils.ConvertTo<T>(node.InnerText);
                }

                settingsCahce[settingName] = result;

                return result;
            }
        }
        /// <summary>
        /// 设置 settings 指定名称节点的 InnerText 值
        /// </summary>
        /// <param name="settingName">  节点名. </param>
        /// <param name="value">        值(String 表现形式). </param>
        public  void SetSetting<T>(string settingName, T value)
        {
            
            lock (settingsCahce)
            {
                settingName = FormatXPath(settingName);

                if (this.Node.SelectNodes(settingName).Count > 1)
                {
                    var path=this.Node.GetXPath();
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点存在多个 {1} 子节点，系统将以第一个 {1} 节点配置为准", path, settingName),this);
                }

                var settingNode = this.Node.SelectSingleNode(settingName);
                if (settingNode == null)
                {
                    string path = "";
                    var rootNode=this.Node;
                    foreach (var item in settingName.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            path = item;
                        }
                        else
                        {
                            path = path + "/" + item;
                        }
                        settingNode = this.Node.SelectSingleNode(path);
                        if (settingNode == null)
                        {
                            settingNode = this.Node.OwnerDocument.CreateElement(item);
                            rootNode.AppendChild(settingNode);
                        }
                        rootNode = settingNode;
                    }
                }

                settingNode.InnerText = Convert.ToString(value) ?? "";

                settingNode.OwnerDocument.Save(this.ConfigurationFileName);

                settingsCahce[settingName] = value;
            }
        }

        static String FormatXPath(String xpath)
        {
            xpath = xpath.Replace("(", "")
                        .Replace(")", "")
                        .Replace("~", "")
                        .Replace("!", "")
                        .Replace("#", "")
                        .Replace("$", "")
                        .Replace("%", "")
                        .Replace("^", "")
                        .Replace("&", "")
                        .Replace("*", "")
                        .Replace("、", "");

            if (!System.Text.RegularExpressions.Regex.IsMatch(xpath, @"(/)?(\d+).*?"))
            {
                return xpath;
            }

            List<String> paths = new List<string>();
            foreach (var item in xpath.Split(new string[] { @"\", @"/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var step1Pattern = @"^(?<r>\d+)(?<c>.*)";
                if (System.Text.RegularExpressions.Regex.IsMatch(item, step1Pattern))
                {
                    var v = System.Text.RegularExpressions.Regex.Replace(item, step1Pattern, "_${r}${c}");
                    paths.Add(v);
                }
                else
                {
                    paths.Add(item);
                }
            }

            return string.Join(@"/", paths);
        }
    }
}
