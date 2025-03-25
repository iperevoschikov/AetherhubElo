using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using YandexCloudFunctions.Net.Sdk;

namespace AetherhubEloFunctions;

public class TelegramWebhookFunction() : BaseFunctionHandler(HandleAsync)
{
    private static async Task<FunctionHandlerResponse> HandleAsync(
        FunctionHandlerRequest request,
        ITelegramBotClient botClient,
        FirestoreDb firestoreDb)
    {
        var update = JsonConvert.DeserializeObject<Update>(request.body)!;
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
}