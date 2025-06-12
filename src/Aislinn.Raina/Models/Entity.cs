
using System.Text.Json.Serialization;

public class Entity
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}