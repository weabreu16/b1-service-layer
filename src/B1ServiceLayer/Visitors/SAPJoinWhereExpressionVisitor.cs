using System.Linq.Expressions;

namespace B1ServiceLayer.Visitors;

public class SAPJoinWhereExpressionVisitor : SAPWhereExpressionVisitor
{
    private readonly Dictionary<Type, string> _resources;

    public SAPJoinWhereExpressionVisitor(Dictionary<Type, string> resources) : base()
    {
        _resources = resources;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        Append($"{_resources![node.Member.DeclaringType!]}/{node.Member.Name}");

        return node;
    }
}
