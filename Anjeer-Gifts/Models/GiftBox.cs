using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models;

public class GiftBox : Auditable
{
    public long UserId { get; set; }

    public List<Product> Products { get; set; }
}
