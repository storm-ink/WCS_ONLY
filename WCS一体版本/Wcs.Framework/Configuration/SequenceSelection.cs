using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Wcs.Framework.Cfg
{
    /// <summary>
    /// 序列节点，包含一组描述序列特性相关的配置信息。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 包含属性如下<br />
    /// type：指示该节点映射到的具体实现类。该类必须为 <see cref="T:Wcs.Framework.EquipmentActionSequence"/> 或继承自 <see cref="T:Wcs.Framework.EquipmentActionSequence"/>.<br />
    /// compare:指示动作序列中使用的排序对比器类型。该类必须继承自 <see cref="T:Wcs.Framework.EquipmentActionSortComparer"/>.如果并不需要，可以使用框架提供的默认实现 <see cref="T:Wcs.Framework.Impl.DefaultEquipmentActionSortComparer"/><br />
    /// logTarget：字符串，日志输出目标名称。引用自 <see cref="T:Wcs.Framework.Cfg.LogTargetsSelection"/> 节点内的 <see cref="T:Wcs.Framework.Cfg.LogTargetElement"/> 配置。
    /// </para>
    /// <para>
    /// 子节点如下<br />
    /// <see cref="T:Wcs.Framework.Cfg.SequenceActionFilterSelection"/><br />
    /// <see cref="T:Wcs.Framework.Cfg.SequenceAdditionalTasksSelection"/>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="xml">
    /// <sequence type="Wcs.Framework.EquipmentActionSequence, Wcs.Framework" comparer="Wcs.Framework.Impl.DefaultEquipmentActionSortComparer, Wcs.Framework" logTarget="任务序列" />
    /// </code>
    /// <h4>需要注意的是：如果你添加了一个自定义设备，且这个设备不执行任何动作或者说不需要 Wcs 给他发送物理动作（任务），请使用框架提供的 <see cref="T:Wcs.Framework.Impl.EmptyEquipmentActionSequence"/> 空序列对象，以减少系统资源占用。比如：条码扫描设备、称重设备。</h4>
    /// </example>
    public class SequenceSelection:ConfigurationElement
    {
        /// <summary>
        /// 获取该配置映射到的物理动作序列。
        /// </summary>
        public EquipmentActionSequence Sequence { get; private set; }
        /// <summary>
        /// 获取序列动作过滤器集合配置节点。
        /// </summary>
        public SequenceActionFilterSelection SequenceActionFilterSelection { get; private set; }
        /// <summary>
        /// 获取序列在空闲时的额外动作集合配置节点。
        /// </summary>
        public SequenceAdditionalTasksSelection SequenceAdditionalTasksSelection { get; private set; }
        /// <summary>
        /// 获取与此对象关联的设备配置节点。
        /// </summary>
        public DeviceElement DeviceElement { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <param name="deviceElement">设备配置节点</param>
        public SequenceSelection(XmlNode node, DeviceElement deviceElement)
            : base(node,false)
        {
            DeviceElement = deviceElement;
            this.Deserialize(node);
        }
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="node">配置节点</param>
        /// <remarks>此实现为空方法，真正的创建创建是由外部程序调用 <see cref="M:Wcs.Framework.Cfg.SequenceSelection.CreateSequence"/> 方法而来。</remarks>
        protected override void Deserialize(System.Xml.XmlNode node)
        {
        }

        /// <summary>
        /// 创建 sequence，
        /// 不在 Deserialize 创建是因为 sequence 在创建后会立即运行，
        /// 这样在其它配置未初始化之前会引发一些意想不到的错误
        /// </summary>
        public virtual void CreateSequence()
        {
            if (this.Sequence == null)
            {
                if (this.Element.Attributes["type"] == null)
                {
                    throw new Exception("未指定 sequence 节点的 type 属性");
                }

                if (this.Element.Attributes["comparer"] == null)
                {
                    throw new Exception("未指定 sequence 节点的 comparer 属性");
                }

                string typeName = this.Element.Attributes["type"].Value;
                string comparerTypeName = this.Element.Attributes["comparer"].Value;

                LogTarget logTarget = null;
                string logTargetName = this.Element.Attributes["logTarget"] == null ? "" : this.Element.Attributes["logTarget"].Value;
                if (!string.IsNullOrWhiteSpace(logTargetName))
                {
                    logTarget = Configuration.GetLogTarget(logTargetName);
                }

                EquipmentActionSortComparer comparer = CreateInstance<EquipmentActionSortComparer>(comparerTypeName);
                this.SequenceActionFilterSelection = new SequenceActionFilterSelection(this.Element.SelectSingleNode("actionFilters"));
                var filters = SequenceActionFilterSelection.SequenceActionFilterElements.Select(x => x.Filter).ToArray();
                this.SequenceAdditionalTasksSelection = new SequenceAdditionalTasksSelection(this.Element.SelectSingleNode("additionalTasks"));
                var additionalTasks = SequenceAdditionalTasksSelection.SequenceAdditionalTasks.Select(x => x.SequenceAdditionalTask).ToArray();

                EquipmentActionSequenceGroup group = null; ;
                if (this.Element.Attributes["group"] != null && !string.IsNullOrWhiteSpace(this.Element.Attributes["group"].Value))
                {
                    string groupName = this.Element.Attributes["group"].Value;
                    var groupElement = Configuration.SequenceGroupSelection.SequenceGroupElements.SingleOrDefault(x => x.Group.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));
                    if (groupElement == null)
                    {
                        throw new Exception(string.Format("未找到名为 {0} 的 SequenceGroup", groupName));
                    }
                    group = groupElement.Group; ;
                }

                this.Sequence = CreateInstance<EquipmentActionSequence>(typeName, DeviceElement.Device, comparer, logTarget, filters, additionalTasks, group);
            }

            if (this.DeviceElement.Device.ActionSequence == null)
            {
                this.DeviceElement.Device.ActionSequence = this.Sequence;
            }
        }
    }
}
