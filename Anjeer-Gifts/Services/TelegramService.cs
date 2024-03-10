using Newtonsoft.Json;
using Spectre.Console;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Anjeer_Gifts.Models.Users;
using User = Anjeer_Gifts.Models.Users.User;
using Anjeer_Gifts.Models.Orders;
using Anjeer_Gifts.Configuration;
using Anjeer_Gifts.Models.GiftBoxes;
using Anjeer_Gifts.Models.Products;
using Anjeer_Gifts.Models.Categories;
using Npgsql;
using Anjeer_Gifts.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Anjeer_Gifts.Models;

namespace Anjeer_Gifts.Services;

public class TelegramService
{
    private TelegramBotClient botClient;
    private MyDbContext _dbContext;

    public TelegramService(TelegramBotClient telegramBotClient, MyDbContext dbContext)
    {
        this.botClient = telegramBotClient;
        _dbContext = dbContext;
    }

    public async Task Run()
    {
        botClient.StartReceiving(Update, Error);
        Console.ReadLine();
    }

    public async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        var message = update.Message;

        AnsiConsole.MarkupLine($"[yellow]{message?.From?.FirstName}[/]  |  {message?.Text}");

        if(update?.Message?.Location is not null)
        {
            decimal totalPrice = 0;

            var boxes = GetBoxes() ?? new List<GiftBox>();

            var box = boxes.FirstOrDefault(b => b.UserId == update.Message.From.Id);

            var distinctMeals = box.AddedProducts.DistinctBy(m => m.Name);

            foreach (var meall in distinctMeals)
            {
                var quantity = box.AddedProducts.Count(m => m.Name == meall.Name);
                var price = meall.Price * quantity;
                totalPrice += price;
            }

            var order = new Order
            {
                CustomerId = update.Message.From.Id,
                TotalAmount = totalPrice
            };

            // Generate a unique order number
            var orderNumber = GenerateOrderNumber();
            order.OrderNumber = orderNumber;
            var orders = await LoadOrdersAsync();
            if (update.Message.Location is null)
            {
                order.Location = new MyLocation{ Latitude = 41, Longitude = 69 };
            }
            else
            {
                order.Location = ConvertToMyLocation(update.Message.Location);
            }
            orders.Add(order);
            box.AddedProducts.Clear();
            await SaveBoxesAsync(boxes);
            await SaveOrdersAsync(orders, order);
            await botClient.SendTextMessageAsync(update.Message.From.Id, "Your order has been placed successfully!");
        }

