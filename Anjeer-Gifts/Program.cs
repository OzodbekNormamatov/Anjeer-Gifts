using Anjeer_Gifts.AppDbContext;
using Anjeer_Gifts.Configuration;
using Anjeer_Gifts.Models.Categories;
using Anjeer_Gifts.Models.GiftBoxes;
using Anjeer_Gifts.Models.Orders;
using Anjeer_Gifts.Models.Products;
using Anjeer_Gifts.Models.Users;
using Anjeer_Gifts.Services;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Telegram.Bot;

var client = new TelegramBotClient(Constants.BOTTOKEN);

var dbContext = new MyDbContext();

var dataService = new DataService(dbContext);

//await dataService.SaveDataFromJsonToDb();

TelegramService telegramService = new TelegramService(client, dbContext);

await telegramService.Run();

public class DataService
{
    private readonly MyDbContext _dbContext;

    public DataService(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveDataFromJsonToDb()
    {
        await SaveCategoriesFromJson();
        await SaveUsersFromJson();
        await SaveGiftBoxesFromJson();
        await SaveProductsFromJson();
        await SaveOrdersFromJson();
    }

    private async Task SaveCategoriesFromJson()
    {
        string jsonContent = await File.ReadAllTextAsync(Constants.CATEGORYPATH);
        var categories = JsonConvert.DeserializeObject<List<Category>>(jsonContent);
        _dbContext.Categories.AddRange(categories);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveUsersFromJson()
    {
        string jsonContent = await File.ReadAllTextAsync(Constants.USERPATH);
        var users = JsonConvert.DeserializeObject<List<User>>(jsonContent);
        _dbContext.Users.AddRange(users);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveGiftBoxesFromJson()
    {
        string jsonContent = await File.ReadAllTextAsync(Constants.GIFTBOXPATH);
        var giftBoxes = JsonConvert.DeserializeObject<List<GiftBox>>(jsonContent);
        _dbContext.GiftBoxes.AddRange(giftBoxes);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveProductsFromJson()
    {
        string jsonContent = await File.ReadAllTextAsync(Constants.PRODUCTPATH);
        var products = JsonConvert.DeserializeObject<List<Product>>(jsonContent);
        _dbContext.Products.AddRange(products);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveOrdersFromJson()
    {
        string jsonContent = await File.ReadAllTextAsync(Constants.ORDERPATH);
        var orders = JsonConvert.DeserializeObject<List<Order>>(jsonContent);
        _dbContext.Orders.AddRange(orders);
        await _dbContext.SaveChangesAsync();
    }
}
