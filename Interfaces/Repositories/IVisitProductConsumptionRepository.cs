namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IVisitProductConsumptionRepository : IRepository<VisitProductConsumption>
{
    Task<IReadOnlyList<VisitProductConsumption>> GetByVisitAsync(int visitId);
    Task<decimal> GetTotalCostAsync(int visitId);
}
