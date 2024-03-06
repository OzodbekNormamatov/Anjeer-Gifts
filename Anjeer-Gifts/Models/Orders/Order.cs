using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models.Orders;

public class Order
{
    [JsonProperty("customerId")]
    public long CustomerId { get; set; }
    [JsonProperty("location")]
    public Location Location { get; set; }
    [JsonProperty("orderNumber")]
    public int OrderNumber { get; set; }
    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }
    [JsonProperty("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
