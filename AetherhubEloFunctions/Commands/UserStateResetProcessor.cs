using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class UserStateResetProcessor(
    ILogger<UserStateResetProcessor> logger,
    UsersStorage usersStorage,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "reset")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        logger.LogWarning("Something went wrong");
        await usersStorage.SetUserState(chatMeta.UserId, UserState.Default);
        await Respond(chatMeta, "Я тебя не понимаю, давай начнем сначала");
    }

    public override int Order => -100;
}