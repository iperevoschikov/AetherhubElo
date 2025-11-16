using AetherhubEloFunctions.Aetherhub;
using AetherhubEloFunctions.Commands;
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
        IServiceProvider serviceProvider,
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

                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: $"Ок, выбран {communix.Name}",
                        showAlert: false
                    );

                    await botClient.EditMessageText(
                        chatId: callbackQuery.Message!.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        text: $"Выбран {communix.Name}"
                    );
                    break;
                default:
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Не понял что делать",
                        showAlert: false
                    );
                    break;
            }
        }
        else if (message is { Text: not null, From: not null })
        {

            var (state, _) = await usersStorage.GetUserState(message.From.Id);
            var commandProcessors = serviceProvider.GetServices<CommandProcessor>();
            var processor = commandProcessors
            .OrderByDescending(p => p.Order)
            .FirstOrDefault(p => p.CanProcess(state, message.Text));
            if (processor != null)
            {
                await processor.ProcessCommand(MessageMeta.FromMessage(message));
                return WebhookHandlerResponses.Ok();
            }
            else
            {
                logger.LogWarning(
                    "No processor found for state {state} and message {message}",
                    state,
                    message.Text
                );
            }
        }

        return WebhookHandlerResponses.Ok();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        var telegramAccessToken = Configuration.GetConfigurationValue("TG_ACCESS_TOKEN");
        services
            .ConfigureStorage()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken))
            .AddSingleton<AetherhubTourneysFetcher>()
            .AddHttpClient()
            .ConfigureCommands();
    }
}
