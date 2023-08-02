using B1ServiceLayer.Models;
using B1ServiceLayer.Queries;
using RestSharp;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    private const string _resourceName = "SQLQueries";

    public static async Task<List<SqlQuery>> GetSqlQueriesAsync(this B1Service service, CancellationToken cancellationToken = default)
        => (await service.ExecuteAsync<SAPResponse<List<SqlQuery>>>(new(_resourceName), cancellationToken))!.Value!;

    public static async Task<SqlQuery> GetSqlQueryAsync(this B1Service service, string sqlCode, CancellationToken cancellationToken = default)
        => (await service.ExecuteAsync<SqlQuery>(new RestRequest($"{_resourceName}('{sqlCode}')"), cancellationToken))!;

    public static async Task<SqlQuery> CreateSqlQueryAsync(this B1Service service, SqlQuery query, CancellationToken cancellationToken = default)
        => (await service.ExecuteAsync<SqlQuery>(new RestRequest(_resourceName, Method.Post).AddBody(query), cancellationToken))!;

    public static async Task UpdateSqlQueryAsync(this B1Service service, string sqlCode, object query, CancellationToken cancellationToken = default)
        => await service.RequestAsync(new RestRequest($"{_resourceName}('{sqlCode}')", Method.Patch).AddBody(query), cancellationToken);

    public static async Task DeleteSqlQueryAsync(this B1Service service, string sqlCode, CancellationToken cancellationToken = default)
        => await service.RequestAsync(new RestRequest($"{_resourceName}('{sqlCode}')", Method.Delete), cancellationToken);

    public static async Task<List<TResult>> ExecuteSqlQueryAsync<TResult>(
        this B1Service service,
        string sqlCode,
        int? skip = null,
        bool paginated = true,
        string? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sqlCode, nameof(sqlCode));

        var request = new RestRequest($"{_resourceName}('{sqlCode}')/List", Method.Post);

        if (skip.HasValue)
            request.AddQueryParameter("$skip", skip.Value);

        if (!paginated)
            request.AddHeader("Prefer", "odata.maxpagesize=0");

        if (parameters is not null)
            request.AddBody(new { ParamList = parameters });

        return (await service.ExecuteAsync<SAPResponse<List<TResult>>>(request, cancellationToken))!.Value!;
    }

    public static List<SqlQuery> GetSqlQueries(this B1Service service)
        => service.GetSqlQueriesAsync().GetAwaiter().GetResult();

    public static SqlQuery GetSqlQuery(this B1Service service, string sqlCode)
        => service.GetSqlQueryAsync(sqlCode).GetAwaiter().GetResult();

    public static SqlQuery CreateSqlQuery(this B1Service service, SqlQuery query)
        => service.CreateSqlQueryAsync(query).GetAwaiter().GetResult();

    public static void UpdateSqlQuery(this B1Service service, string sqlCode, object query)
        => service.UpdateSqlQueryAsync(sqlCode, query).GetAwaiter().GetResult();

    public static void DeleteSqlQuery(this B1Service service, string sqlCode)
        => service.DeleteSqlQueryAsync(sqlCode).GetAwaiter().GetResult();

    public static List<TResult> ExecuteSqlQuery<TResult>(
        this B1Service service,
        string sqlCode,
        int? skip = null,
        bool paginated = true,
        string? parameters = null)
            => service.ExecuteSqlQueryAsync<TResult>(sqlCode, skip, paginated, parameters).GetAwaiter().GetResult();
}
