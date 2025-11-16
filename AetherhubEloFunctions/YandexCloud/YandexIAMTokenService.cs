using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AetherhubEloFunctions.YandexCloud;

public class YandexIAMTokenService(
    IHttpClientFactory httpClientFactory,
    ILogger<YandexIAMTokenService> logger,
    IOptions<YandexIAMTokenServiceOptions> options)
{
    // Метод для получения IAM-токена с использованием сервисного аккаунта (authorized key)
    public async Task<string> GetIamTokenFromServiceAccountAsync()
    {
        try
        {
            var (serviceAccountId, keyId, privateKey) = options.Value;
            // Создаем JWT токен
            var jwtToken = CreateJwtToken(serviceAccountId, keyId, privateKey);


            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    jwt = jwtToken
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
            var expiresAt = result.GetProperty("expiresAt").GetString();

            logger.LogDebug($"IAM-токен получен успешно!");
            logger.LogDebug("Срок действия до: {expiresAt}", expiresAt);

            return iamToken!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении IAM-токена: {Message}", ex.Message);
            throw;
        }
    }

    // Вспомогательный метод для создания JWT (требует дополнительной библиотеки)
    private static string CreateJwtToken(string serviceAccountId, string keyId, string privateKey)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(1);

        var claims = new[]
        {
            new Claim("iss", serviceAccountId),
            new Claim("sub", serviceAccountId),
            new Claim("aud", "https://iam.api.cloud.yandex.net/iam/v1/tokens"),
            new Claim("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
            new Claim("exp", new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString())
        };

        var key = new RsaSecurityKey(
            System.Security.Cryptography.RSA.Create())
        {
            KeyId = keyId
        };

        // Load private key from PEM format
        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportFromPem(privateKey.ToCharArray());
            key = new RsaSecurityKey(rsa) { KeyId = keyId };
        }

        var signingCredentials = new SigningCredentials(
            key, SecurityAlgorithms.RsaSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials);

        return handler.WriteToken(token);
    }
}