using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AetherhubEloFunctions.Commands;

public class CommunixSelectProcessor(
    UsersStorage usersStorage,
    CommunixesStorage communixesStorage,
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "communix")
{
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        await usersStorage
            .SetUserState(chatMeta.UserId, UserState.SelectCommunix);

        var communixes = await communixesStorage.GetAll();

        await Respond(
            chatMeta,
            "Выбери комуникс",
            replyMarkup: new InlineKeyboardMarkup(
                communixes
                    .Select(c =>
                        InlineKeyboardButton.WithCallbackData(c.Name, c.Id)
                    )
                    .ToArray()
            )
        );
    }
}