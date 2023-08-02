using System.Linq.Expressions;

namespace B1ServiceLayer.Expressions;

/// <summary>
/// Base expression that provides information about SAP entity.
/// </summary>
public class SAPRootExpression: Expression
{
    public virtual Type ElementType { get; }

    public virtual string ResourceName { get; }

    public override Type Type { get; }

    public override ExpressionType NodeType => ExpressionType.Extension;

    public SAPRootExpression(Type elementType, string resourceName)
    {
        ElementType = elementType;
        Type = typeof(IQueryable<>).MakeGenericType(elementType);
        ResourceName = resourceName;
    }
}
