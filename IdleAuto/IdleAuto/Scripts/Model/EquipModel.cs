using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EquipModel
{
    [JsonProperty("etype")]
    public emEquipType emEquipType { get; set; }
    [JsonProperty("eid")]
    public long EquipID { get; set; }
    public string EquipTypeName { get; set; }
    public string EquipName { get; set; }
    public string Content { get; set; }
}

