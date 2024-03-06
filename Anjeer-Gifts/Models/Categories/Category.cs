using Anjeer_Gifts.Models.Commons;
using Newtonsoft.Json;

namespace Anjeer_Gifts.Models.Categories;

public class Category : Auditable
{
#pragma warning disable
    [JsonProperty("name")]

    public string Name { get; set; }
}
