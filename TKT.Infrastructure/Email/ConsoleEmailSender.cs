using Microsoft.Extensions.Logging;
using TKT.Core.Abstractions;

namespace TKT.Infrastructure.Email;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        logger.LogInformation("EMAIL -> {To} | {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
