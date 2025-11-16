using Telegram.Bot.Types;

namespace AetherhubEloFunctions;

public record MessageMeta(long UserId, string ChatId, string Text)
{
    public static MessageMeta FromMessage(Message message)
        => new(
            message.From!.Id,
            message.Chat.Id.ToString(),
            message.Text ?? string.Empty);
}