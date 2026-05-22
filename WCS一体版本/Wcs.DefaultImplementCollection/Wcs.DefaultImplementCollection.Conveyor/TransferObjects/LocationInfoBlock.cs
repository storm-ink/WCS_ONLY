using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    public class LocationInfoBlock : NetTransferObject, IConveyorGeneralMapping
    {
        /// <summary>
        /// 货位号
        /// </summary>
        public UInt16 PosNo { get; set; }
        /// <summary>
        /// 任务号
        /// </summary>
        public UInt32 TaskNo { get; set; }
        /// <summary>
        /// 控制状态
        /// </summary>
        /// <remarks>
        /// 0-无信号，1-有信号
        /// 目前仅涉及Sineva输送线包含此类型，后续PLC同事会提供此项说明
        /// 例如：
        /// x方向可以接收
        /// x方向接收完成
        /// 本节申请传递
        /// 本节输送线就绪状态
        /// y方向可以接收
        /// y方向接收完成
        /// </remarks>
        public Byte[] ControlState { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        /// <remarks>默认值0,PLC同事提供</remarks>
        public UInt16 DeviceType { get; set; }
        /// <summary>
        /// 电机状态
        /// </summary>
        /// <remarks>
        /// PLC自定义块，项目设计过程中，PLC同事提供表格及分类说明
        /// </remarks>
        public Byte[] MotorState { get; set; }
        /// <summary>
        /// 光电信号
        /// </summary>
        /// <remarks>
        /// PLC自定义块，项目设计过程中，PLC同事提供表格及分类说明
        /// </remarks>
        public Byte[] Sensor { get; set; }
        /// <summary>
        /// 是否有货
        /// </summary>
        /// <remarks>
        /// 0-无货，1-有货
        /// 此字段需要PLC单独处理，综合若干光电信号上报此设备上是否有货，供2D、3D展示使用
        /// </remarks>
        public UInt16 HaveGoods { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        /// <remarks>
        /// 0-无信号，1-有信号
        /// 需要供应商根据实际情况提供
        /// </remarks>
        public Byte[] Alarms { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        /// <remarks>
        /// 0:初始化,1:报警,2:离线,3:手动,4:停止,5:运行
        /// /remarks>
        /// <remarks>
        /// 离线适用于每节输送机增加隔离开关的情况，无隔离开关时此等同于手动3
        /// 区域单个急停按钮关联 1报警 具体急停信息填充到报警信息中
        /// </remarks>
        public LocationStatus State { get; set; }
        /// <summary>
        /// 所属电控柜
        /// </summary>
        public UInt16 OwerArea { get; set; }
        public Dictionary<string, GeneralMappings> GeneralMappingsDic { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "TaskNo":
                        return this.TaskNo;
                    case "ControlState":
                        return this.ControlState;
                    case "DeviceType":
                        return this.DeviceType;
                    case "MotorState":
                        return this.MotorState;
                    case "Sensor":
                        return this.Sensor;
                    case "HaveGoods":
                        return this.HaveGoods;
                    case "Alarms":
                        return this.Alarms;
                    case "State":
                        return this.State;
                    case "OwerArea":
                        return this.OwerArea;
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
                    case "TaskNo":
                        this.TaskNo = Convert.ToUInt32(value);
                        break;
                    case "ControlState":
                        this.ControlState = (Byte[])value;
                        break;
                    case "DeviceType":
                        this.DeviceType = Convert.ToUInt16(value);
                        break;
                    case "MotorState":
                        this.MotorState = (Byte[])value;
                        break;
                    case "Sensor":
                        this.Sensor = (Byte[])value;
                        break;
                    case "HaveGoods":
                        this.HaveGoods = Convert.ToUInt16(value);
                        break;
                    case "Alarms":
                        this.Alarms = (Byte[])value;
                        break;
                    case "State":
                        this.State = (LocationStatus)Convert.ToUInt16(value);
                        break;
                    case "OwerArea":
                        this.OwerArea = Convert.ToUInt16(value);
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
                TaskNo = this.TaskNo,
                ControlState = GetGeneralMappingDynamicShowMessage("ControlState", this.ControlState),
                DeviceType = this.DeviceType,
                MotorState = GetGeneralMappingDynamicShowMessage("MotorState", this.MotorState),
                Sensor = GetGeneralMappingDynamicShowMessage("Sensor", this.Sensor),
                HaveGoods = this.HaveGoods == 0 ? "无货" : "有货",
                Alarms = GetGeneralMappingDynamicShowMessage("Alarms", this.Alarms),
                State = this.State.GetDescription(),
                OwerArea = this.OwerArea
            };
        }

        public string GetMessages()
        {
            throw new NotImplementedException();
        }

        public dynamic GetGeneralMappingDynamic(string key, byte[] bytes)
        {
            if (this.GeneralMappingsDic == null)
                return null;
            if (!this.GeneralMappingsDic.ContainsKey(key))
                return null;
            var generalMappings = this.GeneralMappingsDic[key];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue((byte[])bytes);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            dynamic _obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)_obj).Add(item.Key, item.Value);
            }
            return _obj;
        }

        public string GetGeneralMappingDynamicShowMessage(string key, byte[] bytes)
        {
            if (this.PosNo == 0)
                return "null";
            if (this.GeneralMappingsDic == null)
                return "null";
            if (!this.GeneralMappingsDic.ContainsKey(key))
                return "null";
            var generalMappings = this.GeneralMappingsDic[key];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return "";
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return "";
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue((byte[])bytes);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            return string.Join(",", temp.Select(x => $"{x.Key}={x.Value}"));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns>dynamic is IDictionary<string, object></returns>
        public dynamic GetControlStateDynamic()
        {
            if (this.PosNo == 0)
                return "null";
            if (this.GeneralMappingsDic == null)
                return null;
            if (!this.GeneralMappingsDic.ContainsKey("ControlState"))
                return null;
            var generalMappings = this.GeneralMappingsDic["ControlState"];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.ControlState);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>dynamic is IDictionary<string, object></returns>
        public dynamic GetMotorStateDynamic()
        {
            if (this.PosNo == 0)
                return "null";
            if (this.GeneralMappingsDic == null)
                return null;
            if (!this.GeneralMappingsDic.ContainsKey("MotorState"))
                return null;
            var generalMappings = this.GeneralMappingsDic["MotorState"];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.MotorState);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>dynamic is IDictionary<string, object></returns>
        public dynamic GetSensorDynamic()
        {
            if (this.PosNo == 0)
                return "null";
            if (this.GeneralMappingsDic == null)
                return null;
            if (!this.GeneralMappingsDic.ContainsKey("Sensor"))
                return null;
            var generalMappings = this.GeneralMappingsDic["Sensor"];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.Sensor);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>dynamic is IDictionary<string, object></returns>
        public dynamic GetAlarmsDynamic()
        {
            if (this.PosNo == 0)
                return "null";
            if (this.GeneralMappingsDic == null)
                return null;
            if (!this.GeneralMappingsDic.ContainsKey("Alarms"))
                return null;
            var generalMappings = this.GeneralMappingsDic["Alarms"];

            var group = generalMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = generalMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = generalMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.Alarms);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.Name, property.Content);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }
    }
}
