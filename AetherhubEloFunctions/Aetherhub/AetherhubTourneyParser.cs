using System.Text.RegularExpressions;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;

namespace AetherhubEloFunctions.Aetherhub;

public static partial class AetherhubTourneyParser
{
    public static async Task<(DateOnly Date, Round[] Rounds)> ParseTourney(int tourneyId)
    {
        var baseAddress = new Url("https://aetherhub.com/");
        var url = new Url(baseAddress, $"/Tourney/RoundTourney/{tourneyId}");
        var browsingContext = BrowsingContext.New(AngleSharp.Configuration.Default
            .WithDefaultLoader()
            .With(new DefaultHttpRequester("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36")));
        var document = await browsingContext.OpenAsync(url);
        var title = document.QuerySelector("title");

        if (!DateOnly.TryParse(title?.TextContent.Trim().Split(" - ").LastOrDefault(), out var date))
        {
            Console.WriteLine(document.ToHtml());
            throw new Exception($"Unable to parse tourney date: {title?.TextContent.Trim()}");
        }

        var links = document
            .QuerySelectorAll(".pagination a.page-link")
            .Select(a => a.GetAttribute("href")!)
            .ToArray();

        var rounds = new List<Round>();
        foreach (var link in links[1..^1])
        {
            var round = await browsingContext.OpenAsync(new Url(baseAddress, link));
            var rows = round.QuerySelectorAll("#matchList tbody tr");
            var games = new List<Game>();

            foreach (var row in rows)
            {
                var player1 = ExtractPlayerName(row.Children[1]);
                var player2 = ExtractPlayerName(row.Children[2]);

                var result = row.Children[3]
                    .TextContent
                    .Trim()
                    .Split(" - ")
                    .Select(int.Parse)
                    .ToArray();

                games.Add(new Game(player1, result[0], player2, result[1]));
            }

            rounds.Add(new Round([.. games]));
        }

        return (date, rounds.ToArray());
    }

    private static string ExtractPlayerName(INode node)
    {
        var text = HttpUtility.HtmlDecode(node.TextContent.Trim());
        return text.Split(" (").First();
    }

    public static bool TryParseAetherhubTourneyIdFromUrl(string url, out int tourneyId)
    {
        if (!AetherhubTourneyUrlRegex().IsMatch(url))
        {
            tourneyId = -1;
            return false;
        }

        tourneyId = int.Parse(AetherhubTourneyUrlRegex().Match(url).Groups[1].Value);
        return true;
    }

    [GeneratedRegex(@"^https:\/\/aetherhub\.com\/Tourney\/RoundTourney\/(\d+)(\?.*)?$")]
    private static partial Regex AetherhubTourneyUrlRegex();
}