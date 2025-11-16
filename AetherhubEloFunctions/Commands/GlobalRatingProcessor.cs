using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class GlobalRatingProcessor(
    TourneysStorage tourneysStorage,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "global")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        await Respond(
            chatMeta,
            RatingPrinter.PrintRatings(
                RatingCalculator.CalculateRatings(
                    await tourneysStorage
                        .GetTourneys()
                        .OrderBy(t => t.Date)
                        .ToArrayAsync()
                )
            )
        );
    }
}