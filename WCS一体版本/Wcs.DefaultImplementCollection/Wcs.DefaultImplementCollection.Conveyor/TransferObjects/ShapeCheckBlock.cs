using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class ShapeCheckBlock : NetTransferObject, IConveyorGeneralMapping
    {
        /// <summary>
        /// 货位编号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 外形检测状态
        /// </summary>
        /// <remarks>
        /// 成功写1，失败写2，离位后写0
        /// 外型检测结果，PLC写，离位，即离开当节输送机
        /// </remarks>
        public ShapeSatus ShapeState { get; set; }
        /// <summary>
        /// 货型
        /// </summary>
        /// <remarks>
        /// 0无效值，有效值从1开始，依次增大
        /// 场景1：高度检测，一种高度的情况下此字段无效默认0，n种高度，有效值从1开始一直到n结束，具体的高度值由WMS、WCS等上位机转换
        /// 场景2：待定义
        /// </remarks>
        public UInt16 ShapeType { get; set; }
        /// <summary>
        /// 检测结果
        /// </summary>
        /// <remarks>
        /// PLC自定义块，WCS和PLC同事提前定制通用性表格，如需定制，每个项目具体沟通
        /// </remarks>
        public Byte[] ShapeResult { get; set; }

        public Dictionary<string, GeneralMappings> GeneralMappingsDic { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return PosNo;
                    case "ShapeState":
                        return ShapeState;
                    case "ShapeType":
                        return ShapeType;
                    case "ShapeResult":
                        return ShapeResult;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));
                }
            }
            set
            {
                switch (name)
                {
                    case "PosNo":
                        this.PosNo = Convert.ToUInt16(value);
                        break;
                    case "ShapeState":
                        this.ShapeState = (ShapeSatus)Convert.ToUInt16(value);
                        break;
                    case "ShapeType":
                        this.ShapeType = Convert.ToUInt16(value);
                        break;
                    case "ShapeResult":
                        this.ShapeResult = (Byte[])value;
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override object GetDataGridViewShow()
        {
            return new
            {
                PosNo = this.PosNo,
                ShapeState = this.ShapeState.GetDescription(),
                ShapeType = this.ShapeType,
                //ShapeResult = string.Join(" ", this.ShapeResult)
                ShapeResult = this.ShapeResult != null ? string.Join(" ", this.ShapeResult) : string.Empty
            };
        }

        public string GetMessages()
        {
            throw new NotImplementedException();
        }

        public dynamic GetGeneralMappingDynamic(string key, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public string GetGeneralMappingDynamicShowMessage(string key, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetResultsDynamic()
        {
            if (this.PosNo == 0)
                return null;
            if (this.GeneralMappingsDic == null)
                return null;
            var key = nameof(this.ShapeResult);
            if (!this.GeneralMappingsDic.ContainsKey(key))
                return null;
            var generalMappings = this.GeneralMappingsDic[key];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return null;
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return null;
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.ShapeResult);
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.ShowName, property.Content);
            }
            return temp;
        }
    }
}
