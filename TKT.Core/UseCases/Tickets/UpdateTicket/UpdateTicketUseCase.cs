using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Tickets.UpdateTicket;

public sealed class UpdateTicketUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyMembersGateway members,
    ITicketsGateway tickets) : IUpdateTicketUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<TicketDetail> ExecuteAsync(UpdateTicketInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || !TicketAuthorizationPolicy.CanModify(caller.Role))
            throw new ForbiddenException(TicketErrors.Forbidden);
        var companyId = caller.CompanyId;

        var current = await _tickets.GetByIdAsync(companyId, input.TicketId);
        if (current is null)
            throw new NotFoundException(TicketErrors.NotFound);

        var status = ResolveStatus(input.Status, current.Status);
        var priority = input.Priority is null ? null : TicketPriority.CreateOrDefault(input.Priority).Value;

        if (input.AssignedTo is { } assigneeId &&
            !TicketAuthorizationPolicy.CanBeAssigned(await _members.GetActiveRoleAsync(companyId, assigneeId)))
            throw new ValidationException(TicketErrors.AssigneeInvalid);

        if (status is null && input.AssignedTo is not null)
            status = TicketWorkflow.NextStatusOnAssignment(current.Status);

        var update = new TicketUpdate(companyId, input.TicketId, status, priority,
            input.AssignedTo, input.CategoryId, input.DueDate);

        var updated = await _tickets.UpdateAsync(update);
        if (updated is null)
            throw new NotFoundException(TicketErrors.NotFound);

        return updated;
    }

    private static string? ResolveStatus(string? rawStatus, string currentStatus)
    {
        if (rawStatus is null)
            return null;

        var target = rawStatus.Trim().ToLowerInvariant();
        if (!TicketWorkflow.IsKnownStatus(target))
            throw new ValidationException(TicketErrors.StatusInvalid);

        if (!TicketWorkflow.CanTransition(currentStatus, target))
            throw new ConflictException(TicketErrors.InvalidTransition);

        return target;
    }
}
