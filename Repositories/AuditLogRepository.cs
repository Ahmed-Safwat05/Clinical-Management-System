namespace ClinicManagementSystem.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null)
    {
        return await BuildQuery(username, actionType, startDate, endDate, keyword)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((NormalizePageNumber(pageNumber) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .ToListAsync();
    }

    public async Task<int> CountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null)
    {
        return await BuildQuery(username, actionType, startDate, endDate, keyword).CountAsync();
    }

    private IQueryable<AuditLog> BuildQuery(
        string? username,
        AuditActionType? actionType,
        DateTime? startDate,
        DateTime? endDate,
        string? keyword)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(x => x.Username.Contains(username));
        }

        if (actionType.HasValue)
        {
            query = query.Where(x => x.ActionType == actionType);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt < endDate.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Description.Contains(keyword) ||
                x.EntityName.Contains(keyword) ||
                (x.IpAddress != null && x.IpAddress.Contains(keyword)));
        }

        return query;
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return pageNumber < 1 ? 1 : pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize is < 1 or > 200 ? 20 : pageSize;
    }
}
