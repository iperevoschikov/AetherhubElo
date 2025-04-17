using AetherhubEloFunctions.Aetherhub;
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
            .BuildServiceProvider();

        var fetcher = provider.GetRequiredService<AetherhubTourneysFetcher>();
        var result = await fetcher.FetchRecentTourneys();
        Assert.That(result, Has.Length.EqualTo(20));
        var latest = result[0];
        Assert.That(latest.Name, Is.Not.Null.Or.Empty);
        Assert.That(latest.Date, Is.GreaterThan(new DateOnly(2025, 1, 1)));
    }
}