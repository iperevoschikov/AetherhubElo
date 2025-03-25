using System.Text.Json;
using Telegram.Bot.Types;

namespace AetherhubEloFunctions.Tests;

public class DeserializationTests
{
    [Test]
    public void TestUpdateDeserialize()
    {
        const string updateJson =
            """
            {"update_id":535242998,"message":{"message_id":14,"from":{"id":222,"is_bot":false,"first_name":"John","last_name":"Smith","username":"username","language_code":"ru","is_premium":true},"chat":{"id":22222,"first_name":"John","last_name":"Smith","username":"john_smith","type":"private"},"date":1742904147,"text":"/newcommunix","entities":[{"offset":0,"length":12,"type":"bot_command"}]}}
            """;
        var update = JsonSerializer.Deserialize<Update>(
            updateJson,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        Assert.That(update.Message, Is.Not.Null);
    }
}