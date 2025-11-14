using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions;

public static class Configuration
{
    public static string GetConfigurationValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
            throw new Exception($"{name} not found");

        return value;
    }

    public static IServiceCollection ConfigureStorage(this IServiceCollection services)
    {
        var googleCloudJsonCredentials =
            System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    GetConfigurationValue("GOOGLE_CLOUD_JSON_CREDENTIALS")));

        var credentials = CredentialFactory.FromJson<GoogleCredential>(googleCloudJsonCredentials);

        return services
            .AddSingleton(
                new FirestoreDbBuilder
                {
                    ProjectId = "mtg-ekb-elo",
                    GoogleCredential = credentials,
                }
                    .Build())
            .AddSingleton<CommunixesStorage>()
            .AddSingleton<UsersStorage>()
            .AddSingleton<TourneysStorage>();
    }
}