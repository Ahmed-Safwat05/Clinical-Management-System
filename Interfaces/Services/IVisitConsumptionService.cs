namespace ClinicManagementSystem.Interfaces.Services;

public interface IVisitConsumptionService
{
    Task<bool> ConsumeProductAsync(int visitId, int productId, int quantity);
    Task<IReadOnlyList<VisitProductConsumption>> GetVisitConsumptionsAsync(int visitId);
    Task<decimal> GetTotalConsumptionCostAsync(int visitId);
}
