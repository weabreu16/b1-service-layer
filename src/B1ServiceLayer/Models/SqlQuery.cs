#nullable disable
namespace B1ServiceLayer.Models;

public class SqlQuery
{
    public string SqlCode { get; set; }

    public string SqlName { get; set; }

    public string SqlText { get; set; }

#nullable enable
    public string? ParamList { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }
}
