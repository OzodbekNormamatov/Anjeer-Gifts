using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models.Orders;

public class Order
{
    public long CustomerId { get; set; }
    public Location Location { get; set; }
    public int OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
