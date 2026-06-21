using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Api.Middleware;

public sealed class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IRequestTransaction transaction)
    {
        try
        {
            await _next(context);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
