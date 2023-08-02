using B1ServiceLayer.Attributes;
using B1ServiceLayer.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace B1ServiceLayer.Expressions;

/// <summary>
/// Packed expression that contains all SAP's query operations.
/// </summary>
public class SAPQueryExpression : Expression
{
    private Expression? _where;
    private int? _top;
    private int? _skip;
    private List<string>? _orders;
    private LambdaExpression? _select;
    private List<SAPAggregateExpression>? _aggregates;
    private IEnumerable<MemberExpression>? _groupBy;
    private LambdaExpression? _expand;
    private MethodInfo? _queryableExecutor;
    private Dictionary<Type, string>? _resources;

    public Expression? Where => _where;

    public int? Top => _top;

    public int? Skip => _skip;

    public List<string> Orders => _orders ??= new();

    public LambdaExpression? Select => _select;

    public List<SAPAggregateExpression> Aggregates => _aggregates ??= new();

    public IEnumerable<MemberExpression> GroupBy => _groupBy ??= Array.Empty<MemberExpression>();

    public LambdaExpression? Expand => _expand;

    internal MethodInfo? QueryableExecutor => _queryableExecutor;

    public bool IsPaginated { get; set; } = true;

    public bool IsCounting { get; set; } = false;

    public bool InlineCount { get; set; } = false;

    public Dictionary<Type, string> Resources => _resources ??= new();

    public override ExpressionType NodeType => ExpressionType.Extension;

    public override Type Type { get; }

    public Type ElementType { get; }

    public SAPQueryExpression(Type elementType, Type type)
    {
        Type = type;
        ElementType = elementType;
        AddResource(elementType);
    }

    internal SAPQueryExpression SetWhere(LambdaExpression where)
    {
        if (_where is not null)
            _where = AndAlso(_where, where.Body);
        else
            _where = where.Body;

        return this;
    }

    internal SAPQueryExpression SetTop(ConstantExpression topExpression)
    {
        _top = System.Convert.ToInt32(topExpression.Value);
        return this;
    }

    internal SAPQueryExpression SetTop(int value)
    {
        _top = value;
        return this;
    }

    internal SAPQueryExpression SetSkip(ConstantExpression skipExpression)
    {
        _skip = System.Convert.ToInt32(skipExpression.Value);
        return this;
    }

    internal SAPQueryExpression SetSkip(int value)
    {
        _skip = value;
        return this;
    }

    internal SAPQueryExpression AddOrderBy(MemberExpression memberExpression, string direction = "asc")
    {
        Orders.Add($"{memberExpression.Member.Name} {direction}");
        return this;
    }

    internal SAPQueryExpression SetSelect(LambdaExpression select)
    {
        _select = select;
        return this;
    }

    internal SAPQueryExpression Count()
    {
        IsCounting = true;
        return this;
    }

    internal SAPQueryExpression IgnorePagination()
    {
        IsPaginated = false;
        return this;
    }

    internal SAPQueryExpression HasInlineCount()
    {
        InlineCount = true;
        return this;
    }

    internal SAPQueryExpression AddAggregate(MemberExpression member, AggregateOperation operation, ConstantExpression constant)
    {
        Aggregates.Add(new(Type, member, operation, constant));
        return this;
    }

    internal SAPQueryExpression AddAggregate(SAPAggregateExpression aggregateExpression)
    {
        Aggregates.Add(aggregateExpression);
        return this;
    }

    internal SAPQueryExpression SetGroupBy(IEnumerable<MemberExpression> members)
    {
        _groupBy = members;
        return this;
    }

    internal SAPQueryExpression SetExpand(LambdaExpression expression)
    {
        _expand = expression;
        return this;
    }

    public static SAPQueryExpression From(SAPRootExpression root)
        => new(root.ElementType, root.Type);

    internal void AddResource(Type type)
    {
        var sapObject = (SAPEntityAttribute?)Attribute.GetCustomAttribute(type, typeof(SAPEntityAttribute))
            ?? throw new InvalidOperationException($"{type.Name} does not implement {nameof(SAPEntityAttribute)}");

        Resources[type] = sapObject.ResourceName;
    }

    internal SAPQueryExpression SetQueryableMethod(MethodInfo method)
    {
        _queryableExecutor = method;
        return this;
    }

    protected override Expression Accept(ExpressionVisitor visitor) => this;
}
