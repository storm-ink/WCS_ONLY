using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZHQXC
{
    public class WcsConfigurationConnectionStrings
    {
        public static String WcsConnectionString
        {
            get
            {
                String currentDir = Path.GetDirectoryName(typeof(WcsConfigurationConnectionStrings).Assembly.Location);
                String wcsConfigPath = Path.Combine(currentDir, "Wcs.App.exe.config");

                if (File.Exists(wcsConfigPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(wcsConfigPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("configuration/cfg:hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                String hibernateCfgPath = Path.Combine(currentDir, "hibernate.cfg.xml");
                if (File.Exists(hibernateCfgPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(hibernateCfgPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("cfg:hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                return System.Configuration.ConfigurationManager.ConnectionStrings["wcs"].ConnectionString;
            }
        }
        public static String WcsBakConnectionString
        {
            get
            {
                String currentDir = Path.GetDirectoryName(typeof(WcsConfigurationConnectionStrings).Assembly.Location);
                String wcsConfigPath = Path.Combine(currentDir, "Wcs.App.exe.config");

                if (File.Exists(wcsConfigPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(wcsConfigPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("configuration/backup-hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                String hibernateCfgPath = Path.Combine(currentDir, "hibernatebackup.cfg.xml");
                if (File.Exists(hibernateCfgPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(hibernateCfgPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("backup-hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                return System.Configuration.ConfigurationManager.ConnectionStrings["wcs_bak"].ConnectionString;
            }
        }

        public static String WcsLogsConnectionString
        {
            get
            {
                String currentDir = Path.GetDirectoryName(typeof(WcsConfigurationConnectionStrings).Assembly.Location);
                String wcsConfigPath = Path.Combine(currentDir, "Wcs.App.exe.config");

                if (File.Exists(wcsConfigPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(wcsConfigPath);
                    var node = doc.SelectSingleNode("configuration/nlog/targets/target[@type='Database']");
                    if (node != null && node.Attributes["connectionString"] != null && !String.IsNullOrWhiteSpace(node.Attributes["connectionString"].Value))
                    {
                        return node.Attributes["connectionString"].Value;
                    }
                }

                if (System.Configuration.ConfigurationManager.ConnectionStrings["wcs_logs"] != null)
                {
                    return System.Configuration.ConfigurationManager.ConnectionStrings["wcs_logs"].ConnectionString;
                }
                else
                {
                    throw new Exception("未配置wcs_logs连接字符串");
                }
            }
        }


        public static String DeviceTrackingDataConnectionString
        {
            get
            {
                String currentDir = Path.GetDirectoryName(typeof(WcsConfigurationConnectionStrings).Assembly.Location);
                String wcsConfigPath = Path.Combine(currentDir, "Wcs.App.exe.config");

                if (File.Exists(wcsConfigPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(wcsConfigPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("configuration/deviceTrackingData-hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                String hibernateCfgPath = Path.Combine(currentDir, "deviceTrackingData.cfg.xml");
                if (File.Exists(hibernateCfgPath))
                {
                    var doc = new XmlDocument();
                    doc.Load(hibernateCfgPath);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("cfg", "urn:nhibernate-configuration-2.2");
                    var node = doc.SelectSingleNode("deviceTrackingData-hibernate-configuration/cfg:session-factory[@name='wcs']/cfg:property[@name='connection.connection_string']", ns);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }

                if (System.Configuration.ConfigurationManager.ConnectionStrings["deviceTrackingData"] != null)
                {
                    return System.Configuration.ConfigurationManager.ConnectionStrings["deviceTrackingData"].ConnectionString;
                }
                else
                {
                    throw new Exception("未配置deviceTrackingData连接字符串");
                }
            }
        }
    }
}
