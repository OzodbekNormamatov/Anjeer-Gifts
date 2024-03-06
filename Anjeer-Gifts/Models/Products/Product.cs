using Anjeer_Gifts.Models.Commons;
using Newtonsoft.Json;

namespace Anjeer_Gifts.Models.Products;

public class Product : Auditable
{
    [JsonProperty("categoryname")]
    public string CategoryName { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("pictureUrl")]
    public string PictureUrl { get; set; }
    [JsonProperty("price")]
    public decimal Price { get; set; }
}
