using System.Text.Json.Serialization;

namespace B1ServiceLayer.Models;

public partial class SapResponse<T>
{
    [JsonPropertyName("@odata.context")]
    public Uri? OdataContext { get; set; }

    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new List<T>();

    [JsonPropertyName("@odata.nextLink")]
    public string? OdataNextLink { get; set; }
}
