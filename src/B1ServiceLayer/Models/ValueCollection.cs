using System.Text.Json.Serialization;

namespace B1ServiceLayer.Models;

public class ValueCollection<T>
{
    [JsonPropertyName("@odata.count")]
    public int Count { get; set; }

    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = Array.Empty<T>().ToList();

    [JsonPropertyName("@odata.nextLink")]
    public string? NextLink { get; set; }
}
