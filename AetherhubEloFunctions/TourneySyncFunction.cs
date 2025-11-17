using AetherhubEloFunctions.Aetherhub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YandexCloudFunctions.Net.Sdk;

namespace AetherhubEloFunctions;

public class TourneySyncFunction() : MessageQueueFunctionHandler(HandleAsync)
{
    private static async Task<string> HandleAsync(
        AetherhubTourneysFetcher tourneysFetcher,
        TourneysStorage tourneysStorage,
        CommunixGuesser communixGuesser,
        ILogger<TourneySyncFunction> logger
    )
    {
        var tourneys = await tourneysStorage.GetTourneys().Select(t => t.AetherhubId).ToListAsync();
        var recentTourneys = tourneysFetcher
            .FetchRecentTourneys()
            .Where(r => !tourneys.Contains(r.ExternalId));

        await foreach (var newTourney in recentTourneys)
        {
            var (date, rounds) = await AetherhubTourneyParser.ParseTourney(newTourney.ExternalId);
            var communix = await communixGuesser.GuessCommunix(newTourney);
            if (communix != null)
            {
                await tourneysStorage.WriteTourney(
                    new Tourney(Guid.NewGuid(), newTourney.ExternalId, communix, date, rounds)
                );
            }
            else
            {
                logger.LogInformation(
                    "Communix not found for tourney: {Name}, {Date:d}",
                    newTourney.Name,
                    newTourney.Date
                );
            }
        }

        return string.Empty;
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .ConfigureStorage()
            .ConfigureAetherhub();
    }
}

