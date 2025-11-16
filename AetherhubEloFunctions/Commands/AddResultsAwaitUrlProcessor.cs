using AetherhubEloFunctions.Aetherhub;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AetherhubEloFunctions.Commands;

public class AddResultsAwaitUrlProcessor(
    UsersStorage usersStorage,
    TourneysStorage tourneysStorage,
    ITelegramBotClient botClient
    ) : CommandProcessor(botClient, UserState.AddResultsAwaitUrl, null)
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
            if (
                !AetherhubTourneyParser.TryParseAetherhubTourneyIdFromUrl(
                    chatMeta.Text,
                    out var tourneyId
                )
            )
                await Respond(
                    chatMeta,
                    "Не смог разобрать урл. Он должен выглядеть вот так: https://aetherhub.com/Tourney/RoundTourney/38072"
                );
            else
            {
                var tourneys = tourneysStorage
                    .GetTourneys()
                    .Where(t => t.AetherhubId == tourneyId);
                if (await tourneys.AnyAsync())
                {
                    await Respond(chatMeta, "Турнир был добавлен ранее");
                }
                else
                {
                    var (date, rounds) =
                        await AetherhubTourneyParser.ParseTourney(tourneyId);
                    await tourneysStorage.WriteTourney(
                        new Tourney(
                            Guid.NewGuid(),
                            tourneyId,
                            userCommunix,
                            date,
                            rounds
                        )
                    );
                    await Respond(chatMeta, "Турнир сохранён");
                }
            }
        }

        await usersStorage.SetUserState(chatMeta.UserId, UserState.Default);
    }
}
