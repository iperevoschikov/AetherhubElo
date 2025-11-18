namespace AetherhubEloFunctions.YandexCloud;
public interface IYandexIAMTokenProvider
{
    public Task<string> GetToken();
}