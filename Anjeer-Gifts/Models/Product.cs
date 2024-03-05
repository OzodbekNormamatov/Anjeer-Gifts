namespace Anjeer_Gifts.Models;

public class Product : Auditable
{
    public long CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string PictureUrl { get; set; }

    public decimal Price { get; set; }
}
