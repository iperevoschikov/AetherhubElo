using System.Text.Json.Serialization;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubFetchPublicTourneysResponse
{
    [JsonPropertyName("model")]
    public AetherhubTourney[] Model { get; set; }
}