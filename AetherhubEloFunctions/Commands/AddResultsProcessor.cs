using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class AddResultsProcessor(
    UsersStorage usersStorage,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "addresults")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        if (await usersStorage.GetUserCommunix(chatMeta.UserId) == null)
        {
            await Respond(chatMeta, "Сначала нужно выбрать твой комуникс /communix");
        }
        else
        {
            await usersStorage.SetUserState(
                chatMeta.UserId,
                UserState.AddResultsAwaitUrl
            );

            await Respond(chatMeta, "Отправь ссылку на турнир Aetherhub");
        }
    }
}