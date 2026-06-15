using TKT.Core.IGateways;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class EmailQueueMapper
{
    public static EmailQueueRow ToRow(this QueuedEmail email, Guid queueId)
    {
        return new EmailQueueRow
        {
            QueueId = queueId,
            CompanyId = email.CompanyId,
            RecipientEmail = email.RecipientEmail,
            Subject = email.Subject,
            BodyHtml = email.BodyHtml,
            EmailType = email.EmailType,
            EntityType = email.EntityType,
            EntityId = email.EntityId,
        };
    }
}
