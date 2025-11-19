namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneyFetcher(AetherhubRendererContainerClient rendererContainerClient)
{
    public async Task<(DateOnly Date, Round[] Rounds)> FetchTourney(int id)
    {
        var html = await rendererContainerClient.Render(
            $"https://aetherhub.com/Tourney/{id}",
            "#matchList tbody tr:nth-child(1)");
        var (date, roundLinks) = await AetherhubTourneyParser.ParseRounds(html);
        var rounds = new List<Round>();
        foreach (var link in roundLinks)
        {
            var roundHtml = await rendererContainerClient.Render(
                $"https://aetherhub.com{link}",
                "#matchList tbody tr:nth-child(1)"
            );
            var round = await AetherhubTourneyParser.ParseRound(roundHtml);
            rounds.Add(round);
        }
        return (date, [.. rounds]);
    }
}
