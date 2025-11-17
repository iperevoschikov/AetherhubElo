namespace AetherhubEloFunctions.YandexCloud;

public class YandexIAMTokenServiceOptions
{
    public string ServiceAccountId { get; set; }
    public string KeyId { get; set; }
    public string PrivateKey { get; set; }

    public bool ExternalObtaining { get; set; }
}