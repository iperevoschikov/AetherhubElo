using AetherhubEloFunctions.YandexCloud;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Tests.YandexCloud;

public static class ContainerConfiguration
{
    public static IServiceCollection ConfigureYandexCloud(this IServiceCollection services)
    {
        return services
            .AddHttpClient()
            .AddSingleton<YandexCloudClient>()
            .Configure<YandexExternalIAMTokenProviderOptions>(
                new ConfigurationBuilder()
                .AddJsonFile("YandexCloud/yandex-cloud-settings.json")
                .Build())
            .AddSingleton<IYandexIAMTokenProvider, YandexExternalIAMTokenProvider>();
    }
}