        if (update.CallbackQuery != null)
        {
            var boxes = GetBoxes() ?? new List<GiftBox>();

            if (update.CallbackQuery.Data == "Order")
            {
                // Ask the user for their location
                var request = new ReplyKeyboardMarkup(new[]
                {
                    new[] { KeyboardButton.WithRequestLocation("Share Location") },
                });

                var locationRequestMessage = "Please share the location where you want to deliver your Gift.";
                await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, locationRequestMessage, replyMarkup: request);
            }
            else if (update.CallbackQuery.Data.Contains("__delete"))
            {
                var mealName = update.CallbackQuery.Data.Replace("__delete", "").Trim();
                var box = boxes.FirstOrDefault(b => b.UserId == update.CallbackQuery.From.Id);

                if (box != null)
                {
                    var productsToRemove = box.AddedProducts.Where(m => m.Name == mealName).ToList();

                    if (productsToRemove.Count() > 0)
                    {
                        foreach (var mealToRemove in productsToRemove)
                        {
                            box.AddedProducts.Remove(mealToRemove);
                        }

                        await SaveBoxesAsync(boxes);
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, $"All meals with the name '{mealName}' have been removed from the box.");
                    }
                    else
                    {
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, $"No meals found with the name '{mealName}' in the box.");
                    }
                }
            }
            else if (!update.CallbackQuery.Data.Contains("__minus"))
            {
                var Product = GetProduct(update.CallbackQuery.Data);
                var box = boxes.FirstOrDefault(b => b.UserId == update.CallbackQuery.From.Id);

                if (box is null)
                {
                    box = new GiftBox()
                    {
                        UserId = update.CallbackQuery.From.Id,
                        AddedProducts = new List<Product>()
                    };
                    box.AddedProducts.Add(Product);
                    boxes.Add(box);
                }
                else
                {
                    box.AddedProducts.Add(Product);
                }
                await SaveBoxesAsync(boxes);
                await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Added to the box");
            }
            else
            {
                var mealName = update.CallbackQuery.Data.Replace("__minus", "").Trim();
                var box = boxes.FirstOrDefault(b => b.UserId == update.CallbackQuery.From.Id);

                if (box != null)
                {
                    var mealToRemove = box.AddedProducts.FirstOrDefault(m => m.Name == mealName);

                    if (mealToRemove != null)
                    {
                        box.AddedProducts.Remove(mealToRemove);
                        await SaveBoxesAsync(boxes);
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Meal removed from the box.");
                    }
                }
            }
        }
        else if (GetCustomers().Any(c => c.Id == message?.From?.Id))
        {
            var users = GetCustomers();
            if (users.FirstOrDefault(c => c.Id == message?.From?.Id).RegionName is null)
            {
                var regions = await GetRegionsAsync();
                var buttons = new List<KeyboardButton[]>();

                foreach (var region in regions)
                {
                    var button = new KeyboardButton(region.Name);
                    buttons.Add(new KeyboardButton[] { button });
                }

                var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons);

                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Please select a region",
                    replyMarkup: replyKeyboardMarkup);

                users = GetCustomers();
                users.FirstOrDefault(c => c.Id == message?.From?.Id).RegionName = message?.Text;
                await SaveUsersAsync(users);
            }
            else if(users.FirstOrDefault(c => c.Id == message?.From?.Id).PhoneNumber is not null)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] { "Orders", "Categories", "Mening Savatim" },
                })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    "Now choose:",
                    replyMarkup: replyKeyboardMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Share your contact",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { KeyboardButton.WithRequestContact("Share Contact") },
                        })
                );
                await Console.Out.WriteLineAsync($" /// {message.Contact?.PhoneNumber} ///");

                var user = new User
                {
                    Id = message.From.Id,
                    FirstName = message.From.FirstName,
                    LastName = message.From.LastName,
                    PhoneNumber = message.Contact?.PhoneNumber,
                };

                users = GetCustomers();
                var existingUser = users.FirstOrDefault(u => u.Id == user.Id);

                if (existingUser != null)
                {
                    existingUser.PhoneNumber = user.PhoneNumber;
                    await SaveUsersAsync(users);
                }
            }
        }
        else if (update.MyChatMember is not null)
        {
            Console.WriteLine("Something went wrong!");
        }
        else
        {
            await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Share your contact",
            replyMarkup: new ReplyKeyboardMarkup(new[]
                {
                    new[] { KeyboardButton.WithRequestContact("Share Contact") },

                })
            );

            var user = new User
            {
                Id = message.From.Id,
                FirstName = message.From.FirstName,
                LastName = message.From.LastName,
                PhoneNumber = message.Contact?.PhoneNumber,
            };

            var users = GetCustomers();
            users.Add(user);

            await SaveUsersAsync(users);
        }

        if (message != null)
        {
            if (message.Text == "Orders")
            {
                // Load the orders for the customer
                var orders = LoadOrdersForCustomer(message.Chat.Id);

                if (orders.Count > 0)
                {
                    // Format and send the orders to the customer
                    var ordersText = FormatOrdersText(orders);
                    await botClient.SendTextMessageAsync(message.Chat.Id, ordersText);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have no orders yet.");
                }
            }
            else if (message.Text == "Categories")
            {
                var categories = await GetCategoriesAsync();
                var buttons = new List<KeyboardButton[]>();

                foreach (var category in categories)
                {
                    var button = new KeyboardButton(category.Name);
                    buttons.Add(new KeyboardButton[] { button });
                }

                var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons);
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Please select a category:",
                    replyMarkup: replyKeyboardMarkup
                );
            }
            else if (GetProducts().Any(p => p.CategoryName == message.Text))
            {
                var products = GetProducts().Where(p => p.CategoryName == message.Text).ToList();
                var buttons = new List<KeyboardButton[]>();

                foreach (var product in products)
                {
                    var button = new KeyboardButton(product.Name);
                    buttons.Add(new KeyboardButton[] { button });
                }

                var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons);
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Please select a product:",
                    replyMarkup: replyKeyboardMarkup
                );
            }
            else if (message.Text == "Mening Savatim")
            {
                var boxes = GetBoxes();
                if (boxes.DefaultIfEmpty() != null)
                {
                    var box = boxes.FirstOrDefault(b => b.UserId == message.From.Id);
                    if (box is null)
                    {
                        await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "No products found!"
                            );
                    }
                    else if (box.AddedProducts.Count() != 0)
                    {
                        var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();
                        decimal totalPrice = 0;

                        await botClient.SendTextMessageAsync(message.Chat.Id, "📥 Box:");

                        var distinctMeals = box.AddedProducts.DistinctBy(m => m.Name);

                        foreach (var meal in distinctMeals)
                        {
                            var quantity = box.AddedProducts.Count(m => m.Name == meal.Name);
                            var price = meal.Price * quantity;
                            totalPrice += price;

                            var mealText = $"{quantity}. {meal.Name}\n{quantity} x {meal.Price} = {price} $";

                            await botClient.SendTextMessageAsync(message.Chat.Id, mealText);
                            var orderButton = InlineKeyboardButton.WithCallbackData("Order");
                            var plusButton = InlineKeyboardButton.WithCallbackData($"+", $"{meal.Name}");
                            var minusButton = InlineKeyboardButton.WithCallbackData($"-", $"{meal.Name}__minus");
                            var removeProductButton = InlineKeyboardButton.WithCallbackData($" X {meal.Name} X ", $"{meal.Name}__delete");
                            var quantityButton = InlineKeyboardButton.WithCallbackData($"{quantity}");
                            inlineKeyboardButtons.Add(new[] { removeProductButton });
                            inlineKeyboardButtons.Add(new[] { minusButton, quantityButton, plusButton });
                            inlineKeyboardButtons.Add(new[] { orderButton });
                        }

                        var totalPriceText = $"\n\nOverall: {totalPrice} $";
                        await botClient.SendTextMessageAsync(message.Chat.Id, totalPriceText);

                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButtons);

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "To place your order, click the button below:",
                            replyMarkup: inlineKeyboardMarkup
                        );

                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "No products found!"
                            );
                    }
                }

            }

            if (GetProducts().Any(m => m.Name == message.Text))
            {
                var meals = GetProducts();
                var meal = meals.FirstOrDefault(m => m.Name == message.Text);

                if (meal != null)
                {
                    using (var webClient = new WebClient())
                    {
                        byte[] photoBytes = webClient.DownloadData(meal.PictureUrl);
                        using (var memoryStream = new MemoryStream(photoBytes))
                        {
                            var photo = new InputFileStream(memoryStream, "photo.jpg");
                            await botClient.SendPhotoAsync(
                                chatId: message.Chat.Id,
                                photo: photo,
                                caption: message.Text
                            );
                        }
                    }

                    var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Add to box", $"{meal.Name}")
                        }
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Meal: {meal.Name}\nDescription: {meal.Description}\nPrice: {meal.Price}",
                        replyMarkup: inlineKeyboardMarkup
                    );

                }
            }
        }
    }

    public MyLocation ConvertToMyLocation(Location telegramLocation)
    {
        MyLocation myLocation = new MyLocation();

        myLocation.Latitude = telegramLocation.Latitude;
        myLocation.Longitude = telegramLocation.Longitude;

        return myLocation;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _dbContext.Categories.ToListAsync();
    }

    public async Task<List<Models.Regions.Region>> GetRegionsAsync()
    {
        string jsonData = await System.IO.File.ReadAllTextAsync(Constants.REGIONSPATH);
        return JsonConvert.DeserializeObject<List<Models.Regions.Region>>(jsonData);
    }

    public string FormatLocation(Location location)
    {
        return $"Latitude: {location.Latitude}, Longitude: {location.Longitude}";
    }

    public string FormatOrdersText(List<Order> orders)
    {
        var ordersText = "Your orders:\n\n";

        foreach (var order in orders)
        {
            ordersText += $"Order location: Lat:{_dbContext.MyLocation.FirstOrDefault(m => m.Id == order.LocationId).Latitude} Long : {_dbContext.MyLocation.FirstOrDefault(m => m.Id == order.LocationId).Longitude}\n";
            ordersText += $"Total Amount: {order.TotalAmount} $\n";
            ordersText += $"Date: {order.Date.ToString("yyyy-MM-dd HH:mm:ss")}\n\n";
        }

        return ordersText;
    }

    public int GenerateOrderNumber()
    {
        Random random = new Random();
        return random.Next(100, 10000);
    }

    public async Task<List<Order>> LoadOrdersAsync()
    {
        return await _dbContext.Orders.ToListAsync();
    }

    public List<Order> LoadOrdersForCustomer(long customerId)
    {
        return _dbContext.Orders.Where(o => o.CustomerId == customerId).ToList();
    }

    public async Task SaveOrdersAsync(List<Order> orders, Order order)
    {
        //_dbContext.Orders.AddRange(orders);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveBoxesAsync(List<GiftBox> boxes)
    {
        string boxJson = JsonConvert.SerializeObject(boxes, Formatting.Indented);

        // Write the user JSON to the users.json file
        string filePath = Constants.GIFTBOXPATH;
        await System.IO.File.WriteAllTextAsync(filePath, boxJson);
    }

    public List<User> GetCustomers()
    {
        return _dbContext.Users.ToList();
    }

    public List<Product> GetProducts()
    {
        return _dbContext.Products.ToList();
    }

    public List<GiftBox> GetBoxes()
    {
        string jsonContent = System.IO.File.ReadAllText(Constants.GIFTBOXPATH);
        return JsonConvert.DeserializeObject<List<GiftBox>>(jsonContent);
    }

    public Product GetProduct(string name)
    {
        return _dbContext.Products.FirstOrDefault(p => p.Name == name);
    }

    public async Task SaveUsersAsync(List<User> users)
    {
        _dbContext.Users.AddRange(users);
        await _dbContext.SaveChangesAsync();
    }

    public async static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}