using AetherhubEloFunctions.YandexCloud;
using AngleSharp;
using AngleSharp.Dom;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneysFetcher(
    YandexCloudClient yandexCloudClient
)
{
    public async IAsyncEnumerable<TourneyMeta> FetchRecentTourneys()
    {
        var aetherhubRendererUri = "https://bbavmv503kd9ksfsd3c5.containers.yandexcloud.net/?url=https://aetherhub.com/Tourney/?search=EDINOROG_EKB";
        var html = await yandexCloudClient.InvokeServerlessContainer(aetherhubRendererUri);
        await foreach (var meta in AetherhubTourneysListParser.Parse(html))
            yield return meta;
    }
}

