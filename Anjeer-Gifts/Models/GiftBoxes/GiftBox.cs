using Anjeer_Gifts.Models.Commons;
using Anjeer_Gifts.Models.Products;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models.GiftBoxes;

public class GiftBox
{
    [JsonProperty("userId")]
    public long UserId { get; set; }
    [JsonProperty("products")]
    public List<Product> Products { get; set; }
}
