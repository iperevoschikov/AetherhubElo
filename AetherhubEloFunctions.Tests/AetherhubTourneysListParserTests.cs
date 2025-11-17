using AetherhubEloFunctions.Aetherhub;

namespace AetherhubEloFunctions.Tests;

public class AetherhubTourneysListParserTests
{
    [Test]
    public async Task TestParsingSuccessful()
    {
        var html = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.WorkDirectory, "tourneys-list.html"));
        var result = await AetherhubTourneysListParser.Parse(html).ToArrayAsync();
        Assert.That(result, Has.Length.EqualTo(20));
    }
}