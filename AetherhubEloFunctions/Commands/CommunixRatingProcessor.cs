using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class CommunixRatingProcessor(
    TourneysStorage tourneysStorage,
    UsersStorage usersStorage,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "rating")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        var userCommunix = await usersStorage.GetUserCommunix(chatMeta.UserId);
        if (userCommunix == null)
        {
            await Respond(chatMeta, "Сначала нужно выбрать твой комуникс /communix");
        }
        else
        {
            await Respond(
                chatMeta,
                RatingPrinter.PrintRatings(
                    RatingCalculator.CalculateRatings(
                        await tourneysStorage
                            .GetTourneys()
                            .Where(t => t.Communix == userCommunix)
                            .OrderBy(t => t.Date)
                            .ToArrayAsync()
                    )
                )
            );
        }
    }
}