using AetherhubEloFunctions.YandexCloud;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubRendererContainerClient(YandexCloudClient yandexCloudClient)
{
    private const string containerUrl = "https://bbavmv503kd9ksfsd3c5.containers.yandexcloud.net/";

    public async Task<string> Render(string url, string locator)
    {
        var aetherhubRendererUri = $"{containerUrl}?url={Uri.EscapeDataString(url)}&locator={Uri.EscapeDataString(locator)}";
        var html = await yandexCloudClient.InvokeServerlessContainer(aetherhubRendererUri);
        return html;
    }
}
