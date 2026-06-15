using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IEmailQueueRepository
{
    Task InsertAsync(EmailQueueRow email);
}
