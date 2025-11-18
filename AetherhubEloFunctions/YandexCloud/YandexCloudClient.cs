using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace AetherhubEloFunctions.YandexCloud;

public class YandexCloudClient(
    ILogger<YandexCloudClient> logger,
    IHttpClientFactory httpClientFactory,
    IYandexIAMTokenProvider tokenProvider)
{
    public async Task<string> InvokeServerlessContainer(string requestUri)
    {
        var token = await tokenProvider.GetToken();
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error occurred while invoking serverless container ({containerUrl})", requestUri);
            throw;
        }
    }
}