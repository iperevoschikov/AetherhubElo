namespace AetherhubEloFunctions;

public class CommunixGuesser(CommunixesStorage communixesStorage)
{
    public async Task<string?> GuessCommunix(TourneyMeta tourney)
    {
        var communixes = await communixesStorage.GetAll();
        var possibleCommunix = communixes.FirstOrDefault(c => c.Weekday == tourney.Date.DayOfWeek);
        if (possibleCommunix != null && possibleCommunix.Aliases.Any(a => tourney.Name.Contains(a, StringComparison.InvariantCultureIgnoreCase)))
            return possibleCommunix.Id;
        return null;
    }
}