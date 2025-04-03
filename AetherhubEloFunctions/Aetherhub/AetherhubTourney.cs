using System.Text.Json.Serialization;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourney
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}