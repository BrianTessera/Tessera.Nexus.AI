using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IAuditRepository
{
    Task<long> LogEventAsync(
        ApplicationEventLog logEntry,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationEventLog>> GetRecentEventsAsync(
        int top = 100,
        CancellationToken cancellationToken = default);
}