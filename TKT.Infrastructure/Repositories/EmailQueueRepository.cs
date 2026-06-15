using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class EmailQueueRepository(IDbSession db) : IEmailQueueRepository
{
    private readonly IDbSession _db = db;

    public Task InsertAsync(EmailQueueRow email)
    {
        const string sql = """
                           INSERT INTO email_queue (queue_id, company_id, recipient_email, subject,
                                                    body_html, email_type, entity_type, entity_id)
                           VALUES (@QueueId, @CompanyId, @RecipientEmail, @Subject,
                                   @BodyHtml, @EmailType, @EntityType, @EntityId);
                           """;
        return _db.ExecuteAsync(sql, email);
    }
}
