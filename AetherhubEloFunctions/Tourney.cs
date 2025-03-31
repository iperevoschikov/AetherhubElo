namespace AetherhubEloFunctions;

public record Tourney(
    Guid Id,
    int AetherhubId,
    string Communix,
    DateOnly Date,
    Round[] Rounds);