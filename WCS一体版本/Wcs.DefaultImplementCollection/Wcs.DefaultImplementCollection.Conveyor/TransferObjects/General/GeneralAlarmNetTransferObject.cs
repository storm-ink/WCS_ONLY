using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Wcs.Framework;
using Newtonsoft.Json;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 报警状态.
    /// 配置
    ///   <collection type="Wcs.DefaultImpls.Conveyor.AlarmNetTransferObject, Wcs.DefaultImpls" blockBytes="6" itemCount="20">
    //        <property name="ShapeCheckNO" index="0" size="2" type="UInt16" />
    //        <property name="Alarms" index="2" size="4" type="" />
    //      </collection>
    /// </summary>
    [DisplayName("通用报警块")]
    [JsonObject]
    public class GeneralAlarmNetTransferObject : NetTransferObject, IConveyorGeneralMapping
    {
        [DisplayName("货位号")]
        public UInt16 PosNo { get; set; }
        [DisplayName("报警列表")]
        public Byte[] Alarms { get; set; }
        public Dictionary<string, GeneralMappings> GeneralMappingsDic { get; set; }

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "PosNo":
                        return this.PosNo;
                    case "Alarms":
                        return this.Alarms;
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
                    case "Alarms":
                        this.Alarms = (Byte[])value;
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        public dynamic GetDynamic()
        {
            var group = GeneralMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = GeneralMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = GeneralMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
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

        public dynamic GetDynamicShowName()
        {
            var group = GeneralMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return new { };
            var binding = GeneralMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return new { };
            var mapping = GeneralMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.Alarms);
            Dictionary<string, object> temp = new Dictionary<string, object>();
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                temp.Add(property.ShowName, property.Content);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            foreach (KeyValuePair<string, object> item in temp)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            }
            return obj;
        }

        public string GetMessages()
        {

            StringBuilder sb = new StringBuilder();
            var group = GeneralMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
            if (group == null)
                return sb.ToString();
            var binding = GeneralMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
            if (binding == null)
                return sb.ToString();
            var mapping = GeneralMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
            mapping.SetPropertyValue(this.Alarms);
            sb.Append("{");
            foreach (var property in mapping.Properties)
            {
                var valueType = property.Type.GetBasicType();
                sb.Append($"\"{property.Name}\":\"{property.Content}\",");
            }
            sb.Remove(sb.Length - 2, 1);
            sb.Append("}");
            return sb.ToString();
        }

        public string[] GetAlarm()
        {
            List<String> result = new List<string>();
            try
            {
                if (GeneralMappings != null && this.PosNo != 0)
                {
                    var group = GeneralMappings.Groups.GroupList.FirstOrDefault(x => x.Content.Contains(this.PosNo.ToString()));
                    if (group == null)
                    {
                        result.Add("未配置 GeneralMappings.Groups");
                        return result.ToArray();
                    }
                    var binding = GeneralMappings.Bindings.BindingList.FirstOrDefault(x => x.Group == group.GroupName);
                    if (binding == null)
                    {
                        result.Add("未配置 GeneralMappings.Bindings");
                        return result.ToArray();
                    }
                    var mapping = GeneralMappings.Mappings.MappingList.FirstOrDefault(x => x.Name == binding.Mapping);
                    mapping.SetPropertyValue(this.Alarms);
                    foreach (var property in mapping.Properties)
                    {
                        var valueType = property.Type.GetBasicType();
                        if (valueType == typeof(Boolean) && !string.IsNullOrWhiteSpace(property.Content) && Boolean.TryParse(property.Content, out bool content) && content)
                        {
                            result.Add(property.ShowName);
                        }
                    }
                }
            }
            catch
            {
            }

            return result.ToArray();
        }

        public override object GetDataGridViewShow()
        {
            return new { 
                PosNo = this.PosNo, 
                Alarms = Alarms == null ? "" : string.Join(" ", this.Alarms.Select(x => System.Convert.ToString(x, 2))), 
                Description = string.Join(",", this.GetAlarm()) };
        }
    }
}
