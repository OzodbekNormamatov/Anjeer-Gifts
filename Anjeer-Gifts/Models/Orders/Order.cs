using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models.Orders;

public class Order
{
#pragma warning disable
    public long Id { get; set; }
    [JsonProperty("customerId")]
    public long CustomerId { get; set; }
    [JsonProperty("location")]
    public MyLocation Location { get; set; }
    [JsonProperty("orderNumber")]
    public int OrderNumber { get; set; }
    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }
    [JsonProperty("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public long LocationId { get; set; }
}
