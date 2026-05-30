namespace ClinicManagementSystem.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByVisitIdAsync(int visitId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.VisitId == visitId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPaidByVisitIdAsync(int visitId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.VisitId == visitId)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;
    }
}
