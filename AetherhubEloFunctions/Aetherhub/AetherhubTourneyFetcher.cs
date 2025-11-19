using AetherhubEloFunctions.YandexCloud;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneyFetcher(AetherhubRendererContainerClient containerClient)
{
    public async IAsyncEnumerable<TourneyMeta> FetchTourney(int id)
    {
        var aetherhubRendererUri =
            "https://bbavmv503kd9ksfsd3c5.containers.yandexcloud.net/?url=https://aetherhub.com/Tourney/?search=EDINOROG_EKB";
        var html = await containerClient.Render().InvokeServerlessContainer(aetherhubRendererUri);
        await foreach (var meta in AetherhubTourneysListParser.Parse(html))
            yield return meta;
    }
}
