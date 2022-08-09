using AutoMapper;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using TGBot.BusinessLogic.Interfaces;
using TGBot.BusinessLogic.Implementations;
using TGBot.Common;
using Microsoft.EntityFrameworkCore;
using TGBot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.Common.Mapper;

static void ConfigurationBuild(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables();
}


var builder = new ConfigurationBuilder();
ConfigurationBuild(builder);
builder.SetBasePath(Directory.GetCurrentDirectory());
builder.AddJsonFile("appsettings.json");
var config = builder.Build();
string connection = config.GetConnectionString("DefaultConnection");

var mapperConfig = new MapperConfiguration(x => { x.AddProfile<MapperProfile>(); });
mapperConfig.AssertConfigurationIsValid();
IMapper mapper = mapperConfig.CreateMapper();

var host = Host.CreateDefaultBuilder()
.ConfigureServices((context, services) =>
{
    services.AddTransient<ISectionService, SectionService>();
    services.AddTransient<ITeacherService, TeacherService>();
    services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));
    services.AddSingleton(mapper);
})
.Build();

var sectionService = ActivatorUtilities.CreateInstance<SectionService>(host.Services);
var sectionService1 = ActivatorUtilities.CreateInstance<TeacherService>(host.Services);

ITelegramBotClient botClient = new TelegramBotClient("5592791639:AAEtZBn6aEOYv8omuc6ArPzXee-Ct_82M_8");
var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;
var receivverOptions = new ReceiverOptions
{
    AllowedUpdates = { }
};
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receivverOptions,
    cancellationToken: cts.Token);
Console.WriteLine("Бот" + " " + botClient.GetMeAsync().Result.FirstName + " " + "запущен");
Console.ReadKey();
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient telegramBot, Update update, CancellationToken cancellationToken)
{

    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;
    }
    if (update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;
    }

}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    string result = null;
    if (callbackQuery.Data.StartsWith("section"))
    {
        List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
        foreach (var item in sectionService.Gets())
        {
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(item.Name, item.Name) });
            result = item.Name;
            // sectionService.Get(item.Name);
        }
        InlineKeyboardMarkup keyboardMarkup = new(buttons.ToArray());
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Выберите секцию: ", replyMarkup: keyboardMarkup);

    }
    if (callbackQuery.Message.Text == "Каратэ")
    {
        GetData(botClient, callbackQuery.Message.Chat.Id, sectionService.DTO(result));
        //InlineKeyboardMarkup inline = 
        //await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{}")
        //foreach (var item in sectionService.Gets())
        //{
        //    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Название секции: {callbackQuery.Message.Text}");
        //    if (result == item.Name)
        //    {
        //        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Название секции: {item.Name}" + Environment.NewLine +
        //        $"Расписание {item.RunningTime}" + Environment.NewLine +
        //        $"Местоположение: {item.Location}");

        //        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Название секции: {callbackQuery.Message.Text}");
        //    }
        //}
    }


}

async Task GetData(ITelegramBotClient botClient, long id, SectionDTO sectionDTO)
{
    await botClient.SendTextMessageAsync(id, $"Название секции: {sectionDTO.Name}" + Environment.NewLine +
            $"Расписание {sectionDTO.RunningTime}" + Environment.NewLine +
            $"Местоположение: {sectionDTO.Location}");
}


async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
    ReplyKeyboardMarkup mark = new(new[]
    {
                    new KeyboardButton[]{"/start"}
    });

    InlineKeyboardMarkup markup = new(new[]
    {
           new[]{ InlineKeyboardButton.WithCallbackData("Секции ПолесГУ", "section")
           }
    });

    if (message.Text == "/start")
    {

        await botClient.SendTextMessageAsync(message.Chat.Id, "Нажмите на кнопку:", replyMarkup: markup);
        return;
    }

}

Task HandleErrorAsync(ITelegramBotClient telegramBot, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Ошибка Telegram API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}