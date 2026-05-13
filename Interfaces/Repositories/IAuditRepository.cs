namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IAuditRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<int> GetAuditLogsCountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
}
