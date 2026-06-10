namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IVisitRepository : IRepository<Visit>
{
    Task<IReadOnlyList<Visit>> GetRecentAsync(int count = 50);
    Task<Visit?> GetDetailsAsync(int id);
    Task<decimal> GetTotalIncomeAsync();
    Task<decimal> GetIncomeByDateAsync(DateTime date);
    Task<int> CountPatientsByVisitDateAsync(DateTime date);

    Task<IReadOnlyList<Visit>> GetPatientVisitsAsync(int patientId, int currentVisitId);
}
