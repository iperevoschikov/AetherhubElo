using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Tests;

public class DebugTests
{
    [Test]
    public async Task Debug()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureStorage()
            .BuildServiceProvider();

        var tourneysStorage = serviceProvider.GetRequiredService<TourneysStorage>();

        var tourneys = await tourneysStorage.GetTourneys().ToArrayAsync();
        var tourney = tourneys.Single(t => t.AetherhubId == 58245) with
        {
            Rounds = [
    new Round([

            new Game("Милованов Андрей",2,"Панкрашин Святослав",1),
            new Game("Сафин Роберт",1,"Nishimura Hideo",2),
            new Game("Воинков Вячеслав",1,"Новиков Дмитрий",1),
            new Game("Прошин Станислав",2,"Васильев Тарас",0),
            new Game("Кокорин Иван",0,"Целищев Роман",2),
            new Game("Леонид Грабовец",2,"BYE",0),]),
            new Round([
                            new Game("Милованов Андрей",1,"Целищев Роман",1),
            new Game("Nishimura Hideo",2,"Прошин Станислав",1),
            new Game("Леонид Грабовец",1,"Воинков Вячеслав",2),
            new Game("Новиков Дмитрий",0,"Кокорин Иван",2),
            new Game("Сафин Роберт",2,"Васильев Тарас",1),
            new Game("Панкрашин Святослав",2,"BYE",0),
            ]),
            new Round([
                            new Game("Nishimura Hideo",2,"Целищев Роман",1),
            new Game("Милованов Андрей",2,"Воинков Вячеслав",0),
            new Game("Прошин Станислав",2,"Сафин Роберт",1),
            new Game("Панкрашин Святослав",0,"Кокорин Иван",2),
            new Game("Леонид Грабовец",0,"Новиков Дмитрий",2),
            new Game("Васильев Тарас",2,"BYE",0),
            ])]
        };
        await tourneysStorage.WriteTourney(tourney);
    }
}