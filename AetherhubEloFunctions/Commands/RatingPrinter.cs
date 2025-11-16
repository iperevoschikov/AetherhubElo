namespace AetherhubEloFunctions.Commands;

public static class RatingPrinter
{
    public static string PrintRatings(Dictionary<string, double> ratings)
    {
        return ratings.Count != 0
            ? string.Join(
                '\n',
                ratings
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => $"{kvp.Key}: {kvp.Value:N0}")
            )
            : "Пока не было добавлено никаких результатов (/addresults)";
    }
}