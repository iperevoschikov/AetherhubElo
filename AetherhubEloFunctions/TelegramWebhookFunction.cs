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
        UsersStorage usersStorage,
        TourneysStorage tourneysStorage,
        ILogger<TelegramWebhookFunction> logger)
    {
        var update = JsonConvert.DeserializeObject<Update>(request.body)!;
        logger.LogInformation("Received update {update}", System.Text.Json.JsonSerializer.Serialize(update));

        var message = update.Message;
        var callbackQuery = update.CallbackQuery;

        if (callbackQuery is { Data: not null })
        {
            var (state, _) = await usersStorage.GetUserState(callbackQuery.From.Id);
            switch (state)
            {
                case UserState.AddResultsSelectCommunix:
                    var communix = (await communixesStorage.GetAll()).Single(c => c.Id == callbackQuery.Data);
                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: $"Ок, выбран {communix.Name}",
                        showAlert: false
                    );

                    await botClient.EditMessageTextAsync(
                        chatId: callbackQuery.Message!.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        text: $"Выбран {communix.Name}"
                    );

                    await usersStorage.SetUserState(
                        callbackQuery.From.Id,
                        UserState.AddResultsAwaitUrl,
                        communix.Id);

                    await botClient.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Теперь отправь ссылку на турнир Aetherhub");

                    break;
                default:
                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: "Не понял что делать",
                        showAlert: false
                    );
                    break;
            }
        }
        else if (message is { Text: not null, From: not null })
        {
            var (state, data) = await usersStorage.GetUserState(message.From.Id);
            switch (state)
            {
                case UserState.Default:
                    switch (message.Text)
                    {
                        case "/rating":
                            await usersStorage.SetUserState(message.From.Id, UserState.Rating);
                            break;
                        case "/addresults":
                            await usersStorage.SetUserState(message.From.Id, UserState.AddResultsSelectCommunix);
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

                    break;
                case UserState.AddResultsAwaitUrl:
                    if (!AetherhubTourneyParser.TryParseAetherhubTourneyIdFromUrl(message.Text, out var tourneyId))
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Не смог разобрать урл. Он должен выглядеть вот так: https://aetherhub.com/Tourney/RoundTourney/38072");
                    else
                    {
                        var tourneys = tourneysStorage.GetTourneys().Where(t => t.AetherhubId == tourneyId);
                        if (await tourneys.AnyAsync())
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Турнир был добавлен ранее");
                        var (date, rounds) = await AetherhubTourneyParser.ParseTourney(tourneyId);
                        await tourneysStorage.WriteTourney(new Tourney(Guid.NewGuid(), tourneyId, data!, date, rounds));
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Турнир сохранён");
                    }

                    break;
                default:
                    logger.LogWarning("Something went wrong");
                    await usersStorage.SetUserState(message.From.Id, UserState.Default);
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Я тебя не понимаю, давай начнем сначала");
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
            .AddSingleton<UsersStorage>()
            .AddSingleton<TourneysStorage>()
            .AddSingleton<AetherhubTourneyParser>()
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