using AetherhubEloFunctions.YandexCloud;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneysListFetcher(AetherhubRendererContainerClient rendererContainerClient)
{
    public async IAsyncEnumerable<TourneyMeta> FetchRecentTourneys()
    {
        var html = await rendererContainerClient.Render(
            "https://aetherhub.com/Tourney/?search=EDINOROG_EKB",
            "#Tourneys tr:nth-child(19)");
        await foreach (var meta in AetherhubTourneysListParser.Parse(html))
            yield return meta;
    }
}
