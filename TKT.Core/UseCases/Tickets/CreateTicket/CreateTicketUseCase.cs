using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Tickets.CreateTicket;

public sealed class CreateTicketUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyMembersGateway members,
    ICompanySubscriptionGateway subscriptions,
    ITicketsGateway tickets) : ICreateTicketUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanySubscriptionGateway _subscriptions = subscriptions;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<CreateTicketResult> ExecuteAsync(CreateTicketInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || !TicketAuthorizationPolicy.CanCreate(caller.Role))
            throw new ForbiddenException(TicketErrors.Forbidden);
        var companyId = caller.CompanyId;

        var title = TicketTitle.Create(input.Title);
        var priority = TicketPriority.CreateOrDefault(input.Priority);
        var source = TicketSource.CreateOrDefault(input.Source);

        var maxPerMonth = await _subscriptions.GetMaxTicketsPerMonthAsync(companyId);
        var usedThisMonth = await _tickets.CountCreatedThisMonthAsync(companyId);
        if (usedThisMonth >= maxPerMonth)
            throw new ConflictException(TicketErrors.QuotaExceeded);

        if (input.AssignedTo is { } assigneeId &&
            !TicketAuthorizationPolicy.CanBeAssigned(await _members.GetActiveRoleAsync(companyId, assigneeId)))
            throw new ValidationException(TicketErrors.AssigneeInvalid);

        var ticket = new Ticket
        {
            TicketId = Guid.CreateVersion7(),
            CompanyId = companyId,
            Title = title.Value,
            Description = input.Description,
            Status = TicketStatuses.Open,
            Priority = priority.Value,
            CreatedBy = input.CallerAccountId,
            AssignedTo = input.AssignedTo,
            CategoryId = input.CategoryId,
            Source = source.Value,
            DueDate = input.DueDate,
        };

        var created = await _tickets.CreateAsync(ticket);

        return new CreateTicketResult(ticket.TicketId, created.TicketNumber, ticket.Status, ticket.Priority, created.CreatedAt);
    }
}
