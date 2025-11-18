using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Tests.YandexCloud;

public class YandexIAMTokenServiceTests
{
    [Test]
    public async Task TestObtainToken()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .ConfigureYandexCloud()
            .AddSingleton<YandexExternalIAMTokenProvider>()
            .BuildServiceProvider();

        var service = provider.GetRequiredService<YandexExternalIAMTokenProvider>();
        var token = await service.GetToken();
        Assert.That(token, Is.Not.Null);
    }
}