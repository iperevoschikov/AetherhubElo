using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YandexCloudFunctions.Net.Sdk;
using YandexCloudFunctions.Net.Sdk.Webhook;

namespace AetherhubEloFunctions;

public class TelegramWebhookFunction() : WebhookFunctionHandler(HandleAsync)
{
    private static async Task<WebhookHandlerResponse> HandleAsync(
        WebhookHandlerRequest request,
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
                case UserState.SelectCommunix:
                    var communix = (await communixesStorage.GetAll()).Single(c => c.Id == callbackQuery.Data);
                    await usersStorage.SetUserState(
                        callbackQuery.From.Id,
                        UserState.Default);

                    await usersStorage.SetUserCommunix(
                        callbackQuery.From.Id,
                        communix.Id);

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
            var userCommunix = await usersStorage.GetUserCommunix(message.From.Id);
            var (state, _) = await usersStorage.GetUserState(message.From.Id);
            switch (state)
            {
                case UserState.Default:
                    switch (message.Text)
                    {
                        case "/communix":
                            await usersStorage.SetUserState(message.From.Id, UserState.SelectCommunix);
                            var communixes = await communixesStorage.GetAll();
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Выбери комуникс",
                                replyMarkup: new InlineKeyboardMarkup(
                                    communixes.Select(c =>
                                            InlineKeyboardButton.WithCallbackData(c.Name, c.Id))
                                        .ToArray()));
                            break;

                        case "/global":
                            await botClient.SendTextMessageAsync(
                                message.From.Id,
                                PrintRatings(RatingCalculator
                                    .CalculateRatings(await tourneysStorage
                                        .GetTourneys()
                                        .ToArrayAsync())));
                            break;

                        case "/rating":

                            if (userCommunix == null)
                            {
                                await botClient.SendTextMessageAsync(
                                    message.From.Id,
                                    "Сначала нужно выбрать твой комуникс /communix");
                                break;
                            }

                            await botClient.SendTextMessageAsync(
                                message.From.Id,
                                PrintRatings(RatingCalculator
                                    .CalculateRatings(await tourneysStorage
                                        .GetTourneys()
                                        .Where(t => t.Communix == userCommunix)
                                        .ToArrayAsync())));
                            break;
                        case "/addresults":
                            if (await usersStorage.GetUserCommunix(message.From.Id) == null)
                            {
                                await botClient.SendTextMessageAsync(
                                    message.From.Id,
                                    "Сначала нужно выбрать твой комуникс /communix");
                            }
                            else
                            {
                                await usersStorage.SetUserState(message.From.Id, UserState.AddResultsAwaitUrl);

                                await botClient.SendTextMessageAsync(
                                    message.From.Id,
                                    "Отправь ссылку на турнир Aetherhub");
                            }

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
                    if (userCommunix == null)
                    {
                        await botClient.SendTextMessageAsync(
                            message.From.Id,
                            "Сначала нужно выбрать твой комуникс /communix");
                        break;
                    }

                    if (!AetherhubTourneyParser.TryParseAetherhubTourneyIdFromUrl(message.Text, out var tourneyId))
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Не смог разобрать урл. Он должен выглядеть вот так: https://aetherhub.com/Tourney/RoundTourney/38072");
                    else
                    {
                        var tourneys = tourneysStorage.GetTourneys().Where(t => t.AetherhubId == tourneyId);
                        if (await tourneys.AnyAsync())
                        {
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Турнир был добавлен ранее");
                        }
                        else
                        {
                            var (date, rounds) = await AetherhubTourneyParser.ParseTourney(tourneyId);
                            await tourneysStorage.WriteTourney(new Tourney(Guid.NewGuid(), tourneyId, userCommunix,
                                date,
                                rounds));
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Турнир сохранён");
                        }

                        await usersStorage.SetUserState(message.Chat.Id, UserState.Default);
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

        return WebhookHandlerResponses.Ok();
    }

    private static string PrintRatings(Dictionary<string, double> ratings)
    {
        return ratings.Count != 0
            ? string.Join('\n',
                ratings
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => $"{kvp.Key}: {kvp.Value:N0}"))
            : "Пока не было добавлено никаких результатов (/addresults)";
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