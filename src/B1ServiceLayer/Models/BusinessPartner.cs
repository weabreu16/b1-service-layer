using System.Text.Json.Serialization;
using B1ServiceLayer.Enums;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer.Models;

public class BusinessPartner: ISAPObject
{
    public string? CardCode { get; set; }

    public string? CardName { set; get; }

    public string? FederalTaxID { get; set; }

    public BoCardTypes? CardType { get; set; }

    public int? GroupCode { get; set; }

    public string DebitorAccount { get; set; }

    public string GetResourceName() => "BusinessPartners";
}

public class CustomBusinessPartner: BusinessPartner
{
    [JsonPropertyName("U_NCF")]
    public string? NCF { get; set; }
}

public class FederalId
{
    public string? FederalTaxID { set; get; }
}
