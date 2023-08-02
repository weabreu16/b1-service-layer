using System.Linq.Expressions;

namespace B1ServiceLayer.Interfaces;

internal interface IAsyncQueryProvider: IQueryProvider
{
    Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
}
