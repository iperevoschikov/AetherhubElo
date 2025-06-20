using AetherhubEloFunctions.NameRecognition;

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

                    var player1 = NameNormalizer.NormalizePlayerName(game.Player1);
                    var player2 = NameNormalizer.NormalizePlayerName(game.Player2);

                    if (IsBye(player1) || IsBye(player2))
                        continue;
                    var r1 = ratings.GetValueOrDefault(player1, 1500);
                    var r2 = ratings.GetValueOrDefault(player2, 1500);
                    var e1 = 1 / (1 + Math.Pow(10, (r2 - r1) / 400));
                    var e2 = 1 / (1 + Math.Pow(10, (r1 - r2) / 400));

                    var s1 = game.Player1WinCount > game.Player2WinCount
                        ? 1
                        : game.Player1WinCount < game.Player2WinCount
                            ? 0
                            : 0.5;

                    var s2 = 1 - s1;

                    ratings[player1] = r1 + K * (s1 - e1);
                    ratings[player2] = r2 + K * (s2 - e2);
                }

        return ratings;
    }

    private static bool IsBye(string player)
    {
        return ByeAliases.Any(x => player.ToLower().Equals(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private static readonly string[] ByeAliases = ["bye", "бай"];
}