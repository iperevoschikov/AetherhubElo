using AetherhubEloFunctions.YandexCloud;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneyFetcher(AetherhubRendererContainerClient rendererContainerClient)
{
    public async Task<(DateOnly Date, Round[] Rounds)> FetchTourney(int id)
    {
        var html = await rendererContainerClient.Render(
            $"https://aetherhub.com/Tourney/{id}",
            "#matchList tbody tr:nth-child(1)");
        return await AetherhubTourneyParser.ParseTourney(html);
    }
}
