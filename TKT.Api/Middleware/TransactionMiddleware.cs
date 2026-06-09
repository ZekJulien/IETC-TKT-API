using TKT.Infrastructure.Persistence;

namespace TKT.Api.Middleware;

public sealed class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, DbSession session)
    {
        try
        {
            await _next(context);
            await session.CommitAsync();
        }
        catch
        {
            await session.RollbackAsync();
            throw;
        }
    }
}
