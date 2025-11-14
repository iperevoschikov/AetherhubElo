using System.Text.Json.Serialization;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubFetchPublicTourneysResponse
{
    [JsonPropertyName("model")]
    public required AetherhubTourney[] Model { get; set; }
}

