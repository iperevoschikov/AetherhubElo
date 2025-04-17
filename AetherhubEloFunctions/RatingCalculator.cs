namespace AetherhubEloFunctions;

public class RatingCalculator
{
    private const double K = 32;

    public static Dictionary<string, double> CalculateRatings(Tourney[] tourneys)
    {
        var ratings = new Dictionary<string, double>();
        foreach (var tourney in tourneys)
            foreach (var round in tourney.Rounds)
                foreach (var game in round.Games)
                {
                    if (IsBye(game.Player1) || IsBye(game.Player2))
                        continue;
                    var r1 = ratings.GetValueOrDefault(game.Player1, 1500);
                    var r2 = ratings.GetValueOrDefault(game.Player2, 1500);
                    var e1 = 1 / (1 + Math.Pow(10, (r2 - r1) / 400));
                    var e2 = 1 / (1 + Math.Pow(10, (r1 - r2) / 400));

                    var s1 = game.Player1WinCount > game.Player2WinCount
                        ? 1
                        : game.Player1WinCount < game.Player2WinCount
                            ? 0
                            : 0.5;

                    var s2 = 1 - s1;

                    ratings[game.Player1] = r1 + K * (s1 - e1);
                    ratings[game.Player2] = r2 + K * (s2 - e2);
                }

        return ratings;
    }

    private static bool IsBye(string player)
    {
        return ByeAliases.Any(x => player.ToLower().Equals(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private static string[] ByeAliases = ["bye", "бай"];
}