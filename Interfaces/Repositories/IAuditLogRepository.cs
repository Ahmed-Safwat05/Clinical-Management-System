namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);

    Task<IReadOnlyList<AuditLog>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null);

    Task<int> CountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null);
}
