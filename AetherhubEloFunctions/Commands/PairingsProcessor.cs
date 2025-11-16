using AetherhubEloFunctions.Aetherhub;
using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class PairingsProcessor(
    AetherhubTourneysFetcher tourneysFetcher,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "pairings")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        var recent = await tourneysFetcher
                                .FetchRecentTourneys()
                                .FirstOrDefaultAsync();
        if (recent != null)
        {
            var (_, Rounds) = await AetherhubTourneyParser.ParseTourney(
                recent.ExternalId
            );
            var lastRound = Rounds.LastOrDefault();
            if (lastRound != null)
            {
                await Respond(
                    chatMeta,
                    string.Join(
                        '\n',
                        lastRound.Games.Select(g =>
                            $"{g.Player1} - {g.Player2}"
                        )
                    )
                );
            }
            else
            {
                await Respond(chatMeta, "Не нашел ни одного раунда");
            }
        }
        else
        {
            await Respond(chatMeta, "Не нашел ни одного турнира");
        }
    }
}
