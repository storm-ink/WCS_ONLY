using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public class LocationCollection:ConfigurationElement
    {
        object locationsLocker = new object();
        System.Collections.ObjectModel.ReadOnlyCollection<Location> _locations;
        ReadOnlyDictionary<String, Location> _locationsDict;

        /// <summary>
        /// 位置节点集合
        /// </summary>
        public ParticularLocationCollection[] ParticularLocationCollection { get; private set; }

        /// <summary>
        /// 获取所有位置对象
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Location> Locations
        {
            get
            {
                lock (locationsLocker)
                {
                    if (_locations == null)
                    {
                        //_locations = new System.Collections.ObjectModel.ReadOnlyCollection<Location>(this.ParticularLocationCollection.SelectMany(x => x.LocationElements)
                        //    .Select(x => x.Location)
                        //    .ToArray());
                        _locations = new System.Collections.ObjectModel.ReadOnlyCollection<Location>(
                            this.ParticularLocationCollection.SelectMany(x => x.Locations).ToArray());
                    }
                }

                return _locations;
            }
        }

        /// <summary>
        /// 将所有货位转换为 Key 为系统可识别（可二次转换的）编码值，Value 为 Location 的字典对像，以供快速访问。
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<String, Location> AsDictionary()
        {
            lock (locationsLocker)
            {
                if (_locationsDict == null)
                {
                    _locationsDict = new ReadOnlyDictionary<string, Location>(Locations.ToDictionary(x => x.ToConvertibleCode(), x => x));
                }
            }

            return _locationsDict;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LocationCollection(XmlNode node, WcsConfiguration configuration)
            : this(node,configuration, true)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deserialize">是否立即解析</param>
        public LocationCollection(XmlNode node, WcsConfiguration configuration, Boolean deserialize)
            : base(node,configuration, false)
        {
            if (deserialize)
            {
                Deserialize();
            }
        }
        protected override void Deserialize()
        {
            List<ParticularLocationCollection> particularLocationCollections = new List<ParticularLocationCollection>();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (XmlNode locationsNode in this.Node.ChildNodes)
            {
                if (locationsNode.NodeType != XmlNodeType.Element)
                {
                    WcsConfiguration._logger.Warn1("未处理的内容 " + locationsNode.OuterXml, this);
                    continue;
                }
                var typeName = GetAttribute<String>("type", locationsNode);
                var particularLocationCollection = ReflectionHelper.CreateInstance<ParticularLocationCollection>(typeName, locationsNode, this.WcsConfiguration);

                var invalidLocations = particularLocationCollections
                    .SelectMany(x => x.LocationElements)
                    .Where(x => !(x.Location is ILocationWildcard))
                    .Select(loc => loc.Location)
                    .Intersect(
                        particularLocationCollection
                        .LocationElements
                        .Where(y => !(y.Location is ILocationWildcard))
                        .Select(loc => loc.Location));
                if (invalidLocations.Any())
                {
                    throw new ConfigurationErrorsException(String.Format("位置 {0} 重复", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray())));
                }

                invalidLocations = particularLocationCollections
                    .SelectMany(x => x.LocationElements)
                    .Select(x => x.Location)
                    .Where(x => x is ILocationWildcard)
                    .Intersect(
                        particularLocationCollection
                        .LocationElements
                        .Select(y => y.Location)
                        .Where(y => y is ILocationWildcard));
                if (invalidLocations.Any())
                {
                    throw new ConfigurationErrorsException(String.Format("通配符位置 {0} 重复", string.Join(",", invalidLocations.Select(x => x.ToString()).ToArray())));
                }

                particularLocationCollections.Add(particularLocationCollection);
            }
            this.ParticularLocationCollection = particularLocationCollections.ToArray();
            sw.Stop();
            Console.WriteLine("Deserialize locations used {0} milliseconds.", sw.ElapsedMilliseconds);
            //创建同义关系
            addSynonymous(); 

        }

        void addSynonymous()
        {
            var locations = this.ParticularLocationCollection.SelectMany(x => x.LocationElements);
            var locationsDict = this.AsDictionary();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (var locationElement in locations.Where(x=>!String.IsNullOrWhiteSpace(x.Synonymous)))
            {
                if (string.IsNullOrWhiteSpace(locationElement.Synonymous))
                {
                    continue;
                }

                var loc = locationElement.Location;
                var synonymous = locationElement.Synonymous.Trim().Split(',');
                foreach (var item in synonymous)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        WcsConfiguration._logger.Warn1(string.Format("{0} 属性 {1} 的值 “{2}” 中包含有无效的项 “{3}”",
                            locationElement.Node.GetXPath(),
                            "synonymous",
                            locationElement.Synonymous,
                            item
                            ), this, synonymous);
                    }
                    else
                    {
                        if (!locationsDict.ContainsKey(item.Trim()))
                        {
                            string msg = string.Format("{0} 属性 {1} 的值 “{2}” 中的项 “{3}” 未匹配到任何 {4} 对象",
                                                            locationElement.Node.GetXPath(),
                                                            "synonymous",
                                                            locationElement.Synonymous,
                                                            item.Trim(),
                                                            typeof(Location)
                                                        );
                            throw new ConfigurationErrorsException(msg,locationElement.Node);
                        }

                        loc.AddSynonymous(locationsDict[item.Trim()]);
                        //添加双向同义
                        //if (!locationsDict[item.Trim()].Synonymous.Any(x => x == loc))
                        {
                            locationsDict[item.Trim()].AddSynonymous(loc);
                        }
                        //var synonymousLoc = locations.Where(x => string.Equals(x.Location.ToConvertibleCode(), item.Trim(), StringComparison.CurrentCultureIgnoreCase))
                        //    .Select(x => x.Location)
                        //    .ToArray();
                        //var matchCount = synonymousLoc.Length;
                        //if (matchCount == 0)
                        //{
                        //    string msg = string.Format("{0} 属性 {1} 的值 “{2}” 中的项 “{3}” 未匹配到任何 {4} 对象",
                        //                                    locationElement.Node.GetXPath(),
                        //                                    "synonymous",
                        //                                    synonymous,
                        //                                    item.Trim(),
                        //                                    typeof(Location)
                        //                                );
                        //    throw new ConfigurationErrorsException(msg);

                        //}
                        //if (matchCount > 1)
                        //{
                        //    string msg = string.Format("{0} 属性 {1} 的值 “{2}” 中的项 “{3}” 匹配到多个 {4} 对象：\n{5}",
                        //                                    locationElement.Node.GetXPath(),
                        //                                    "synonymous",
                        //                                    synonymous,
                        //                                    item.Trim(),
                        //                                    typeof(Location),
                        //                                    string.Join("\n", synonymousLoc.Select(x => x.ToString()).ToArray())
                        //                                );
                        //    throw new ConfigurationErrorsException(msg);
                        //}

                        //loc.AddSynonymous(synonymousLoc[0]);
                    }
                }
            }

            sw.Stop();
            Console.WriteLine("AddSynonymous used {0} milliseconds.", sw.ElapsedMilliseconds);
        }
    }
}
