using System.Text.Json;
using System.Text.Json.Serialization;
using Yandex.Cloud.Functions;

namespace AetherhubEloFunctions.YandexCloud;

public class YandexInternalIAMTokenProvider(Context context) : IYandexIAMTokenProvider
{
    public async Task<string> GetToken()
    {
        return JsonSerializer.Deserialize<Token>(context.TokenJson)!.AccessToken;
    }

    private class Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}