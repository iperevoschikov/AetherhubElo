using AetherhubEloFunctions.Aetherhub;

namespace AetherhubEloFunctions.Tests;

public class AetherhubTourneyParserTests
{
    [TestCase("https://aetherhub.com/Tourney/RoundTourney/38072", true, 38072)]
    [TestCase("https://aetherhub.com/Tourney/RoundTourney/38072?p=1", true, 38072)]
    [TestCase("https://www.mtggoldfish.com/tournament/standard-challenge-64-2025-03-25#paper", false, -1)]
    public void TestTourneyIdParsing(string url, bool succeed, int expected)
    {
        var result = AetherhubTourneyParser.TryParseAetherhubTourneyIdFromUrl(url, out var tourneyId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(succeed));
            Assert.That(tourneyId, Is.EqualTo(expected));
        }
    }

    [Test]
    public async Task TestTourneyParsing()
    {
        var html = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.WorkDirectory, "tourney.html"));
        var (date, rounds) = await AetherhubTourneyParser.ParseRounds(html);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date, Is.EqualTo(new DateOnly(2025, 03, 25)));
            Assert.That(rounds, Has.Length.EqualTo(3));
        }
    }
}