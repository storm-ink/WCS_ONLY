using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Wcs;

namespace Wcs.DefaultImplementCollection.SingleLocationDoubleVehicleManager
{
    /// <summary>
    /// 单货位一轨双车调度配置类
    /// </summary>复制代码
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class SingleLocationDoubleVehicleSubSystemConfig
    {
        public SingleLocationDoubleVehicleSubSystemConfig()
        { }
        /// <summary>
        /// 构造函数
        /// </summary>
        public SingleLocationDoubleVehicleSubSystemConfig(string deviceName)
        {
            FileName = $"{path}\\{deviceName}设置.xml";

            XmlDocument xml = new XmlDocument();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(FileName))
            {
                Vehicles = new Vehicles();
                Stations = new StationInfo();
                Bindings = new BindingInfo();
                FreedomStationList = "";
                SafeDistance = 3900;
                SlowDistance = 6000;
                Save();
            }
            else
            {
                var config = Load();
                Vehicles = config.Vehicles;
                Stations = config.Stations;
                Bindings = config.Bindings;
                FreedomStationList = config.FreedomStationList == null ? "" : config.FreedomStationList;
                SafeDistance = config.SafeDistance;
                SlowDistance = config.SlowDistance;
            }
        }
        [XmlIgnore]
        public string path = @".\系统配置\穿梭车";

        [XmlIgnore]
        public string FileName;
        /// <summary>
        /// 车辆列表
        /// </summary>
        public Vehicles Vehicles { get; set; }
        /// <summary>
        /// 站点设置
        /// </summary>
        public StationInfo Stations { get; set; }
        /// <summary>
        /// 车辆与站点绑定信息，默认情况下，所有站点所有车均可到达
        /// </summary>
        public BindingInfo Bindings { get; set; }
        /// <summary>
        /// 自由站点列表
        /// </summary>
        public string FreedomStationList { get; set; }
        /// <summary>
        /// 安全距离
        /// </summary>
        public int SafeDistance { get; set; }
        /// <summary>
        /// 减速距离
        /// </summary>
        public int SlowDistance { get; set; }

        public void Save()
        {
            XmlDocument xml = new XmlDocument();
            string strxml = this.XmlSerialize<SingleLocationDoubleVehicleSubSystemConfig>();
            xml.LoadXml(strxml);
            xml.Save(FileName);
        }

        public SingleLocationDoubleVehicleSubSystemConfig Load()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(FileName);
            var strxml = xml.OuterXml;
            return xml.DESerializer<SingleLocationDoubleVehicleSubSystemConfig>(strxml);
        }

        List<string> priorityStationList = null;
        public List<string> GetPriorityStations()
        {
            if (priorityStationList == null)
            {
                priorityStationList = new List<string>();
                var _list = this.Stations.Priority.Single.Split(',').Where(x => string.IsNullOrWhiteSpace(x)).ToList();
                if (_list.Count() > 0)
                    priorityStationList.AddRange(_list);
                var _groups = this.Stations.Priority.Groups.Split(',').Where(x => string.IsNullOrWhiteSpace(x)).ToList();
                if (_groups.Count() > 0)
                {
                    foreach (var groupName in _groups)
                    {
                        var group = Stations.Groups.FirstOrDefault(x => x.Name == groupName);
                        if (group == null)
                            continue;

                        _list = group.List.Split(',').Where(x => string.IsNullOrWhiteSpace(x)).ToList();
                        if (_list.Count() > 0)
                            priorityStationList.AddRange(_list);
                    }
                }
            }
            return priorityStationList;
        }

        public List<string> GetFreedomStationList()
        {
            if (this.FreedomStationList == null)
                return new List<string>();
            else
                return this.FreedomStationList.Split(',').ToList();
        }
    }

    /// <summary>
    /// 穿梭车设置状态
    /// </summary>
    [Description("穿梭车设置状态")]
    public enum RailGuidVehicleSetStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("未知")]
        Unknow,
        /// <summary>
        /// 启用
        /// </summary>
        [Description("启用")]
        Working,
        /// <summary>
        /// 停用
        /// </summary>
        [Description("停用")]
        Stoping,
        /// <summary>
        /// 维修
        /// </summary>
        [Description("维修")]
        Repairing
    }

    /// <summary>
    /// 车辆列表
    /// </summary>
    public class Vehicles
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Vehicles()
        {
            AbleList = "";
            StopList = "";
            RepairList = "";
            ConvertList = "";
            AverageTotalDistance = 0;
        }
        /// <summary>
        /// 可用车辆列表
        /// </summary>
        public string AbleList { get; set; }
        /// <summary>
        /// 停用车辆列表
        /// </summary>
        public string StopList { get; set; }
        /// <summary>
        /// 维修车辆列表
        /// </summary>
        public string RepairList { get; set; }
        /// <summary>
        /// 调度计算需要转换条码值车辆列表
        /// 算法：两台车所有站点条码值之和除以站点总数算出平均总长，平均总长减去当前车辆条码值获得相对条码值
        /// </summary>
        public string ConvertList { get; set; }
        /// <summary>
        /// 平均总长
        /// </summary>
        public int AverageTotalDistance { get; set; }
    }
    /// <summary>
    /// 站点信息
    /// </summary>
    public class StationInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public StationInfo()
        {
            Groups = new List<StationGroup>();
            Priority = new PriorityInfo();
        }
        /// <summary>
        /// 站点分组信息集合
        /// </summary>
        public List<StationGroup> Groups { get; set; }
        /// <summary>
        /// 优先级信息
        /// </summary>
        public PriorityInfo Priority { get; set; }
    }
    /// <summary>
    /// 站点分组信息
    /// </summary>
    public class StationGroup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public StationGroup()
        {
            Name = "";
            List = "";
        }
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 站点UserCode序列
        /// </summary>
        public string List { get; set; }
    }
    /// <summary>
    /// 优先级设置信息
    /// </summary>
    public class PriorityInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public PriorityInfo()
        {
            Single = "";
            Groups = "";
        }

        /// <summary>
        /// 单个站点优先级设置
        /// </summary>
        public string Single { get; set; }
        /// <summary>
        /// 分组优先级设置
        /// </summary>
        public string Groups { get; set; }
    }
    /// <summary>
    /// 车辆与站点绑定信息
    /// </summary>
    public class BindingInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BindingInfo()
        {
            Groups = new List<BindingGroup>();
        }
        /// <summary>
        /// 绑定信息分组
        /// </summary>
        public List<BindingGroup> Groups { get; set; }
    }

    /// <summary>
    /// 一组绑定信息
    /// </summary>
    public class BindingGroup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BindingGroup()
        {
            Station = "";
            Vehicle = "";
        }
        public string Station { get; set; }
        public string Vehicle { get; set; }
    }
}
