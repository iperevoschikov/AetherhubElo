using AetherhubEloFunctions.Aetherhub;
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
        AetherhubTourneysFetcher tourneysFetcher,
        ILogger<TelegramWebhookFunction> logger
    )
    {
        var update = JsonConvert.DeserializeObject<Update>(request.body)!;
        logger.LogInformation(
            "Received update {update}",
            System.Text.Json.JsonSerializer.Serialize(update)
        );

        var message = update.Message;
        var callbackQuery = update.CallbackQuery;

        Task Respond(string response)
        {
            return botClient.SendTextMessageAsync(message.Chat.Id, response);
        }

        if (callbackQuery is { Data: not null })
        {
            var (state, _) = await usersStorage.GetUserState(callbackQuery.From.Id);
            switch (state)
            {
                case UserState.SelectCommunix:
                    var communix = (await communixesStorage.GetAll()).Single(c =>
                        c.Id == callbackQuery.Data
                    );
                    await usersStorage.SetUserState(callbackQuery.From.Id, UserState.Default);

                    await usersStorage.SetUserCommunix(callbackQuery.From.Id, communix.Id);

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
                            await usersStorage.SetUserState(
                                message.From.Id,
                                UserState.SelectCommunix
                            );
                            var communixes = await communixesStorage.GetAll();
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Выбери комуникс",
                                replyMarkup: new InlineKeyboardMarkup(
                                    communixes
                                        .Select(c =>
                                            InlineKeyboardButton.WithCallbackData(c.Name, c.Id)
                                        )
                                        .ToArray()
                                )
                            );
                            break;

                        case "/global":
                            await Respond(
                                PrintRatings(
                                    RatingCalculator.CalculateRatings(
                                        await tourneysStorage
                                            .GetTourneys()
                                            .OrderBy(t => t.Date)
                                            .ToArrayAsync()
                                    )
                                )
                            );
                            break;

                        case "/rating":

                            if (userCommunix == null)
                            {
                                await Respond("Сначала нужно выбрать твой комуникс /communix");
                                break;
                            }

                            await Respond(
                                PrintRatings(
                                    RatingCalculator.CalculateRatings(
                                        await tourneysStorage
                                            .GetTourneys()
                                            .Where(t => t.Communix == userCommunix)
                                            .OrderBy(t => t.Date)
                                            .ToArrayAsync()
                                    )
                                )
                            );
                            break;
                        case "/addresults":
                            if (await usersStorage.GetUserCommunix(message.From.Id) == null)
                            {
                                await Respond("Сначала нужно выбрать твой комуникс /communix");
                            }
                            else
                            {
                                await usersStorage.SetUserState(
                                    message.From.Id,
                                    UserState.AddResultsAwaitUrl
                                );

                                await Respond("Отправь ссылку на турнир Aetherhub");
                            }

                            break;
                        case "/pairings":
                            var recent = await tourneysFetcher
                                .FetchRecentTourneys()
                                .FirstOrDefaultAsync();
                            if (recent != null)
                            {
                                var lastTourney = await AetherhubTourneyParser.ParseTourney(
                                    recent.ExternalId
                                );
                                var lastRound = lastTourney.Rounds.LastOrDefault();
                                if (lastRound != null)
                                {
                                    await Respond(
                                        string.Join(
                                            '\n',
                                            lastRound.Games.Select(g =>
                                                $"{g.Player1} - {g.Player2}"
                                            )
                                        )
                                    );
                                }
                                else
                                {
                                    await Respond("Не нашел ни одного раунда");
                                    break;
                                }
                            }
                            else
                                await Respond("Не нашел ни одного турнира");
                            break;
                        case "/start":
                        case "/help":
                        default:
                            await Respond(
                                "Это бот для подсчета рейтинга игроков MTG на локальных турнирах.\n"
                                    + "Чтобы добавить результаты турнира или посмотреть рейтинг, воспользуйтесь соответствующей командой."
                            );
                            break;
                    }

                    break;
                case UserState.AddResultsAwaitUrl:
                    if (userCommunix == null)
                    {
                        await Respond("Сначала нужно выбрать твой комуникс /communix");
                    }
                    else
                    {
                        if (
                            !Aetherhub.AetherhubTourneyParser.TryParseAetherhubTourneyIdFromUrl(
                                message.Text,
                                out var tourneyId
                            )
                        )
                            await Respond(
                                "Не смог разобрать урл. Он должен выглядеть вот так: https://aetherhub.com/Tourney/RoundTourney/38072"
                            );
                        else
                        {
                            var tourneys = tourneysStorage
                                .GetTourneys()
                                .Where(t => t.AetherhubId == tourneyId);
                            if (await tourneys.AnyAsync())
                            {
                                await Respond("Турнир был добавлен ранее");
                            }
                            else
                            {
                                var (date, rounds) =
                                    await Aetherhub.AetherhubTourneyParser.ParseTourney(tourneyId);
                                await tourneysStorage.WriteTourney(
                                    new Tourney(
                                        Guid.NewGuid(),
                                        tourneyId,
                                        userCommunix,
                                        date,
                                        rounds
                                    )
                                );
                                await Respond("Турнир сохранён");
                            }
                        }
                    }

                    await usersStorage.SetUserState(message.From.Id, UserState.Default);
                    break;
                default:
                    logger.LogWarning("Something went wrong");
                    await usersStorage.SetUserState(message.From.Id, UserState.Default);
                    await Respond("Я тебя не понимаю, давай начнем сначала");
                    break;
            }
        }

        return WebhookHandlerResponses.Ok();
    }

    private static string PrintRatings(Dictionary<string, double> ratings)
    {
        return ratings.Count != 0
            ? string.Join(
                '\n',
                ratings
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => $"{kvp.Key}: {kvp.Value:N0}")
            )
            : "Пока не было добавлено никаких результатов (/addresults)";
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        var telegramAccessToken = Configuration.GetConfigurationValue("TG_ACCESS_TOKEN");
        services
            .ConfigureStorage()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken));
    }
}

