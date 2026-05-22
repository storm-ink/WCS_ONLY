using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Wcs
{
    public static class XmlFileExtentions
    {
        public static string XmlSerialize<T>(this T obj) where T : class
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj,ns);
                sw.Close();
                return sw.ToString();
            }
        }

        public static T DESerializer<T>(this XmlDocument xmlDocument, string strXML) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
