using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AetherhubEloFunctions.Commands;

public abstract class CommandProcessor(
    ITelegramBotClient BotClient,
    UserState UserState,
    string? CommandPrefix)
{
    public virtual bool CanProcess(UserState userState, string? command)
    {
        return userState == UserState && (command == null && CommandPrefix == null || command?[1..] == CommandPrefix);
    }

    protected Task Respond(MessageMeta chatMeta, string message, ReplyMarkup? replyMarkup = null)
    {
        return BotClient.SendMessage(
            chatMeta.ChatId,
            message,
            replyMarkup: replyMarkup
        );
    }

    public abstract Task ProcessCommand(MessageMeta chatMeta);

    public virtual int Order => 0;
}