using AetherhubEloFunctions.Aetherhub;
using AetherhubEloFunctions.Tests.YandexCloud;
using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Tests;

public class AetherhubTourneysFetcherTests
{
    [Test]
    public async Task TestFetchAetherhubTourneys()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddHttpClient()
            .AddSingleton<AetherhubTourneysFetcher>()
            .ConfigureYandexCloud()
            .BuildServiceProvider();

        var fetcher = provider.GetRequiredService<AetherhubTourneysFetcher>();
        var result = await fetcher.FetchRecentTourneys().ToArrayAsync();
        Assert.That(result, Has.Length.EqualTo(20));
        var latest = result[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(latest.Name, Is.Not.Null.Or.Empty);
            Assert.That(latest.Date, Is.GreaterThan(new DateOnly(2025, 1, 1)));
        }
    }
}

