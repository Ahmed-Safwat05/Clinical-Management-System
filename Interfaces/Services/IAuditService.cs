namespace ClinicManagementSystem.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(
        AuditActionType actionType,
        string entityName,
        string description,
        int? entityId = null);

    Task LogLoginAsync(string username);
    Task LogLogoutAsync(string username);

    Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null);

    Task<int> GetAuditLogsCountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null);
}
