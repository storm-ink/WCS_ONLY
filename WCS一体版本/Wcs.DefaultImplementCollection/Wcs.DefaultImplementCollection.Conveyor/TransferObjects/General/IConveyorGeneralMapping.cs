using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    interface IConveyorGeneralMapping
    {
        [JsonIgnore]
        Dictionary<string, GeneralMappings> GeneralMappingsDic { get; set; }
        string GetMessages();
        dynamic GetGeneralMappingDynamic(string key, byte[] bytes);
        string GetGeneralMappingDynamicShowMessage(string key, byte[] bytes);
    }
}
