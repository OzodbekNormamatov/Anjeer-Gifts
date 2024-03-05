using Anjeer_Gifts.Configuration;
using Anjeer_Gifts.Services;
using System.Reflection.Metadata;
using Telegram.Bot;

var client = new TelegramBotClient(Constants.BOTTOKEN);

TelegramService telegramService = new TelegramService(client);

await telegramService.Run();