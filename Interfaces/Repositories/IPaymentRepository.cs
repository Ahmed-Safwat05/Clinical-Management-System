namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByVisitIdAsync(int visitId);
    Task<decimal> GetTotalPaidByVisitIdAsync(int visitId);
}
