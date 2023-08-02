using B1ServiceLayer.Enums;
using B1ServiceLayer.Extensions;
using System.Linq.Expressions;

namespace B1ServiceLayer.Expressions;

/// <summary>
/// Expression that provides information about a SAP's aggregate operation.
/// </summary>
public class SAPAggregateExpression : Expression
{
    public MemberExpression Member { get; }

    public AggregateOperation Operation { get; }

    public ConstantExpression ResultFieldName { get; }

    public override ExpressionType NodeType => ExpressionType.Extension;

    public override Type Type { get; }

    public SAPAggregateExpression(Type type, MemberExpression member, AggregateOperation operation, ConstantExpression resultFieldName)
    {
        Type = type;
        Member = member;
        Operation = operation;
        ResultFieldName = resultFieldName;
    }

    public override string ToString()
        => $"{Member.Member.Name} with {Operation.GetValue()} as {ResultFieldName.Value ?? Member.Member.Name}";
}
