namespace TKT.Core.Abstractions;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}
