using System.Text.Json.Serialization;

namespace B1ServiceLayer.Models;

public partial class SapResponse<TValue>
{
    [JsonPropertyName("@odata.context")]
    public Uri? OdataContext { get; set; }

    [JsonPropertyName("value")]
    public ICollection<TValue> Value { get; set; } = Array.Empty<TValue>();

    [JsonPropertyName("@odata.nextLink")]
    public string? OdataNextLink { get; set; }
}
