using AetherhubEloFunctions.YandexCloud;
using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Aetherhub;

public static class ContainerConfiguration
{
    public static IServiceCollection ConfigureAetherhub(this IServiceCollection services)
    {
        return services
                    .AddSingleton<AetherhubTourneysFetcher>()
                    .AddSingleton<YandexCloudClient>()
                    .AddSingleton<IYandexIAMTokenProvider, YandexInternalIAMTokenProvider>()
                    .AddSingleton<CommunixGuesser>()
                    .AddHttpClient();
    }
}