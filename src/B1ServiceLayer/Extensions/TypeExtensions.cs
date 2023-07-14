using System.Reflection;

namespace B1ServiceLayer.Extensions;

public static class TypeExtensions
{
    public static object? InvokeMember(this Type type, string name, BindingFlags flags)
        => type.InvokeMember(name, flags, null, null, null);

    public static bool IsNumeric(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }
}
