using B1ServiceLayer.Enums;

namespace B1ServiceLayer.Extensions;

public static class AggregateOperationExtensions
{
    public static string GetValue(this AggregateOperation operation)
    {
        return operation switch
        {
            AggregateOperation.Sum => "sum",
            AggregateOperation.Average => "average",
            AggregateOperation.Max => "max",
            AggregateOperation.Min => "min",
            AggregateOperation.CountDistinct => "countdistinct",
            AggregateOperation.Count => "count",
            _ => throw new NotImplementedException(),
        };
    }
}
