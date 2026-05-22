using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane_yc
{
    public sealed class RackLocationCollection:Wcs.Framework.Cfg.ParticularLocationCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RackLocationCollection(XmlNode node, WcsConfiguration configuration)
            : base(node, configuration, true)
        {
            this._leftLine = GetAttributeOrDefault<Int32>("_leftLine");
            this._rightLine = GetAttributeOrDefault<Int32>("_rightLine");
            this._columnCount = GetAttributeOrDefault<Int32>("_columnCount");
            this._levelCount = GetAttributeOrDefault<Int32>("_levelCount");

            int[] props = new[] { this._leftLine, this._rightLine, this._columnCount, this._levelCount };
            if (!props.All(x => x == 0) && !props.All(x => x > 0))
            {
                throw new ConfigurationErrorsException("如果要使用模板参数生成货位则必须同时指定 _leftLine、_rightLine、_columnCount、_levelCount 四个属性值，且不能小于 0");
            }

            //模板生成
            List<LocationElement> locationElements = new List<LocationElement>();
            //左排
            for (int i = 1; i <= this._columnCount; i++)
            {
                for (int j = 1; j <= this._levelCount; j++)
                {
                    XmlNode locationElement = node.OwnerDocument.CreateElement("location");
                    var userCodeAttr = node.OwnerDocument.CreateAttribute("userCode");
                    userCodeAttr.Value = string.Format("{0:00}-{1:000}-{2:000}", this._leftLine, i, j);
                    locationElement.Attributes.Append(userCodeAttr);

                    var columnAttr = node.OwnerDocument.CreateAttribute("column");
                    columnAttr.Value = i.ToString();
                    locationElement.Attributes.Append(columnAttr);

                    var levelAttr = node.OwnerDocument.CreateAttribute("level");
                    levelAttr.Value = j.ToString();
                    locationElement.Attributes.Append(levelAttr);

                    var forkDirectionAttr = node.OwnerDocument.CreateAttribute("forkDirection");
                    forkDirectionAttr.Value = ForkDirection.Left.ToString();
                    locationElement.Attributes.Append(forkDirectionAttr);

                    locationElements.Add(CreateLocationElement(locationElement));
                    //WcsConfiguration.Logger.Trace1(string.Format("生成货架位置节点 {0}", locationElement.OuterXml), this, locationElement);
                }
            }
            //右排
            for (int i = 1; i <= this._columnCount; i++)
            {
                for (int j = 1; j <= this._levelCount; j++)
                {
                    XmlNode locationElement = node.OwnerDocument.CreateElement("location");
                    var userCodeAttr = node.OwnerDocument.CreateAttribute("userCode");
                    userCodeAttr.Value = string.Format("{0:00}-{1:000}-{2:000}", this._rightLine, i, j);
                    locationElement.Attributes.Append(userCodeAttr);

                    var columnAttr = node.OwnerDocument.CreateAttribute("column");
                    columnAttr.Value = i.ToString();
                    locationElement.Attributes.Append(columnAttr);

                    var levelAttr = node.OwnerDocument.CreateAttribute("level");
                    levelAttr.Value = j.ToString();
                    locationElement.Attributes.Append(levelAttr);

                    var forkDirectionAttr = node.OwnerDocument.CreateAttribute("forkDirection");
                    forkDirectionAttr.Value = ForkDirection.Right.ToString();
                    locationElement.Attributes.Append(forkDirectionAttr);

                    locationElements.Add(CreateLocationElement(locationElement));
                    //WcsConfiguration.Logger.Trace1(string.Format("生成货架位置节点 {0}", locationElement.OuterXml), this, locationElement);
                }
            }

            this.LocationElements = this.LocationElements.Concat(locationElements).ToArray();

            WcsConfiguration._logger.Debug1(string.Format("次本使用模块参数生成了 {0} 个货架位置", locationElements.Count), this);
        }

        public Int32 _columnCount { get; private set; }

        public Int32 _leftLine { get; private set; }
        public Int32 _levelCount { get; private set; }

        public Int32 _rightLine { get; private set; }
        public override Framework.Cfg.LocationElement CreateLocationElement(System.Xml.XmlNode LocationNode)
        {
            return new RackLocationElement(LocationNode,this, this.WcsConfiguration);
        }
    }
}
