using System.Text.Json;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using YandexCloudFunctions.Net.Sdk;

namespace AetherhubEloFunctions;

public class TelegramWebhookFunction() : BaseFunctionHandler(HandleAsync)
{
    private static async Task<FunctionHandlerResponse> HandleAsync(
        FunctionHandlerRequest request,
        ITelegramBotClient botClient,
        FirestoreDb firestoreDb,
        ILogger<TelegramWebhookFunction> logger)
    {
        var update = JsonSerializer.Deserialize<Update>(request.body, JsonSerializerOptions)!;
        logger.LogInformation("Received update {update}", JsonSerializer.Serialize(update));

        var message = update.Message;
        if (message?.Text != null)
        {
            if (message.Text.StartsWith("/communixes"))
            {
                var documents = firestoreDb
                    .Collection("communixes")
                    .ListDocumentsAsync();
                var communixes = new List<string>();
                await foreach (var document in documents)
                {
                    communixes.Add((await document.GetSnapshotAsync()).GetValue<string>("name"));
                }

                await botClient.SendMessage(
                    message.Chat.Id,
                    string.Join('\n', communixes));
            }
            else
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    $"Неизвестная команда {message.Text}");
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
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramAccessToken));
    }

    private static string GetConfigurationValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
            throw new Exception($"{name} not found");

        return value;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}