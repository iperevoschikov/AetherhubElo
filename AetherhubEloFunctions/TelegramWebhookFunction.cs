using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YandexCloudFunctions.Net.Sdk;

namespace AetherhubEloFunctions;

public class TelegramWebhookFunction() : BaseFunctionHandler(HandleAsync)
{
    private static async Task<FunctionHandlerResponse> HandleAsync(
        FunctionHandlerRequest request,
        ITelegramBotClient botClient,
        CommunixesStorage communixesStorage,
        ILogger<TelegramWebhookFunction> logger)
    {
        var update = JsonConvert.DeserializeObject<Update>(request.body)!;
        logger.LogInformation("Received update {update}", System.Text.Json.JsonSerializer.Serialize(update));

        var message = update.Message;
        if (message?.Text != null)
        {
            switch (message.Text)
            {
                case "/rating":
                    break;
                case "/addresults":
                    var communixes = await communixesStorage.GetAll();
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Выбери комуникс",
                        replyMarkup: new InlineKeyboardMarkup(
                            communixes.Select(c =>
                                    InlineKeyboardButton.WithCallbackData(c.Name, c.Id))
                                .ToArray()));
                    break;
                case "/start":
                case "/help":
                default:
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Это бот для подсчета рейтинга игроков MTG на локальных турнирах.\n" +
                        "Чтобы добавить результаты турнира или посмотреть рейтинг, воспользуйтесь соответствующей командой.");
                    break;
            }
        }

        return FunctionHandlerResponses.Ok();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        var telegramAccessToken = GetConfigurationValue("TG_ACCESS_TOKEN");
        var googleCloudJsonCredentials =
            System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    GetConfigurationValue("GOOGLE_CLOUD_JSON_CREDENTIALS")));

        services
            .AddSingleton(
                new FirestoreDbBuilder
                    {
                        ProjectId = "mtg-ekb-elo",
                        JsonCredentials = googleCloudJsonCredentials,
                    }
                    .Build())
            .AddSingleton<CommunixesStorage>()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken));
    }

    private static string GetConfigurationValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
            throw new Exception($"{name} not found");

        return value;
    }
}