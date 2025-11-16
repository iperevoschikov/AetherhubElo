using Telegram.Bot;

namespace AetherhubEloFunctions.Commands;

public class HelpProcessor(
    ITelegramBotClient botClient)
    : CommandProcessor(botClient, UserState.Default, "default")
{
    public override bool CanProcess(UserState userState, string? command)
    {
        return userState == UserState.Default;
    }
    public override async Task ProcessCommand(MessageMeta chatMeta)
    {
        await Respond(chatMeta,
            "Это бот для подсчета рейтинга игроков MTG на локальных турнирах.\n" +
            "Доступные команды:\n" +
            "/communix - выбрать твой комуникс\n" +
            "/addresults - добавить результаты турнира\n" +
            "/rating - показать рейтинг игроков\n" +
            "/global - показать глобальный рейтинг игроков\n" +
            "/pairings - показать паринги самого последнего известного тура"
        );
    }

    public override int Order => -10;
}