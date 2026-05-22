using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    public abstract class LocationElement : ConfigurationElement
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        public String UserCode { get; private set; }
        /// <summary>
        /// 该位置在设备中的编码形式
        /// </summary>
        public String DeviceCode { get; private set; }
        /// <summary>
        /// 该位置在设备中的唯一编码形式
        /// </summary>
        public String UnifiedCode { get; private set; }
        /// <summary>
        /// 同义位置对象集合
        /// </summary>
        public String Synonymous { get; private set; }
        /// <summary>
        /// 所属设备
        /// </summary>
        public String DeviceName { get; private set; }
        /// <summary>
        /// 位置对象
        /// </summary>
        public Location Location { get; private set; }
        /// <summary>
        /// 是否是通配符
        /// </summary>
        public Boolean IsWildcard { get; private set; }
        /// <summary>
        /// 如果是通配符位置则标识是否可以作为唯一的Location位置
        /// 非通配符位置无意义
        /// </summary>
        public Boolean AbleAsOnlyLocation { get; private set; }
        /// <summary>
        /// 位置类型。
        /// 有可能为 null，在某些特殊的位置上有可能需要指定类型并在 CreateLocation() 中使用它。
        /// </summary>
        public Type LocationType { get; private set; }
        /// <summary>
        /// 所属的位置集合节点对象
        /// </summary>
        public ParticularLocationCollection ParticularLocationCollection { get; private set; }
        /// <summary>
        /// 创建位置对象。<br />
        /// 需要注意的是，此处应该将创建的位置对象加入到相关联的设备当中。
        /// </summary>
        /// <returns></returns>
        protected abstract Location CreateLocation();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        public LocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration)
            : this(node, particularLocationCollection,configuration, true)
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deserialize">是否立即解析</param>
        public LocationElement(XmlNode node, ParticularLocationCollection particularLocationCollection, WcsConfiguration configuration, Boolean deserialize)
            : base(node, configuration, false)
        {
            this.ParticularLocationCollection = particularLocationCollection;
            IsWildcard = GetAttributeOrDefault<Boolean>("isWildcard");
            UserCode = GetAttributeOrDefault<String>("userCode");
            DeviceCode = GetAttributeOrDefault<String>("deviceCode");
            UnifiedCode = GetAttributeOrDefault<string>("unifiedCode", DeviceCode);
            Synonymous = GetAttributeOrDefault<String>("synonymous");
            AbleAsOnlyLocation = GetAttributeOrDefault<Boolean>("ableAsOnlyLocation", false);

            var typeName = GetAttributeOrDefault<String>("type");
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                var locationType = Type.GetType(typeName);
                if (locationType == null)
                {
                    throw new ConfigurationErrorsException(string.Format("未找到 {0} 节点属性 {1} 所指定的类型 {2}。", this.Node.GetXPath(), "type", typeName));
                }
                if (!locationType.IsSubclassOf(typeof(Location)))
                {
                    throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 所指定的类型 {2} 不是 {3} 的子类。", this.Node.GetXPath(), "type", typeName, typeof(Location)));
                }
                this.LocationType = locationType;
            }

            DeviceName = GetAttributeOrDefault<String>("device", this.ParticularLocationCollection.DeviceName);
            if (string.IsNullOrWhiteSpace(DeviceName))
            {
                throw new ConfigurationErrorsException(string.Format("{0} 节点属性 {1} 未指定任何值。\n你可以尝试在节点上删除该属性或在节点上为该属性指定一个值。", this.Node.GetXPath(), "device"));
            }

            if (deserialize)
            {
                Deserialize();
            }

            this.Location = CreateLocation();
        }
    }
}
