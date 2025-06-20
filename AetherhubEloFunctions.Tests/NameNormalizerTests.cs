using AetherhubEloFunctions.NameRecognition;

namespace AetherhubEloFunctions.Tests;

public class NameNormalizerTests
{
    [TestCase("Иван Иванов", "Иванов Иван")]
    [TestCase("Иванов Иван", "Иванов Иван")]
    [TestCase("алексей Смирнов", "Смирнов Алексей")]
    [TestCase("Смирнов Алексей", "Смирнов Алексей")]
    [TestCase("  Иван   Иванов  ", "Иванов Иван")]
    [TestCase("", "")]
    [TestCase("Ольга Петрова", "Петрова Ольга")]
    [TestCase("Петрова Ольга", "Петрова Ольга")]
    public void NormalizePlayerName_ReturnsExpected(string input, string expected)
    {
        var result = NameNormalizer.NormalizePlayerName(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void NormalizePlayerName_OneWord_ReturnsAsIs()
    {
        Assert.That(NameNormalizer.NormalizePlayerName("Иванов"), Is.EqualTo("Иванов"));
    }

    [Test]
    public void NormalizePlayerName_ThreeWords_ReturnsAsIs()
    {
        Assert.That(NameNormalizer.NormalizePlayerName("Иван Иванов Иванович"), Is.EqualTo("Иван Иванов Иванович"));
    }

    [Test]
    public void RealLifeTest()
    {
        string[] players = [
            "Купельский Артем",
            "Милованов Андрей",
            "Попов Михаил",
            "Ямщиков Юрий",
            "Безногов Дмитрий",
            "Кокорин Иван",
            "Новиков Дмитрий",
            "Целищев Роман",
            "Орлов Денис",
            "Разумов Артем",
            "Дубинский Никита",
            "Гришенков Николай",
            "Сафин Роберт",
            "Воинков Вячеслав",
            "Завадкин Иван",
            "Сотников Артем",
            "Юра Ямщиков",
            "Горбачев Максим",
            "Иванов Данил",
            "Печников Владислав",
            "Ратенков Кирилл",
            "Василий Наборнов",
            "Коновалов Роман",
            "Попов Стас",
            "Прошин Станислав",
            "Курсанов Александр",
            "Попов Станислав",
            "Согомонян Сергей",
            "Рассказов Никита",
            "Лановаров Иван",
            "Власов Максим",
            "Планков Борис",
            "Гавриков Кирилл",
            "Осинцев Александр",
            "Козлов Александр",
            "Афиногенов Андрей",
            "Корниенко Евгений",
            "Газейкин Егор",
            "Кузницын Артем",
            "Перевощиков Иван",
            "Будаква Александр",
            "Радевич Андрей",
            "Кригер Владислав",
            "Сигов Иван",
            "Панкрашин Святослав",
            "Карабанов Георгий",
            "Демин Данил",
            "Вторушин Никита",
            "Аудилов Евгений",
            "Васильев Тарас",
            "Колотовкин Матвей",
            "Козельский Андрей",
            "Руденко Константин",
            "Снигирев Алексей",
            "Леонид Будаква",
            "Данил Дёмин",
            "Иванов Данила",
            "Грабовец Леонид",
            "Ллановаров Иван",
            "Подкопаев Алексей",
            "Илькив Антон",
            "Неслов Сергей",
            "Семенов Виталий",
            "Жильцов Артем",
            "Демахин Иван",
            "Пинчуков Александр",
            "Снигирева Лиза",
            "Поколодный Никита",
            "Постовалов Николай",
            "Паньшин Евгений",
            "Фадин Илья",
            "Хисамова Вероника",
            "Бирюков Павел",
            "Малых Денис",
            "Зенков Михаил",
        ];
        var normalized = players
            .Select(NameNormalizer.NormalizePlayerName)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        foreach(var name in normalized)
        {
            Console.WriteLine(name);
        }

        Assert.That(normalized, Has.Length.EqualTo(71));
    }
}