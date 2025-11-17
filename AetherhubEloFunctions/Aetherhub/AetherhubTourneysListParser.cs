using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;

namespace AetherhubEloFunctions.Aetherhub;

public static partial class AetherhubTourneysListParser
{
    public static async IAsyncEnumerable<TourneyMeta> Parse(string html)
    {
        var context = BrowsingContext.New(AngleSharp.Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));
        var rows = document.QuerySelector("#Tourneys")!.QuerySelectorAll("tr");
        foreach (var row in rows.Skip(1))
        {
            var cells = row.QuerySelectorAll("td");
            var href = cells[0].QuerySelector("a")!.GetAttribute("href")!;
            var startOfId = href.LastIndexOf('/') + 1;
            var id = int.Parse(href[startOfId..]);
            var name = cells[0].QuerySelector("a b")!.Text();
            var dateRaw = cells[2].Text();
            var dateNormalized = MyRegex().Replace(dateRaw, "$1 $2 $3");
            var date = DateOnly.Parse(dateNormalized);
            yield return new TourneyMeta(id, name, date);
        }
    }

    [GeneratedRegex(@"(\w+) (\d+)\w+ (\d+)")]
    private static partial Regex MyRegex();
}