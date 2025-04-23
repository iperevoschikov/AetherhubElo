using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Tests;

public class DebugTests
{
    [Test]
    public async Task Debug()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureStorage()
            .BuildServiceProvider();

        var tourneysStorage = serviceProvider.GetRequiredService<TourneysStorage>();

        var tourneys = await tourneysStorage.GetTourneys().ToArrayAsync();
        var pioneer = tourneys.Where(t => t.Communix == "pioneer")
            .OrderByDescending(t => t.Date)
            .ToArray();
        Assert.That(pioneer, Has.Length.EqualTo(pioneer.Select(t => t.Date).Distinct().Count()));
    }
}