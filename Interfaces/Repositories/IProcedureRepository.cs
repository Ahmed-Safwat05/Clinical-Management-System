namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IProcedureRepository : IRepository<Procedure>
{
    Task<IReadOnlyList<Procedure>> GetOrderedAsync();
}
