using Anjeer_Gifts.Models.Commons;
using Anjeer_Gifts.Models.Products;
using Telegram.Bot.Types;

namespace Anjeer_Gifts.Models.GiftBoxes;

public class GiftBox : Auditable
{
    public long UserId { get; set; }

    public List<Product> Products { get; set; }
}
