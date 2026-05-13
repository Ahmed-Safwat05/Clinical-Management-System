namespace ClinicManagementSystem.Repositories;

public class AuditRepository : Repository<AuditLog>, IAuditRepository
{
    public AuditRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = DbSet.AsNoTracking();

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
            query = query.Where(x => x.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt < endDate.Value.AddDays(1));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAuditLogsCountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = DbSet.AsNoTracking();

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
            query = query.Where(x => x.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt < endDate.Value.AddDays(1));
        }

        return await query.CountAsync();
    }
}
