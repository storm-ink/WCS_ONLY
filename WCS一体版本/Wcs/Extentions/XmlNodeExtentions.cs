using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs
{
    public static class XmlNodeExtentions
    {
        /// <summary>
        /// Gets the X-Path to a given Node
        /// </summary>
        /// <param name="node">The Node to get the X-Path from</param>
        /// <returns>The X-Path of the Node</returns>
        public static string GetXPath(this XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Attribute)
            {
                // attributes have an OwnerElement, not a ParentNode; also they have             
                // to be matched by name, not found by position             
                return String.Format("{0}/@{1}", GetXPath(((XmlAttribute)node).OwnerElement), node.Name);
            }
            if (node.ParentNode == null)
            {
                // the only node with no parent is the root node, which has no path
                return "";
            }

            // Get the Index
            int indexInParent = 0;
            XmlNode siblingNode = node.PreviousSibling;
            // Loop thru all Siblings
            while (siblingNode != null)
            {
                // Increase the Index if the Sibling has the same Name
                if (siblingNode.Name == node.Name)
                {
                    indexInParent++;
                }
                siblingNode = siblingNode.PreviousSibling;
            }

            // the path to a node is the path to its parent, plus "/node()[n]", where n is its position among its siblings.         
            return String.Format("{0}/{1}[{2}]", GetXPath(node.ParentNode), node.Name, indexInParent);
        }
        /// <summary>
        /// xml格式化输出
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string FormatXml(this XmlNode xml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml.OuterXml);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xtw.Indentation = 1;
                xtw.IndentChar = '\t';
                //xtw.WriteFullEndElement();
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }
    }
}
