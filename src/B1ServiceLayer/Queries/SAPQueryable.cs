using B1ServiceLayer.Expressions;
using System.Collections;
using System.Linq.Expressions;

namespace B1ServiceLayer.Queries;

public class SAPQueryable<TSAPEntity>: IQueryable<TSAPEntity>, IOrderedQueryable<TSAPEntity>
{
    public Type ElementType => typeof(TSAPEntity);

    public Expression Expression { get; }

    public IQueryProvider Provider { get; }

    public SAPQueryable(IQueryProvider provider, string resourceName)
    {
        Provider = provider;
        Expression = new SAPRootExpression(ElementType, resourceName);
    }

    public SAPQueryable(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    public IEnumerator<TSAPEntity> GetEnumerator()
        => Provider.Execute<IEnumerable<TSAPEntity>>(Expression).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => Provider.Execute<IEnumerable>(Expression).GetEnumerator();
}
