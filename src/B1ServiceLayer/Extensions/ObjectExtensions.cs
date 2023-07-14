namespace B1ServiceLayer.Extensions;

public static class ObjectExtensions
{
    public static bool In(this object? obj, params object?[] values)
        => values.Contains(obj);
}
