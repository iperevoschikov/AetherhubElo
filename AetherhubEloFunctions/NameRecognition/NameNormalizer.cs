namespace AetherhubEloFunctions.NameRecognition;

public static class NameNormalizer
{
    private static readonly HashSet<string> RussianNames = new(
        File.ReadAllLines("NameRecognition/male_names_rus.txt")
            .Concat(File.ReadAllLines("NameRecognition/female_names_rus.txt")),
        StringComparer.InvariantCultureIgnoreCase);

    private static readonly Dictionary<string, string> ShortNames = new()
    {
        { "Юра", "Юрий" },
        { "Стас", "Станислав" },
        { "Данила", "Данил" },
        { "Лиза", "Елизавета" },
    };

    private static readonly Dictionary<string, string> SurnameAliases = new()
    {
        { "Ллановаров", "Лановаров" },
    };

    public static string NormalizePlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        name = name.Trim();
        name = name.Replace('ё', 'е');
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return name;

        var first = Capitalize(parts[0]);
        var second = Capitalize(parts[1]);

        if (ShortNames.TryGetValue(first, out var fullName) && !ShortNames.ContainsKey(second))
        {
            first = fullName;
        }
        else if (ShortNames.TryGetValue(second, out fullName) && !ShortNames.ContainsKey(first))
        {
            second = fullName;
        }

        if (SurnameAliases.TryGetValue(first, out var alias))
        {
            first = alias;
        }
        else if (SurnameAliases.TryGetValue(second, out alias))
        {
            second = alias;
        }

        return (!RussianNames.Contains(first) && RussianNames.Contains(second))
            ? $"{first} {second}"
            : $"{second} {first}";
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s[1..].ToLower();
    }
}