using Anjeer_Gifts.Models.Commons;

namespace Anjeer_Gifts.Models.Products;

public class Product : Auditable
{
    public long CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string PictureUrl { get; set; }

    public decimal Price { get; set; }
}
