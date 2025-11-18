using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Jose;
using AetherhubEloFunctions.YandexCloud;

namespace AetherhubEloFunctions.Tests.YandexCloud;

public class YandexExternalIAMTokenProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<YandexExternalIAMTokenProvider> logger,
    IOptions<YandexExternalIAMTokenProviderOptions> options) : IYandexIAMTokenProvider
{
    public async Task<string> GetToken()
    {
        try
        {
            var iamToken = await ObtainExternal();

            logger.LogDebug($"IAM-токен получен успешно!");

            return iamToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении IAM-токена: {Message}", ex.Message);
            throw;
        }
    }

    private async Task<string> ObtainExternal()
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                jwt = CreateJwtToken(options.Value.ServiceAccountId, options.Value.KeyId, options.Value.PrivateKey)
            }),
            Encoding.UTF8,
            "application/json");

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(
            "https://iam.api.cloud.yandex.net/iam/v1/tokens",
            content
        );

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

        var iamToken = result.GetProperty("iamToken").GetString();
        return iamToken!;
    }

    private static string CreateJwtToken(string serviceAccountId, string keyId, string privateKey)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var headers = new Dictionary<string, object>()
            {
                { "kid", keyId }
            };

        var payload = new Dictionary<string, object>()
            {
                { "aud", "https://iam.api.cloud.yandex.net/iam/v1/tokens" },
                { "iss", serviceAccountId },
                { "iat", now },
                { "exp", now + 3600 }
            };

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey.ToCharArray());
        return JWT.Encode(payload, rsa, JwsAlgorithm.PS256, headers);
    }
}