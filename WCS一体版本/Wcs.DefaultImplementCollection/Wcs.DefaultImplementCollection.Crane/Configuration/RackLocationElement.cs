using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wcs.Framework;
using Wcs.Framework.Cfg;

namespace Wcs.DefaultImplementCollection.Crane
{
    public class RackLocationElement : LocationElement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public RackLocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : base(node, particularLocationCollection, configuration, true)
        {

        }

        /// <summary>
        /// 列位置
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// 指示该位置是否启用
        /// </summary>
        public Boolean Enabled { get; protected set; }

        /// <summary>
        /// 货叉允许的动作
        /// </summary>
        public ForkAction ForkAction { get; protected set; }

        /// <summary>
        /// 堆垛机执行任务时此位置的排相对于设备所在的左右位置
        /// </summary>
        public ForkDirection? ForkDirection { get; protected set; }

        /// <summary>
        /// 是否是原点位置
        /// </summary>
        public Boolean IsOriginPoint { get; protected set; }

        /// <summary>
        /// 层位置
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 用户排位置
        /// </summary>
        public int UserLine { get; set; }
        /// <summary>
        /// 用户列位置
        /// </summary>
        public int UserColumn { get; set; }
        /// <summary>
        /// 用户层位置
        /// </summary>
        public int UserLevel { get; set; }
        /// <summary>
        /// 货格高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 统一编码
        /// </summary>
        public string UnifiedCode { get; set; }
        protected override Framework.Location CreateLocation()
        {
            Device device;
            if (String.Equals(this.DeviceName, this.ParticularLocationCollection.DeviceName, StringComparison.CurrentCultureIgnoreCase))
            {
                device = this.ParticularLocationCollection.GetDeviceElement().Device;
            }
            else
            {
                device = this.WcsConfiguration
                .DeviceCollection
                .ParticularDeviceCollection
                .SelectMany(x => x.DeviceElements)
                .Where(x => string.Equals(x.Device.Name, this.DeviceName, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Device)
                .SingleOrDefault();

                if (device == null)
                {
                    string msg = string.Format("未找到 {0} 节点属性 {1} 值 “{2}” 中标识的设备",
                        this.Node.GetXPath(),
                        "device",
                        this.DeviceName);
                    throw new ConfigurationErrorsException(msg);
                }
            }

            if (!(device is CraneDevice))
            {
                string msg = string.Format("{0} 节点属性 {1} 值 “{2}” 中标识的设备 {3} 并非 {4} 类型",
                    this.Node.GetXPath(),
                    "device",
                    this.DeviceName,
                    device,
                    typeof(CraneDevice)
                    );
                throw new ConfigurationErrorsException(msg);
            }

            RackLocation loc;

            if (!this.IsWildcard)
            {
                if (this.LocationType != null)
                {
                    WcsConfiguration._logger.Warn1(string.Format("{0} 节点属性 {1} 指定了一个值 {2}，但该位置不是通配符，已被忽略。", this.Node.GetXPath(), "type", this.LocationType), this);
                }

                if (this.IsOriginPoint)
                {
                    loc = new CraneDeviceOriginPointLocation((CraneDevice)device, UserCode, Column, Level, UserColumn, UserLevel);
                }
                else
                {
                    loc = new RackLocation((CraneDevice)device, UserCode, ForkDirection, ForkAction, Column, Level, UserLine, UserColumn, UserLevel, Enabled, Height, UnifiedCode);
                }
            }
            else
            {
                if (this.LocationType != null)
                {
                    if (!this.LocationType.IsSubclassOf(typeof(RackLocationWildcard)))
                    {
                        throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 所指定的类型 {2} 不是 {3} 的子类。", this.Node.GetXPath(), "type", this.LocationType, typeof(RackLocationWildcard)));
                    }

                    loc = ReflectionHelper.CreateInstance<RackLocationWildcard>(this.LocationType, device, this, this.AbleAsOnlyLocation);
                }
                else
                {
                    loc = new RackLocationWildcard((CraneDevice)device, this, this.AbleAsOnlyLocation);
                }
            }
            ((CraneDevice)device).AcceptLocation(loc);

            return loc;
        }

        protected override void Deserialize()
        {
            if (!this.IsWildcard)
            {
                //如果没有指定的 type 值，说明使用了 RackLocation 类型，此时必须保证 column 和 level 属性有值
                if (String.IsNullOrWhiteSpace(this.GetAttributeOrDefault<String>("type")))
                {
                    this.Column = GetAttribute<Int32>("column");
                    this.Level = GetAttribute<Int32>("level");
                }
                else
                {
                    this.Column = GetAttributeOrDefault<Int32>("column");
                    this.Level = GetAttributeOrDefault<Int32>("level");
                }

                this.UserLine = GetAttributeOrDefault<Int32>("userLine", this.UserLine);
                this.UserColumn = GetAttributeOrDefault<Int32>("userColumn", this.Column);
                this.UserLevel = GetAttributeOrDefault<Int32>("userLevel", this.Level);
                this.UnifiedCode = GetAttributeOrDefault<string>("unifiedCode", null);
                this.IsOriginPoint = GetAttributeOrDefault<Boolean>("isOriginPoint", false);
                if (this.IsOriginPoint)
                {
                    this.ForkDirection = null;
                    this.ForkAction = ForkAction.None;
                    WcsConfiguration._logger.Warn1(string.Format("{0} 是一个原点位置，系统强制禁用了伸叉操作。如果需要启用该位置的伸叉操作，请删除 isOriginPoint 属性或将其值设为 false", this.Node.GetXPath()), this);
                }
                else
                {
                    this.ForkDirection = GetAttributeOrDefault<ForkDirection?>("forkDirection", this.Node, null);
                    this.ForkAction = GetAttributeOrDefault<ForkAction>("forkAction", this.Node, ForkAction.Pickup | ForkAction.Putdown);
                }
                this.Enabled = GetAttributeOrDefault<Boolean>("enabled", true);
                this.Height = GetAttributeOrDefault<int>("height", 0);
            }
        }
    }
}
