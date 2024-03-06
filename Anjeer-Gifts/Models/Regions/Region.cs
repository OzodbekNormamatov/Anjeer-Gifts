using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anjeer_Gifts.Models.Regions;

public class Region
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("capital")]
    public string Capital { get; set; }
}
