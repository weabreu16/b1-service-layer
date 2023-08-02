using System.Text.Json.Serialization;

namespace B1ServiceLayer.Models;

public partial class SAPResponse<TValue>
{
    [JsonPropertyName("@odata.context")]
    public Uri? OdataContext { get; set; }

    [JsonPropertyName("value")]
    public TValue? Value { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? OdataNextLink { get; set; }
}
